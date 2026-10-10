# PimpMyBakkie photo service

This backend keeps the OpenAI API key on the computer/server, never inside the Unity client.

## Start locally on Windows

1. Install the .NET 8 SDK or newer.
2. Open PowerShell in this folder.
3. Run `./run-api.ps1`.
4. Enter an OpenAI API key when prompted. The key is held only in the current process environment.
5. Copy the generated app access token into Unity: **API Settings → Access Token**.
6. Keep the PowerShell window open. The local API health check is `http://127.0.0.1:5078/api/health`.

AI image edits are billed by the API provider. ChatGPT subscription access is separate from API billing.

## API

- `GET /api/health` — confirms whether the server has an OpenAI API key configured.
- `POST /api/identify` — multipart form with an `image` field; returns a cautious make/model/year/variant suggestion.
- `POST /api/preview` — multipart form with `image`, comma-separated `accessories`, and optional owner-confirmed `vehicle`; returns an edited JPEG.

Supported accessories: Bullbar, All-terrain tyres, Black alloy wheels, Suspension lift, Snorkel, Canopy, Spotlights, Dark window tint, Paint: white, Paint: black, Paint: graphite, Paint: sand.

## Safety and limitations

- The backend defaults to binding to localhost only. Do not expose it publicly without HTTPS, authentication, rate limiting, and spending limits.
- The generated preview is a visual concept. It cannot prove that parts fit, are road-legal, or are mechanically safe.
- Image editing can change details despite strict instructions. Compare against the original photo before accepting a preview.
- A confirmed make/model is a user-visible identity hint; the app does not yet have a curated exact-variant 3D asset library.
- For phone use against a development PC, deploy this service securely or intentionally bind it to the LAN and use the PC's LAN address in the app settings. Never expose the API key in the Unity app.
