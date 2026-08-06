# 03 — LoginService decision logic with injected authenticator

**What to build:** One login attempt can be decided in isolation, with no database and no form. Given a username and password, the service returns exactly one of three outcomes: the attempt succeeded, a required field was blank, or the credentials were rejected.

Empty-field checking happens here and short-circuits — when a field is blank, no credential lookup is attempted at all. The username is trimmed of surrounding whitespace before lookup, so an accidental leading or trailing space doesn't fail an otherwise-correct login. The password is passed through exactly as typed, untrimmed, so a deliberate trailing space in a password is honored.

The credential lookup itself is **injected** — a delegate or small interface — so the decision logic runs against a stub. The real database-backed implementation arrives in [[04-user-model-and-validate-user]] and is wired up in [[07-wire-submit-to-login-service]].

Wrong username and wrong password must be indistinguishable in the outcome, so nothing downstream can reveal which field was valid.

The seam is `public` so tests reach it without production code referencing the test assembly.

**Blocked by:** None — can start immediately. (Its tests need [[01-deletable-test-project-skeleton]]; whichever lands first creates the test project.)

**Status:** done

- [x] An attempt with a valid username and password reports success
- [x] An attempt with rejected credentials reports invalid credentials
- [x] A blank username reports the empty-field outcome and performs no credential lookup
- [x] A blank password reports the empty-field outcome and performs no credential lookup
- [x] A username with surrounding spaces still authenticates
- [x] The password reaches the authenticator byte-for-byte as supplied, including trailing spaces
- [x] Wrong username and wrong password produce the same outcome value
- [x] The authenticator is injected — every test above runs with no database present

## What landed

- `C969-Project/Login/IUserAuthenticator.cs` — the injected seam. `Validate(string userName, string password)`, spelled to match the `DatabaseManager.ValidateUser(string userName, string password)` signature ticket 04 implements.
- `C969-Project/Login/LoginService.cs` — `Attempt(string username, string password)`, constructor-injected authenticator.
- `C969-Project/Database/Enums.cs` — `LoginResult { Success, EmptyFields, InvalidCredentials }`, appended alongside the existing enums as ticket 02 did with `Language`.
- `C969-Project.Tests/LoginServiceTests.cs` — 11 `[Fact]`s plus a hand-rolled `StubAuthenticator` that records call count and the exact arguments received. No mocking library added.

Suite: 20/20 green, 0 warnings.

## Human verification

Automated first:

```powershell
dotnet build C969-Project.slnx    # 0 warnings, 0 errors
dotnet test C969-Project.slnx     # 20/20, no database required
```

Then by hand — these are the things a passing suite won't tell you:

- [x] **The short-circuit is real.** Read `Attempt` in `C969-Project/Login/LoginService.cs` top to bottom and confirm the blank-field check is an early `return LoginResult.EmptyFields;` that happens *before* the only `_authenticator.Validate(...)` call — not a lookup computed and then discarded. A loosely-asserted stub could let the wrong shape pass.
- [x] **Trim asymmetry, by eye.** At the single `Validate` call site: `.Trim()` on the username argument, and *nothing* on the password argument. This is the most likely thing to get silently "tidied" wrong by a later edit.
- [x] **`LoginResult` member names read right.** Ticket 07 maps these three to localized messages; renaming after that is churn.
- [x] **No database, form, or startup code was touched.** `git diff --stat` should show no change to `Database/DatabaseManager.cs`, `Program.cs`, or anything under `Forms/` — those are tickets 04 and 07.
- [x] **No user-facing strings crept in.** `LoginService` returns enum values, never text; the localized messages come from `Localizer` in ticket 07. Only exception messages should contain English prose.
- [x] **The app is unchanged at runtime.** `dotnet run --project C969-Project` still launches straight to `MainForm`. Nothing calls `LoginService` yet, so behavior must be identical to before this ticket.
- [x] **Deletability still holds.** No `[InternalsVisibleTo]` anywhere in production code, and `C969-Project.csproj` has no reference to the test project. Optionally do the full drill: delete `C969-Project.Tests/`, remove its one line from `C969-Project.slnx`, confirm build + run, then `git checkout` to restore.

## Open note for ticket 07

"Blank" was implemented as null/empty/**whitespace-only** for both fields, so an all-spaces password returns `EmptyFields` without a lookup. That reads slightly against the spec's "password compared exactly as I typed it" (`docs/specs/a1-login-form.md`, user story 16) for that pathological case. The untrimmed byte-for-byte pass-through is intact for every non-blank password. Flagged here in case it warrants revisiting before the form is wired up.
