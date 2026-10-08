# PimpMyBakkie

Mobile 3D bakkie configurator.

## Product
Take a photo -> AI identifies the bakkie -> user confirms/corrects it -> a realistic 3D vehicle loads -> user rotates it and adds compatible upgrades.

Planned upgrades include wheels, tyres, suspension, bullbars, lights, exhausts, snorkels, canopies, paint and engine upgrades.

## Current prototype
Unity 6 + C# mobile foundation:
- touch swipe rotation
- pinch zoom
- mouse controls for desktop testing
- vehicle/part data models
- build-state performance estimates
- camera capture service
- AI identification service boundary with a development mock
- procedural placeholder bakkie

The placeholder is intentionally temporary. Production vehicles will be modular 3D assets with PBR materials and vehicle-specific compatibility.

## Architecture
Camera -> Identification Service -> Confirmation -> Vehicle Asset -> Configurator -> Build/Performance

The identification provider is isolated behind IVehicleIdentificationService, so the production vision API can be changed without replacing the rest of the app.

See Docs/ROADMAP.md.