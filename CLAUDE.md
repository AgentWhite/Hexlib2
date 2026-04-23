# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Documentation

```bash
# One-time install
dotnet tool install -g docfx

# Regenerate API metadata (needed after adding/renaming public members)
cd docs && docfx metadata

# Build the static site
cd docs && docfx build

# Serve locally at http://localhost:8090
cd docs && docfx serve _site --port 8090

# Build + serve in one step
cd docs && docfx --serve --port 8090
```

Generated output (`docs/_site/`, `docs/api/`, `docs/obj/`) is git-ignored. Only source files in `docs/` are committed.

## Build & Test Commands

```bash
# Build the entire solution
dotnet build HexLib.slnx

# Run all tests
dotnet test HexLib.slnx

# Run tests for a specific project
dotnet test HexLib.Tests
dotnet test ASL.Tests
dotnet test ASLInputTool.Tests

# Run a single test by name
dotnet test ASL.Tests --filter "FullyQualifiedName~MyTestName"

# Build only the WPF app
dotnet build ASLInputTool/ASLInputTool.csproj
```

The solution uses the modern `.slnx` format (`HexLib.slnx`). Target framework is **.NET 10.0**; the WPF project (`ASLInputTool`) targets `net10.0-windows`.

## Project Layout & Dependencies

```
HexLib/           — Platform-agnostic hex-grid geometry library (no game logic)
HexLib.Tests/     — xUnit tests for HexLib
HexLibConsole/    — Minimal console scratch pad
ASL/              — Advanced Squad Leader domain library (depends on HexLib)
ASL.Tests/        — xUnit tests for ASL
ASLInputTool/     — WPF editor application (depends on ASL + HexLib)
ASLInputTool.Tests/ — xUnit tests for WPF ViewModels
```

**Dependency direction:** `ASLInputTool` → `ASL` → `HexLib`. Never add reverse references.

## Architecture

### HexLib
Pure geometry library. `Hex<THexMetadata>` stores axial coordinates, metadata, and counters. `Board<THexMetadata, TEdgeData>` is a collection of hexes with orientation/rotation support. Both are generic so different game systems can attach their own metadata types.

### ASL Domain (ASL/)
Layered inside `ASL/`:

- **Models/Units/** — `Unit` (implements `ICounter`) is composed from `IUnitComponent` implementations in `Models/Components/` (e.g., `InfantryComponent`, `LeadershipComponent`, `FirePowerComponent`). Add behaviour by adding components, not by subclassing `Unit`.
- **Models/Board/** — `AslBoard` wraps `Board<ASLHexMetadata, ASLEdgeData>` with ASL-specific terrain (`TerrainType`, `LocationFeature`, `RubbleType`) and edge data.
- **Models/Scenarios/** — `ASLProject` is the top-level save container holding counters, modules, scenarios, and boards.
- **Rules/** — `IRule<TContext>` / `IRulePipeline` provide an extensible pipeline for applying game rules against typed contexts (e.g., `MovementContext`).
- **Infrastructure/** — `IDiceProvider` abstraction with `RandomDiceProvider`, `MockDiceProvider`, and `JournaledDiceProvider` for testing and replay.
- **Persistence/** — `ASLSaveManager` orchestrates JSON serialization via a pluggable `IStorageAdapter` (production uses `FileStorageAdapter`). JSON options use `ReferenceHandler.IgnoreCycles` and `JsonStringEnumConverter`.

### ASLInputTool (WPF)
MVVM pattern throughout:

- **ViewModelBase** (`ViewModels/ViewModelBase.cs`) — base class implementing `INotifyPropertyChanged` + `INotifyDataErrorInfo`. Use `SetProperty(ref _field, value)` for all property setters; validation via `DataAnnotations` runs automatically on every set.
- **CrudViewModelBase** — extends `ViewModelBase` for list-based CRUD screens (Squads, Leaders, Heroes, Equipment, etc.).
- **MainViewModel** — navigation host; holds `CurrentView` and a list of top-level `NavigationItems` resolved through `ViewModelLocator`.
- **ViewModelLocator** — central DI/registry for all top-level ViewModels.
- **BoardEditorViewModel** — split across three partial files (`*.cs`, `*.Commands.cs`, `*.Grid.cs`, `*.Visuals.cs`) for the hex board editor feature.
- **SvgEditorViewModel** — manages real-time SVG counter graphics preview.
- **RelayCommand** — standard `ICommand` implementation; use for all command bindings.

WPF-specific packages: `Microsoft.Xaml.Behaviors.Wpf`, `SharpVectors.Wpf` (SVG rendering).

## Key Conventions

- **Nullable reference types** are enabled globally (`<Nullable>enable</Nullable>`). All code must be null-safe; the build treats warnings as errors for the main projects.
- **XML doc comments** (`<GenerateDocumentationFile>true</GenerateDocumentationFile>`) are required on all public members in `HexLib`, `ASL`, and `ASLInputTool`.
- Properties in ViewModels must always go through `SetProperty` — never assign backing fields directly and call `OnPropertyChanged` manually.
- New game components belong in `ASL/Models/Components/` implementing `IUnitComponent`.
- New rules belong in `ASL/Rules/` implementing `IRule<TContext>`.
- Persistence round-trips through `ASLSaveManager`; do not add direct `File.Write` calls elsewhere.
