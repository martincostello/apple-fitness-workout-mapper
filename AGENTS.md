# Coding Agent Instructions

This file provides guidance to coding agents when working with code in this repository.

## Repository Overview

Apple Fitness Workout Mapper is a .NET web application that visualizes Apple Fitness workouts on a map. Users export Apple Health Data and import it locally to see all their workouts plotted geographically. Built with .NET, TypeScript, and Entity Framework Core.

---

## High-Level Architecture

### Source Code Structure (src/AppleFitnessWorkoutMapper/)

- **Program.cs**: ASP.NET Core minimal API bootstrapper. Configures DI (services, DB context, resilience), sets up static file caching, and defines HTTP endpoints:
  - GET /: Returns Razor Slice rendered app UI (App.cshtml)
  - GET /api/tracks: Returns JSON list of tracks filtered by date range
  - GET /api/tracks/count: Returns track count
  - POST /api/tracks/import: Imports tracks from disk to database

- **Services/**
  - TrackParser.cs: Reads GPX files from disk; parses into Track/TrackPoint models
  - TrackImporter.cs: Orchestrates parsing + database insertion via TracksContext
  - TrackService.cs: Query interface (get tracks by date, latest timestamp, count)

- **Data/** (Entity Framework Core)
  - TracksContext.cs: SQLite DbContext; defines Track and TrackPoint entities
  - Track.cs, TrackPoint.cs: EF entity models
  - ResilientExecutionStrategy.cs: Custom execution strategy using Polly for retry logic on transient DB failures

- **Models/** (View/API DTOs)
  - Track.cs, TrackPoint.cs, TrackCount.cs: JSON-serialized responses via ApplicationJsonSerializerContext (source-gen)

- **Slices/** (View Layer)
  - App.cshtml: Razor Slice using RazorSlices library
  - _ViewImports.cshtml: Shared imports/helpers

- **scripts/ts/**: TypeScript source for client-side logic
  - Compiled via npm run build (webpack) to wwwroot/static/js/main.js
  - Tests via vitest

- **ApplicationOptions.cs**: Configuration class for data directory, database file path, Google Maps API key
- **ApplicationJsonSerializerContext.cs**: AOT-friendly JSON serialization context
- **AppModel.cs**: View model passed to Razor Slice with date ranges and API key

### Test Structure (tests/AppleFitnessWorkoutMapper.Tests/)

xUnit v3 + Shouldly assertions

- **AppTests.cs**: Integration tests via WebApplicationFactory
- **Services/TrackParserTests.cs**: Unit tests for GPX parsing
- **UITests.cs**: Playwright-based end-to-end tests
- **Pages/**: Page Object Model classes for Playwright tests
- **WebApplicationFactory.cs**: Test host factory; configures in-memory config, FakeTimeProvider, isolated SQLite database per test
- **BrowserFixture.cs**, **HttpWebApplicationFactory.cs**: Playwright/HTTP test infrastructure

### Documentation (docs/)

- getting-started.md: User setup guide
- help.md: Troubleshooting
- images/: Screenshots

---

## Repository-Specific Conventions & Patterns

### Namespace Convention

All code uses MartinCostello.AppleFitnessWorkoutMapper as root namespace with file-scoped namespaces.

### Database & Resilience Pattern

- SQLite via Entity Framework Core with custom ResilientExecutionStrategy wrapping Polly retry pipeline
- Transient failures (DbException, InvalidOperationException, TimeoutException) are retried automatically
- TimeProvider injection for testability

### JSON Serialization (AOT-Friendly)

- Uses ApplicationJsonSerializerContext (source gen) instead of reflection
- All API responses explicitly provide context: Results.Json(..., ApplicationJsonSerializerContext.Default.TrackCount)

### Testing Isolation

- Each test gets its own ephemeral SQLite database file
- Fixed current time via FakeTimeProvider for deterministic assertions
- Static test data in App_Data/ directory (GPX files)

### Code Style & Analysis

- StyleCop + custom ruleset (AppleFitnessWorkoutMapper.ruleset)
- File-scoped namespaces
- Copyright header on all C# and TypeScript source files

### Frontend Build & TypeScript Tests

npm scripts in src/AppleFitnessWorkoutMapper/package.json:

- npm run compile: webpack bundling
- npm run format: prettier auto-format
- npm run lint: eslint checks
- npm test: vitest
- npm run build: full pipeline (compile -> format -> lint -> test)
- npm run watch: webpack watch mode

Prettier: 4-space indentation, single quotes, 140-char line width, ES5 trailing commas
ESLint: TypeScript-aware with stylistic rules

---

## Build, Test, and Lint Commands

### Prerequisites

- .NET SDK (auto-bootstrapped; see global.json)
- Node.js 24
- PowerShell 7+

### Commands

Full Build Pipeline (Recommended)
./build.ps1

Run Tests Only
dotnet test --configuration "Release"

Run Single Test
dotnet test --filter "FullyQualifiedName=MartinCostello.AppleFitnessWorkoutMapper.AppTests.Can_Load_Homepage" --configuration "Release"

Skip Tests
./build.ps1 -SkipTests

Frontend Only
cd src/AppleFitnessWorkoutMapper
npm run build      # compile + format + lint + test
npm run compile    # webpack only
npm test           # vitest only
npm run lint       # eslint only
npm run watch      # webpack watch

CI Workflows (in .github/workflows/)

- build.yml: Multi-platform (macOS, Linux, Windows)
- lint.yml: actionlint, markdownlint, zizmor, PSScriptAnalyzer

---

## Existing Configuration Files

Configuration files present:

- .editorconfig: Editor settings
- .markdownlint.json: Markdown lint config
- stylecop.json: StyleCop rules
- AppleFitnessWorkoutMapper.ruleset: Code analysis rule set
- Directory.Build.props: MSBuild centralized versioning
- Directory.Packages.props: NuGet central version management
- .github/CONTRIBUTING.md: Contributor guidelines
- .github/workflows/: CI/CD pipelines

---

## Guidance for Agents

1. **Namespace Convention**: Always use MartinCostello.AppleFitnessWorkoutMapper as root namespace; file-scoped namespaces; include Apache 2.0 copyright header on new files.

2. **Database Work**: Update both EF entity models in Data/ and DTO models in Models/. Use TracksContext for queries. Test with isolated ephemeral databases (WebApplicationFactory provides this).

3. **API Development**: Use Results.* builders for API endpoints in Program.cs (e.g., the /api/tracks mappings). Always pass explicit JSON context to Results.Json() from ApplicationJsonSerializerContext.Default.

4. **Frontend Changes**: TypeScript in scripts/ts/, webpack compiles to wwwroot/static/js/. Run npm run build before commit.

5. **Testing**: xUnit v3 + Shouldly. Inject ITestOutputHelper into test constructors. Use WebApplicationFactory for integration tests. Maintain >= 70% code coverage.

6. **Build Validation**: Always run ./build.ps1 locally before submitting PRs. All checks must pass.

7. **CI Environment**: Tests run on macOS, Linux, and Windows. Lint checks include markdown, PowerShell, code analysis, and GitHub Actions workflows.

## General guidelines

- Always ensure code compiles with no warnings or errors and tests pass locally before pushing changes.
- Do not use APIs marked with `[Obsolete]`.
- Bug fixes should **always** include a test that would fail without the corresponding fix.
- Do not introduce new dependencies unless specifically requested.
- Do not update existing dependencies unless specifically requested.
