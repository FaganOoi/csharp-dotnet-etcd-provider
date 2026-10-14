# Spec: Fix Dependabot Alert #2 — System.Text.Json (+ dependency refresh & NuGet release)

**Source:** [Dependabot alert #2](https://github.com/FaganOoi/csharp-dotnet-etcd-provider/security/dependabot/2)  
**Status:** Implemented — ready for review / NuGet upload  
**Date:** 2026-10-14

## Objective

Resolve GitHub Dependabot security alert #2 by upgrading vulnerable dependencies (starting with `System.Text.Json`), refresh other direct package references onto the **active .NET LTS** line, and produce a **packable NuGet release** (`DotnetEtcdProvider`) ready to upload to nuget.org.

**User:** Maintainers and consumers of the `DotnetEtcdProvider` NuGet package.

**Success looks like:** Alert #2 is fixed on the default branch; library builds/tests on LTS TFMs; `dotnet pack` yields an uploadable `.nupkg` with correct metadata.

## Decisions (approved)

| # | Question | Decision |
| --- | --- | --- |
| Q1 | Alert identity | Yes — `System.Text.Json` / CVE-2024-30105 (GHSA-hh2w-p6rv-4g7w) |
| Q2 | Version line | Prefer **LTS** → target **.NET 10** (active LTS through Nov 2028; .NET 8 LTS ends Nov 10, 2026) |
| Q3 | Scope | Bump **other direct deps** in the same effort (library, test, demo) |
| Q4 | Packaging | Yes — produce a NuGet package for upload |

## Alert

| Field | Value |
| --- | --- |
| Package | `System.Text.Json` |
| Current version | `7.0.3` |
| Advisory | [GHSA-hh2w-p6rv-4g7w](https://github.com/advisories/GHSA-hh2w-p6rv-4g7w) / CVE-2024-30105 |
| Severity | High |
| **Target** | **`10.0.13`** (latest .NET 10 LTS patch; also clears CVE-2024-43485) |

## Tech Stack (target)

| Area | Current | Target |
| --- | --- | --- |
| Library TFM | `netstandard2.1` | `net8.0;net10.0` (required once `dotnet-etcd` ≥ 8 drops netstandard) |
| Test / Demo TFM | `net6.0` (EOL) | `net10.0` |
| `System.Text.Json` | 7.0.3 | 10.0.13 |
| `Microsoft.Extensions.Configuration*` | 6.0.x | 10.0.13 |
| `Microsoft.Extensions.Hosting` (tests) | 6.0.1 | 10.0.13 |
| `dotnet-etcd` | 5.2.1 | 9.1.0 (latest; supports net8/net10) |
| Test stack | older xunit/Testcontainers | Latest compatible (xunit 2.9.x + runner 3.x/4.x, Testcontainers 4.x, etc.) |
| Demo | Swashbuckle 6.2.3 | Latest compatible with ASP.NET Core 10 |
| NuGet package version | 1.3.0 (published) | **2.0.0** (breaking: TFM + major dependency bumps) |

## Commands

```bash
dotnet restore DotnetEtcdProvider.sln
dotnet build DotnetEtcdProvider.sln --configuration Release
dotnet test DotnetEtcdProvider.sln --configuration Release
dotnet list DotnetEtcdProvider/DotnetEtcdProvider.csproj package --vulnerable --include-transitive
dotnet pack DotnetEtcdProvider/DotnetEtcdProvider.csproj --configuration Release --output ./artifacts
# Upload (human): nuget push ./artifacts/DotnetEtcdProvider.2.0.0.nupkg -s https://api.nuget.org/v3/index.json -k <API_KEY>
```

## Project Structure

```
DotnetEtcdProvider/           → Library (TFM, PackageReferences, NuGet metadata)
DotnetEtcdProvider.Test/      → Tests (TFM + test PackageReferences)
DemoProject/                  → Sample host (TFM + Swashbuckle)
docs/specs/                   → This spec
tasks/plan.md                 → Implementation plan
tasks/todo.md                 → Task checklist
artifacts/                    → Pack output (gitignored)
README.md                     → Version / breaking-change notes for 2.0.0
```

## Code Style

```xml
<PropertyGroup>
  <TargetFrameworks>net8.0;net10.0</TargetFrameworks>
  <Nullable>disable</Nullable>
  <PackageId>DotnetEtcdProvider</PackageId>
  <Version>2.0.0</Version>
  <Authors>Fagan Ooi</Authors>
  <Description>It is custom configuration provider that use `dotnet-etcd` library to load the configuration and update it from time to time.</Description>
  <PackageProjectUrl>https://github.com/FaganOoi/csharp-dotnet-etcd-provider</PackageProjectUrl>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <RepositoryUrl>https://github.com/FaganOoi/csharp-dotnet-etcd-provider</RepositoryUrl>
  <GeneratePackageOnBuild>false</GeneratePackageOnBuild>
</PropertyGroup>

<PackageReference Include="System.Text.Json" Version="10.0.13" />
```

- Prefer explicit PackageReference versions (no CPM unless already present).
- No drive-by refactors unrelated to deps / packaging / TFM compile fixes.
- If `dotnet-etcd` API changed, minimal adapter fixes only.

## Testing Strategy

| Level | What |
| --- | --- |
| Build | Solution builds for `net8.0` and `net10.0` library outputs |
| Tests | Existing xUnit + Testcontainers suite on `net10.0` |
| Vuln scan | `dotnet list ... --vulnerable` clean for library direct/transitive STJ |
| Pack | `dotnet pack` produces `DotnetEtcdProvider.2.0.0.nupkg` with expected dependency groups |
| Manual | Human uploads `.nupkg` to nuget.org; Dependabot #2 closes after merge |

## Boundaries

- **Always:** Clear CVE-2024-30105; keep public configuration-provider behavior; document 2.0.0 breaking changes; verify build/test/pack.
- **Ask first:** Pushing to nuget.org with a shared secret; major public API redesign; dropping `net8.0` target; skipping `dotnet-etcd` major bump.
- **Never:** Commit API keys; remove failing tests to “pass”; ship 1.3.1 pretending TFM drop is non-breaking.

## Success Criteria

1. Library references `System.Text.Json` ≥ 10.0.13 (LTS) and Dependabot #2 is addressed.
2. Other direct deps refreshed to LTS-compatible currents; test/demo on `net10.0`.
3. `dotnet build` + `dotnet test` succeed.
4. `dotnet pack` produces upload-ready `DotnetEtcdProvider.2.0.0.nupkg` with metadata aligned to prior 1.3.0 release (authors, license MIT, project URL).
5. README notes 2.0.0 breaking changes (TFM, dependency majors).
6. No intentional behavior change beyond what dependency/TFM upgrades require.

## Open Questions (resolved or deferred)

- ~~Alert identity~~ → Yes, STJ CVE.
- ~~LTS vs 8.0.6~~ → .NET 10 LTS line.
- ~~Other deps~~ → Yes.
- ~~NuGet~~ → Yes, pack for upload (push remains human with API key).
- **Deferred default:** Package version **2.0.0** (SemVer breaking due to TFM drop). Say if you prefer `1.4.0` instead.
