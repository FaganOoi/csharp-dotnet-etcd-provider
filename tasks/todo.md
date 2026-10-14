# Tasks: Dependabot #2 + LTS deps + NuGet 2.0.0

## Task 1: Library TFM, metadata, and PackageReferences

**Description:** Retarget `DotnetEtcdProvider` to `net8.0;net10.0`, add NuGet pack metadata for 2.0.0 (aligned with published 1.3.0 authors/license/URL), and bump library PackageReferences to LTS versions including `System.Text.Json` 10.0.13.

**Acceptance criteria:**
- [x] `TargetFrameworks` is `net8.0;net10.0`
- [x] `Version` / pack metadata present (`PackageId`, `Authors`, `Description`, `PackageLicenseExpression`, `PackageProjectUrl`, `RepositoryUrl`)
- [x] Package pins: STJ 10.0.13, Extensions.Configuration(+Binder) 10.0.13, `dotnet-etcd` 9.1.0

**Verification:**
- [ ] `dotnet restore DotnetEtcdProvider/DotnetEtcdProvider.csproj`
- [ ] `dotnet list DotnetEtcdProvider/DotnetEtcdProvider.csproj package` shows target versions

**Dependencies:** None  

**Files likely touched:**
- `DotnetEtcdProvider/DotnetEtcdProvider.csproj`
- `.gitignore` (add `artifacts/` if missing)

**Estimated scope:** Small (1–2 files)

---

## Task 2: Fix library compile breaks from dependency majors

**Description:** Resolve any C# compile errors after `dotnet-etcd` 9.x / Extensions 10.x (constructors, Watch callbacks, Authenticate, usings). Keep behavior equivalent.

**Acceptance criteria:**
- [ ] Library builds Release for `net8.0` and `net10.0`
- [ ] No public API renames unless required by upstream

**Verification:**
- [ ] `dotnet build DotnetEtcdProvider/DotnetEtcdProvider.csproj -c Release`

**Dependencies:** Task 1  

**Files likely touched:**
- `DotnetEtcdProvider/EtcdConfigurationProvider.cs`
- `DotnetEtcdProvider/EtcdConfigurationProvider.*.cs` (only if needed)

**Estimated scope:** Small–Medium (1–3 files)

---

### Checkpoint: Library
- [ ] Library builds clean for both TFMs
- [ ] `System.Text.Json` is 10.0.13

---

## Task 3: Retarget and bump test project

**Description:** Move tests to `net10.0` and upgrade test PackageReferences to a compatible modern set (xunit 2.9.x + runner 3.1.x, Testcontainers 4.x, Hosting 10.0.13, Test.Sdk / coverlet current stable).

**Acceptance criteria:**
- [ ] Test TFM is `net10.0`
- [ ] Packages restored without conflicts
- [ ] Test project builds

**Verification:**
- [ ] `dotnet build DotnetEtcdProvider.Test/DotnetEtcdProvider.Test.csproj -c Release`

**Dependencies:** Task 2  

**Files likely touched:**
- `DotnetEtcdProvider.Test/DotnetEtcdProvider.Test.csproj`
- `DotnetEtcdProvider.Test/*.cs` (only if Testcontainers/xunit APIs require)

**Estimated scope:** Medium (1–4 files)

---

## Task 4: Retarget and bump demo project

**Description:** Move `DemoProject` to `net10.0` and bump `Swashbuckle.AspNetCore` to a version compatible with ASP.NET Core 10.

**Acceptance criteria:**
- [ ] Demo TFM is `net10.0`
- [ ] Demo builds against updated library

**Verification:**
- [ ] `dotnet build DemoProject/DemoProject.csproj -c Release`

**Dependencies:** Task 2  

**Files likely touched:**
- `DemoProject/DemoProject.csproj`
- `DemoProject/Program.cs` (only if Swashbuckle/minimal hosting APIs require)

**Estimated scope:** Small (1–2 files)

---

### Checkpoint: Solution build
- [ ] `dotnet build DotnetEtcdProvider.sln -c Release` succeeds

---

## Task 5: Run tests and vulnerability check

**Description:** Execute the full test suite (Docker required for Testcontainers) and confirm the library no longer pulls a vulnerable `System.Text.Json`.

**Acceptance criteria:**
- [ ] All tests pass
- [ ] `dotnet list ... --vulnerable` does not report STJ CVE for the library project

**Verification:**
- [ ] `dotnet test DotnetEtcdProvider.sln -c Release`
- [ ] `dotnet list DotnetEtcdProvider/DotnetEtcdProvider.csproj package --vulnerable --include-transitive`

**Dependencies:** Tasks 3–4  

**Files likely touched:**
- Test sources only if failures require fixes

**Estimated scope:** Small–Medium

---

## Task 6: Docs, pack NuGet 2.0.0

**Description:** Document 2.0.0 breaking changes in README; run `dotnet pack` to `artifacts/`; confirm nupkg version and dependency groups.

**Acceptance criteria:**
- [ ] README documents 2.0.0 TFM / dependency breaking changes
- [ ] `artifacts/DotnetEtcdProvider.2.0.0.nupkg` exists
- [ ] Pack metadata matches authors MIT + project URL from 1.3.0

**Verification:**
- [ ] `dotnet pack DotnetEtcdProvider/DotnetEtcdProvider.csproj -c Release -o ./artifacts`
- [ ] Inspect nupkg / `dotnet nuget` locals as needed

**Dependencies:** Task 5  

**Files likely touched:**
- `README.md`
- `DotnetEtcdProvider/README.md` (if version notes live there)
- `.gitignore`

**Estimated scope:** Small (2–3 files)

---

### Follow-up (code review Required)
- [x] Watch reload integration test (put / add / delete)
- [x] PackageReleaseNotes + pack root README for nuget.org
- [x] Remove stale nupkg (repack only after commit so RepositoryCommit matches)

### Checkpoint: Complete
- [x] Spec success criteria met
- [ ] `.nupkg` ready for human `nuget push` (commit first, then `dotnet pack`)
- [ ] Ready for PR / merge (Dependabot #2 closes after default-branch merge)

## Parallelization

- After Task 2: Tasks 3 and 4 can proceed in parallel.
- Tasks 5–6 are sequential.
