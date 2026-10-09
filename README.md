# PimpMyBakkie

A photo-first, realism-focused bakkie configurator.

## Non-negotiable product rule

The user's own photo is the source of truth. Never substitute a generic or cartoon-style vehicle for the vehicle in the photo. The app must identify the exact make, model, year and variant, ask the user to confirm it, and then load a high-quality matching 3D asset before allowing realistic accessory fitting.

## Intended workflow

1. Take or import a photo of the actual bakkie.
2. Identify the exact vehicle and let the user confirm/correct it.
3. Load a matching photorealistic vehicle asset with correct proportions, trim and body details.
4. Fit vehicle-compatible real accessories: wheels, tyres, suspension, bullbars, lights, exhaust, snorkel, canopy, paint and other parts.
5. Show believable fitment, costs, and clearly labelled estimates for performance changes.

## Current prototype status

- Unity 6 / C# project.
- Photo capture and preview workflow.
- Touch and mouse interaction scaffolding remains available for a later true 3D configurator.
- A production vision service is **not connected** yet.
- A production library of accurate, variant-specific vehicle and accessory assets is **not connected** yet.
- Vehicle identification must not be faked by returning a hard-coded Hilux.
- Until the exact vehicle asset is available, the app must not display a generic procedural bakkie as if it were the user's vehicle.

A single photo can preserve the vehicle's visible appearance, but true 360-degree rotation with correctly placed accessories requires a matching 3D asset or multi-view reconstruction. The product should not claim that capability until implemented and tested.

## Architecture

Photo capture/import -> production vision service -> user confirmation -> exact variant asset -> compatible parts catalogue -> photorealistic configurator.

See `Docs/ROADMAP.md` for the planned production work.
