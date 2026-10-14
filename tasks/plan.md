# Implementation Plan: Dependabot #2 + LTS deps + NuGet 2.0.0

## Overview

Clear Dependabot alert #2 (`System.Text.Json` 7.0.3 / CVE-2024-30105) by moving the library onto the **.NET 10 LTS** dependency line, refreshing other direct packages, retargeting off EOL `net6.0` / `netstandard2.1` (required for modern `dotnet-etcd`), and producing a packable **`DotnetEtcdProvider` 2.0.0** `.nupkg` for human upload to nuget.org.

## Architecture Decisions

1. **LTS = .NET 10 line** — Active LTS through Nov 2028. Prefer `*.10.0.13` Microsoft packages over STS 9.x and over maintenance-only .NET 8 (EOS Nov 10, 2026).
2. **Library multi-targets `net8.0;net10.0`** — `dotnet-etcd` ≥ 8 dropped `netstandard2.1`; 9.1.0 supports net8/net9/net10. Keep net8 for broader consumers while aligning primary host apps to net10.
3. **Package version `2.0.0`** — SemVer major: removing `netstandard2.1` and bumping major deps is breaking for consumers.
4. **Minimal code change** — Prefer PackageReference/TFM/metadata edits; only touch C# if `dotnet-etcd` 9.x API breaks compile (Watch/GetRange/Authenticate).
5. **Pack locally, push manually** — `dotnet pack` → `artifacts/`; do not commit API keys or run `nuget push` without explicit human credentials/request.
6. **Test/demo on `net10.0` only** — EOL `net6.0` hosts cannot consume the LTS stack cleanly.

## Target dependency map

| Project | Package | From | To |
| --- | --- | --- | --- |
| Library | System.Text.Json | 7.0.3 | 10.0.13 |
| Library | Microsoft.Extensions.Configuration | 6.0.1 | 10.0.13 |
| Library | Microsoft.Extensions.Configuration.Binder | 6.0.0 | 10.0.13 |
| Library | dotnet-etcd | 5.2.1 | 9.1.0 |
| Test | Microsoft.Extensions.Hosting | 6.0.1 | 10.0.13 |
| Test | Microsoft.NET.Test.Sdk | 17.1.0 | latest stable |
| Test | xunit | 2.4.1 | 2.9.3 |
| Test | xunit.runner.visualstudio | 2.4.3 | 3.1.5 (xunit v2-compatible) |
| Test | coverlet.collector | 3.1.2 | latest stable 6.x/8.x line that works with net10 |
| Test | Testcontainers | 2.1.0 | 4.x latest |
| Demo | Swashbuckle.AspNetCore | 6.2.3 | latest compatible with ASP.NET Core 10 |

## Implementation order

```
1. Library TFM + NuGet metadata + PackageReferences
        │
        ▼
2. Fix compile breaks from dotnet-etcd / Extensions (if any)
        │
        ▼
3. Retarget + bump Test project
        │
        ▼
4. Retarget + bump Demo project
        │
        ▼
5. Verify build/test/vulnerable-list
        │
        ▼
6. README 2.0.0 notes + pack to artifacts/
```

**Sequential:** 1 → 2 → 3/4 can be partly parallel after library builds → 5 → 6.

## Verification checkpoints

| After | Checkpoint |
| --- | --- |
| Tasks 1–2 | `dotnet build DotnetEtcdProvider/DotnetEtcdProvider.csproj -c Release` succeeds for both TFMs |
| Tasks 3–4 | Full solution builds |
| Task 5 | `dotnet test` green; STJ not flagged vulnerable |
| Task 6 | `.nupkg` exists; nuspec shows Version 2.0.0 and STJ ≥ 10.0.13 |

## Risks and Mitigations

| Risk | Impact | Mitigation |
| --- | --- | --- |
| `dotnet-etcd` 5→9 API breaks Watch/GetRange | High | Compile first; read upstream changelog; minimal adapter |
| Testcontainers 4.x API / image changes | Med | Fix tests only as needed; keep etcd image pin if present |
| Consumers still on netstandard / net6 | Med | Document breaking change; multi-target net8+net10 |
| Blind latest test packages (xunit 3 / runner 4) | Med | Stay on xunit **2.9.x** + runner **3.1.x** unless tests migrate cleanly |
| Accidental nuget.org push | High | Pack only; human uploads |

## Out of scope

- Automating `nuget push` in CI
- Closing Dependabot via API (permission-limited)
- Unrelated refactors / nullable enablement

## Open questions for human (defaults applied)

- Package version defaulted to **2.0.0** — override if you want `1.4.0`.
- Push to nuget.org remains **manual** after pack.
