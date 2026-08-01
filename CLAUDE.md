# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

WGU C969 coursework: a WinForms scheduling application (.NET 10, `net10.0-windows`, C# with nullable + implicit usings enabled) backed by a MySQL `client_schedule` database. Single project, solution file is `C969-Project.slnx` (XML solution format, not `.sln`).

## Commands

```powershell
dotnet build C969-Project.slnx           # build
dotnet run --project C969-Project        # build + launch the app
```

There is **no test project yet**. `docs/specs/a1-login-form.md` specifies the first one — see the deletability constraint below before adding it.

The app requires a reachable MySQL instance; connection details live in `C969-Project/App.config` under the `localDb` connection string (`ConfigurationManager.ConnectionStrings["localDb"]`). Note the `providerName` there says `System.Data.SqlClient` but the code uses `MySql.Data` — the provider name is inert, don't "fix" it into a runtime change.

## Architecture

**Startup flow** (`Program.cs`): `ApplicationConfiguration.Initialize()` → `DatabaseManager.StartConnection()` → `Application.Run(new MainForm())` → `DatabaseManager.EndConnection()`. Per the A1 spec, a modal `LoginForm` is meant to be inserted between the connection and `MainForm`, gating on `DialogResult.OK`.

**Database layer** (`C969-Project/Database/`) is hand-rolled ADO.NET — no ORM.
- `DatabaseManager` is a **static class holding one long-lived `MySqlConnection`** (`Conn`) opened at startup and reused by every query. Query methods are static, build SQL inline, and read via `MySqlDataReader`. New queries follow that shape; parameterize anything taking user input.
- Two parallel model families, deliberately distinct:
  - `Models.cs` — table-shaped entities (`Customer`, `City`, `Country`) inheriting `AuditableModel` (`CreateDate`/`CreatedBy`/`UpdatedDate`/`LastUpdatedBy`), mirroring the DB's audit columns.
  - `DisplayModels.cs` — flattened read models (`CustomerDisplay`) produced by joins across `customer`/`address`/`city`/`country`. These are what the UI binds to. Computed members like `FullAddress` exist here.
- `Enums.cs` — small UI/domain enums (`CustomerFormType`).

**UI layer.** `MainForm` (still named `Form1.cs`/`Form1.Designer.cs` on disk, class is `MainForm`) is a tabbed shell: Customers, Appointments, Calendar. The customers grid is bound with `AutoGenerateColumns = false` and columns wired via `DataPropertyName` in the designer — **adding a grid column means editing the Designer file's `DataPropertyName`, not just the model**.

`CustomerForm` (`Forms/`) is a dual-mode dialog: parameterless ctor = Add, `CustomerForm(CustomerDisplay)` = Edit; mode is captured in `_formType` and the title is set in the ctor. Field population happens in `CustomerForm_Load`, validation in `ValidateCustomerInput()` returning a `List<string>` of messages joined into one `MessageBox`. The save handler's Add/Edit branches are currently empty stubs — persistence is unimplemented. City/Country inputs are commented out pending a lookup-control decision.

## Working docs

- `CONTEXT.md` — domain glossary. **Use this vocabulary.** Most important distinction: a **User** logs into the app; a **Customer** is a managed record and never logs in. **Location** is a detected default, not a lock; **Language** defaults from Location and is overridable.
- `docs/specs/` — implementation-ready specs (problem, user stories, module decisions, testing decisions, out-of-scope). Read the relevant spec before implementing a feature it covers.
- `docs/adr/` — architecture decisions. ADR 0001 mandates **plaintext** password comparison against the provided `user` table (parameterized SELECT, no `active` check) so the seeded `test`/`test` account works against the unmodified grading database. Do not introduce hashing.
- `graphify-out/` — generated knowledge graph of the codebase; regenerated, not hand-edited.

## Coursework constraints

- Any test project must be **strictly additive and trivially deletable**: one-way reference (test → main), production seams kept `public` so no `[InternalsVisibleTo]` is ever added, and deletion = remove the folder plus its one entry in `C969-Project.slnx`. The app must build and run unchanged afterward.
- The rubric requires at least three lambda expressions; `CustomerDisplay.FullAddress` is flagged in-code as a candidate. Prefer lambdas where they read naturally rather than retrofitting them later.
- Localization for the login form is done with in-memory dictionaries in a `Localizer` class, **not** `.resx` (per the A1 spec). Only the login form is localized; the rest of the app stays English.
