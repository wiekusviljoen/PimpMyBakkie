using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 22 * 1024 * 1024);
builder.Services.AddHttpClient("openai", client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/");
    client.Timeout = TimeSpan.FromMinutes(4);
});
var app = builder.Build();

app.MapGet("/api/health", (IConfiguration config) => Results.Ok(new
{
    service = "PimpMyBakkie image service",
    status = "online",
    imageEditingConfigured = !string.IsNullOrWhiteSpace(config["OPENAI_API_KEY"]),
    vehicleIdentificationConfigured = !string.IsNullOrWhiteSpace(config["OPENAI_API_KEY"]),
    note = "Image generation uses the configured OpenAI API key and may incur API charges."
}));

app.MapPost("/api/identify", async (HttpRequest request, IConfiguration config, IHttpClientFactory factory, CancellationToken ct) =>
{
    if (!await IsAuthorized(request, config, ct)) return Results.Unauthorized();
    var apiKey = config["OPENAI_API_KEY"];
    if (string.IsNullOrWhiteSpace(apiKey))
        return Results.Problem("The server is missing OPENAI_API_KEY. Set it as an environment variable; do not put it in the Unity app.", statusCode: 503);

    IFormCollection form;
    try { form = await request.ReadFormAsync(ct); }
    catch { return Results.BadRequest(new { error = "Send the photo as multipart/form-data in the 'image' field." }); }

    var image = form.Files.GetFile("image");
    if (image is null || image.Length == 0) return Results.BadRequest(new { error = "A vehicle photo is required." });
    if (image.Length > 15 * 1024 * 1024) return Results.BadRequest(new { error = "Photo must be 15 MB or smaller." });
    if (!IsImage(image.ContentType)) return Results.BadRequest(new { error = "Use a JPEG, PNG, or WebP photo." });

    await using var stream = image.OpenReadStream();
    using var memory = new MemoryStream();
    await stream.CopyToAsync(memory, ct);
    var dataUrl = "data:" + image.ContentType + ";base64," + Convert.ToBase64String(memory.ToArray());

    var payload = new
    {
        model = config["OPENAI_VISION_MODEL"] ?? "gpt-4.1-mini",
        temperature = 0.1,
        response_format = new { type = "json_object" },
        messages = new object[]
        {
            new { role = "system", content = "You identify road vehicles from photographs. Be conservative. Never invent an exact variant if it cannot be read or distinguished. Return valid JSON with keys make, model, year (integer or null), variant, bodyStyle, confidence (0 to 1), visibleClues (array of strings), uncertainty. If uncertain, use null or 'Unknown'. A photo does not reliably prove model year or mechanical trim." },
            new { role = "user", content = new object[]
                {
                    new { type = "text", text = "Identify this bakkie/pickup. Return the most likely make and model, but clearly mark uncertain year and trim. Do not assume it is a Hilux." },
                    new { type = "image_url", image_url = new { url = dataUrl, detail = "high" } }
                }
            }
        }
    };

    using var client = factory.CreateClient("openai");
    using var message = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions");
    message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    message.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
    using var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, ct);
    var responseText = await response.Content.ReadAsStringAsync(ct);
    if (!response.IsSuccessStatusCode)
    {
        app.Logger.LogWarning("Vehicle identification provider returned {Status}: {Body}", (int)response.StatusCode, Truncate(responseText, 800));
        return Results.Problem("Vehicle identification provider failed. Check the server log and API account.", statusCode: 502);
    }

    try
    {
        using var json = JsonDocument.Parse(responseText);
        var content = json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        if (string.IsNullOrWhiteSpace(content)) return Results.Problem("The identification service returned an empty result.", statusCode: 502);
        using var identified = JsonDocument.Parse(content);
        return Results.Content(identified.RootElement.GetRawText(), "application/json", Encoding.UTF8);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Could not parse vehicle identification response.");
        return Results.Problem("Could not interpret the identification result.", statusCode: 502);
    }
});

app.MapPost("/api/preview", async (HttpRequest request, IConfiguration config, IHttpClientFactory factory, CancellationToken ct) =>
{
    if (!await IsAuthorized(request, config, ct)) return Results.Unauthorized();
    var apiKey = config["OPENAI_API_KEY"];
    if (string.IsNullOrWhiteSpace(apiKey))
        return Results.Problem("The server is missing OPENAI_API_KEY. Set it as an environment variable; do not put it in the Unity app.", statusCode: 503);

    IFormCollection form;
    try { form = await request.ReadFormAsync(ct); }
    catch { return Results.BadRequest(new { error = "Send multipart/form-data with image and accessories fields." }); }

    var image = form.Files.GetFile("image");
    if (image is null || image.Length == 0) return Results.BadRequest(new { error = "A vehicle photo is required." });
    if (image.Length > 15 * 1024 * 1024) return Results.BadRequest(new { error = "Photo must be 15 MB or smaller." });
    if (!IsImage(image.ContentType)) return Results.BadRequest(new { error = "Use a JPEG, PNG, or WebP photo." });

    var requested = (form["accessories"].ToString() ?? "").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Bullbar", "All-terrain tyres", "Black alloy wheels", "Suspension lift",
        "Snorkel", "Canopy", "Spotlights", "Dark window tint", "Paint: white",
        "Paint: black", "Paint: graphite", "Paint: sand"
    };
    var selected = requested.Where(allowed.Contains).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    if (selected.Length == 0) return Results.BadRequest(new { error = "Select at least one accessory." });

    var vehicleDescription = form["vehicle"].ToString();
    var identityNote = string.IsNullOrWhiteSpace(vehicleDescription) ? "No owner-confirmed make/model was provided; do not guess a different vehicle." : "Owner-entered vehicle identification (use only as a consistency clue, never as a reason to alter visible details): " + vehicleDescription;
    var prompt = $@"EDIT THE PROVIDED REAL VEHICLE PHOTOGRAPH. This is a photorealistic visual concept for a vehicle accessory configurator, not an illustration, CGI render, 3D game, or new vehicle generation.

{identityNote}

ABSOLUTE PRIORITY: preserve the original photographed bakkie and photograph. Keep the exact same vehicle identity, body panels, cab and load-bed shape, wheelbase, grille, headlights, mirrors, windows, trim, perspective, camera angle, scene, background, shadows and natural lighting. Preserve the existing make/model styling and all original visible details. Do not replace the vehicle, change the body shape, redesign the front, change the number of doors, invent decals or text, or alter the surroundings. Do not beautify or stylize it. It must still look like the exact same real photograph.

Only add these selected requested modifications, fitted in physically plausible positions and believable scale: {string.Join(", ", selected)}.

Render the selected accessories with realistic materials, correct perspective, contact shadows, reflections and occlusion. If an accessory is not visible from this photo angle, do not invent a new camera angle or change the photo to show it. Preserve all unselected parts exactly. No text, logos, watermarks, comparison layout, borders, or extra accessories. Output one natural-looking edited photograph from the same viewpoint.

Important: this is a visual concept, not verified mechanical fitment or a guarantee of legal road compliance. Do not add accessories that would make the vehicle unsafe.";

    await using var source = image.OpenReadStream();
    using var content = new MultipartFormDataContent();
    var imagePart = new StreamContent(source);
    imagePart.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
    content.Add(imagePart, "image[]", SafeFilename(image.FileName, image.ContentType));
    content.Add(new StringContent(prompt), "prompt");
    content.Add(new StringContent(config["OPENAI_IMAGE_MODEL"] ?? "gpt-image-1.5"), "model");
    content.Add(new StringContent("high"), "input_fidelity");
    content.Add(new StringContent("high"), "quality");
    content.Add(new StringContent("auto"), "size");
    content.Add(new StringContent("jpeg"), "output_format");
    content.Add(new StringContent("85"), "output_compression");

    using var client = factory.CreateClient("openai");
    using var message = new HttpRequestMessage(HttpMethod.Post, "v1/images/edits");
    message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    message.Content = content;
    using var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, ct);
    var responseText = await response.Content.ReadAsStringAsync(ct);
    if (!response.IsSuccessStatusCode)
    {
        app.Logger.LogWarning("Image editing provider returned {Status}: {Body}", (int)response.StatusCode, Truncate(responseText, 1000));
        return Results.Problem("Photorealistic preview generation failed. Check the server log and API account.", statusCode: 502);
    }

    try
    {
        using var json = JsonDocument.Parse(responseText);
        var encoded = json.RootElement.GetProperty("data")[0].GetProperty("b64_json").GetString();
        if (string.IsNullOrWhiteSpace(encoded)) return Results.Problem("Image provider returned no image.", statusCode: 502);
        var bytes = Convert.FromBase64String(encoded);
        return Results.File(bytes, "image/jpeg", "pimpmybakkie-preview.jpg");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Could not parse generated image response.");
        return Results.Problem("Could not read the generated image.", statusCode: 502);
    }
});

app.Run(builder.Configuration["PIMPMYBAKKIE_BIND_URL"] ?? "http://127.0.0.1:5078");

static bool IsImage(string contentType) =>
    contentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase) ||
    contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase) ||
    contentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase);

static string SafeFilename(string name, string contentType)
{
    var extension = Path.GetExtension(name).ToLowerInvariant();
    if (extension is ".jpg" or ".jpeg" or ".png" or ".webp") return "vehicle" + extension;
    return contentType switch
    {
        "image/png" => "vehicle.png",
        "image/webp" => "vehicle.webp",
        _ => "vehicle.jpg"
    };
}

static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];

static Task<bool> IsAuthorized(HttpRequest request, IConfiguration config, CancellationToken ct)
{
    var expected = config["PIMPMYBAKKIE_ACCESS_TOKEN"];
    if (string.IsNullOrWhiteSpace(expected)) return Task.FromResult(true);
    var supplied = request.Headers["X-PimpMyBakkie-Token"].ToString();
    return Task.FromResult(!string.IsNullOrWhiteSpace(supplied) &&
        System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(supplied)));
}
