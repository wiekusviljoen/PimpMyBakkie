$ErrorActionPreference = "Stop"
$project = Join-Path $PSScriptRoot "PimpMyBakkie.Api.csproj"

Write-Host ""
Write-Host "PimpMyBakkie photo service" -ForegroundColor Green
Write-Host "Your OpenAI API key stays on this computer and is not saved in the Unity project." -ForegroundColor Gray
Write-Host "Image editing can incur API charges." -ForegroundColor Yellow
Write-Host ""

$secure = Read-Host "Enter OpenAI API key (input is hidden)" -AsSecureString
$ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
try {
    $env:OPENAI_API_KEY = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
}
finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)
}

if ([string]::IsNullOrWhiteSpace($env:OPENAI_API_KEY)) {
    Write-Warning "No API key entered. The service will start, but AI identification and image edits will return a setup message until OPENAI_API_KEY is configured."
}

$env:PIMPMYBAKKIE_ACCESS_TOKEN = [Guid]::NewGuid().ToString("N") + [Guid]::NewGuid().ToString("N")
$env:PIMPMYBAKKIE_BIND_URL = "http://127.0.0.1:5078"

Write-Host ""
Write-Host "App access token (copy this into Unity > API Settings):" -ForegroundColor Yellow
Write-Host $env:PIMPMYBAKKIE_ACCESS_TOKEN -ForegroundColor White
Write-Host ""
Write-Host "Starting local API at http://127.0.0.1:5078 ..." -ForegroundColor Green
Write-Host "Keep this window open while using the app." -ForegroundColor Gray
Write-Host "Press Ctrl+C to stop the service." -ForegroundColor Gray
Write-Host ""

dotnet run --project $project --no-launch-profile
