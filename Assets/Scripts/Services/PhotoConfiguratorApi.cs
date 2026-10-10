using System;
using System.Collections;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PimpMyBakkie.Services
{
    [Serializable]
    public sealed class VehicleIdentificationResult
    {
        public string make, model, variant, bodyStyle, uncertainty;
        public int year;
        public float confidence;
        public string[] visibleClues;
    }

    [Serializable]
    public sealed class PhotoServiceHealth
    {
        public string service, status, note;
        public bool imageEditingConfigured, vehicleIdentificationConfigured;
    }

    public static class PhotoConfiguratorApi
    {
        static readonly HttpClient Client = CreateClient();

        static HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            return client;
        }
        public static IEnumerator CheckHealth(string baseUrl, Action<bool, PhotoServiceHealth, string> done)
        {
            Task<HttpResponseMessage> task = Client.GetAsync(Endpoint(baseUrl, "/api/health"));
            while (!task.IsCompleted) yield return null;
            if (task.IsFaulted || task.IsCanceled) { done(false, null, task.Exception?.GetBaseException().Message ?? "Service request timed out or was cancelled."); yield break; }
            using (var response = task.Result)
            {
                Task<string> bodyTask = response.Content.ReadAsStringAsync();
                while (!bodyTask.IsCompleted) yield return null;
                if (!response.IsSuccessStatusCode) { done(false, null, "Health check failed: HTTP " + (int)response.StatusCode); yield break; }
                PhotoServiceHealth health = null;
                try { health = JsonUtility.FromJson<PhotoServiceHealth>(bodyTask.Result); }
                catch (Exception ex) { done(false, null, "Could not read service status: " + ex.Message); yield break; }
                done(health != null && health.status == "online", health, health == null ? "Invalid health response." : "");
            }
        }

        public static IEnumerator Identify(Texture2D photo, string baseUrl, string token, Action<bool, VehicleIdentificationResult, string> done)
        {
            if (photo == null) { done(false, null, "Take or import a photo first."); yield break; }
            Task<HttpResponseMessage> task = SendPhoto(photo, baseUrl, token, "/api/identify", null, null);
            while (!task.IsCompleted) yield return null;
            if (task.IsFaulted || task.IsCanceled) { done(false, null, task.Exception?.GetBaseException().Message ?? "Photo-service request timed out or was cancelled."); yield break; }
            using (var response = task.Result)
            {
                Task<string> bodyTask = response.Content.ReadAsStringAsync();
                while (!bodyTask.IsCompleted) yield return null;
                if (!response.IsSuccessStatusCode) { done(false, null, Explain(response, bodyTask.Result)); yield break; }
                VehicleIdentificationResult result = null;
                try { result = JsonUtility.FromJson<VehicleIdentificationResult>(bodyTask.Result); }
                catch (Exception ex) { done(false, null, "Could not read identification: " + ex.Message); yield break; }
                if (result == null || string.IsNullOrWhiteSpace(result.make) || string.IsNullOrWhiteSpace(result.model))
                    done(false, null, "AI could not identify this vehicle reliably. Enter details manually or try a clearer photo.");
                else done(true, result, "");
            }
        }
        public static IEnumerator GeneratePreview(Texture2D photo, string accessories, string vehicle, string baseUrl, string token, Action<bool, Texture2D, string> done)
        {
            if (photo == null) { done(false, null, "Take or import a photo first."); yield break; }
            if (string.IsNullOrWhiteSpace(accessories)) { done(false, null, "Choose at least one accessory."); yield break; }
            Task<HttpResponseMessage> task = SendPhoto(photo, baseUrl, token, "/api/preview", accessories, vehicle);
            while (!task.IsCompleted) yield return null;
            if (task.IsFaulted || task.IsCanceled) { done(false, null, task.Exception?.GetBaseException().Message ?? "Photo-service request timed out or was cancelled."); yield break; }
            using (var response = task.Result)
            {
                Task<byte[]> bodyTask = response.Content.ReadAsByteArrayAsync();
                while (!bodyTask.IsCompleted) yield return null;
                if (!response.IsSuccessStatusCode)
                {
                    done(false, null, Explain(response, Encoding.UTF8.GetString(bodyTask.Result)));
                    yield break;
                }
                var texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
                if (!texture.LoadImage(bodyTask.Result, false))
                {
                    UnityEngine.Object.Destroy(texture);
                    done(false, null, "Preview service returned an unreadable image.");
                    yield break;
                }
                done(true, texture, "");
            }
        }

        static async Task<HttpResponseMessage> SendPhoto(Texture2D photo, string baseUrl, string token, string path, string accessories, string vehicle)
        {
            var form = new MultipartFormDataContent();
            var image = new ByteArrayContent(photo.EncodeToJPG(90));
            image.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            form.Add(image, "image", "vehicle.jpg");
            if (accessories != null) form.Add(new StringContent(accessories), "accessories");
            if (vehicle != null) form.Add(new StringContent(vehicle), "vehicle");

            using (var request = new HttpRequestMessage(HttpMethod.Post, Endpoint(baseUrl, path)))
            {
                request.Content = form;
                if (!string.IsNullOrWhiteSpace(token))
                    request.Headers.TryAddWithoutValidation("X-PimpMyBakkie-Token", token.Trim());
                return await Client.SendAsync(request);
            }
        }
        static string Endpoint(string url, string path) =>
            (string.IsNullOrWhiteSpace(url) ? "http://127.0.0.1:5078" : url.Trim()).TrimEnd('/') + path;

        static string Explain(HttpResponseMessage response, string body)
        {
            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    var problem = JsonUtility.FromJson<Problem>(body);
                    if (!string.IsNullOrWhiteSpace(problem.detail)) return problem.detail;
                    if (!string.IsNullOrWhiteSpace(problem.title)) return problem.title;
                    if (!string.IsNullOrWhiteSpace(problem.error)) return problem.error;
                }
                catch { }
            }
            if ((int)response.StatusCode == 401) return "API token rejected. Check Settings and server configuration.";
            if ((int)response.StatusCode == 503) return "Photo service needs OPENAI_API_KEY on the server.";
            return "Photo service error " + (int)response.StatusCode + ": " + response.ReasonPhrase;
        }

        [Serializable]
        sealed class Problem { public string title, detail, error; }
    }
}
