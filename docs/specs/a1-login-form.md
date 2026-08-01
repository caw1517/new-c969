# Spec: A1 — Login Form

Status: Ready for implementation
Date: 2026-07-22
Related: [ADR 0001 — Plaintext password match](../adr/0001-plaintext-password-match.md), [CONTEXT.md](../../CONTEXT.md)

## Problem Statement

A User must authenticate before operating the scheduling application. Today the
app launches straight into `MainForm` with no gate — anyone who runs it is "in."
There is also no accommodation for Users in different locations or languages: all
text is English, hard-coded. The application needs a login step that (a) knows
where the User is, (b) speaks the User's language for both prompts and errors,
and (c) reliably confirms the User is who they say they are.

## Solution

A modal **Login Form** shown at startup, before any authenticated UI exists. It:

- Detects the User's **Location** from the operating system and shows it, using
  that to pick a **default Language**.
- Displays all of its text — labels, buttons, and error/validation messages — in
  English or German, with a manual toggle so the User (or a grader) can switch on
  the spot.
- Verifies the entered username and password against the `user` table and only
  then lets the User into `MainForm`. A failed or cancelled login never
  constructs the authenticated UI.

## User Stories

1. As a User, I want the application to require a username and password before I
   can use it, so that unauthenticated people cannot operate it.
2. As a User, I want to see a login form at startup, so that I know
   authentication is expected.
3. As a User, I want the login form to detect my location automatically, so that
   I don't have to tell the app where I am.
4. As a User, I want my detected location shown on the login form, so that I can
   confirm the app located me correctly.
5. As a User in a German-speaking region, I want the login form to appear in
   German by default, so that I can read it in my language.
6. As a User in any other region, I want the login form to appear in English by
   default, so that I have a sensible fallback.
7. As a User, I want a control to switch the login language manually, so that I
   can read the form in my preferred language regardless of my detected location.
8. As a User, I want the form title, username label, password label, and login
   button translated, so that the whole login prompt is in my language.
9. As a User, I want validation and error messages translated too, so that when
   something goes wrong I understand why in my language.
10. As a User, I want to enter my username and password and submit them, so that
    I can log in.
11. As a User, I want to be taken into the main application when my credentials
    are correct, so that I can start working.
12. As a User, I want a clear message when my credentials are wrong, so that I
    know to try again.
13. As a User, I want the same generic message whether my username or my password
    was wrong, so that no attacker can tell which field was valid.
14. As a User, I want to be told when I've left the username or password blank, so
    that I don't submit an obviously incomplete attempt.
15. As a User, I want leading/trailing spaces in my username to be ignored, so
    that an accidental space doesn't fail an otherwise-correct login.
16. As a User, I want my password compared exactly as I typed it, so that a
    deliberate trailing space in a password is honored.
17. As a User, I want a wrong login to leave me on the login form, so that I can
    correct my entry without restarting the app.
18. As a User, I want to cancel/close the login form, so that I can exit the app
    without logging in.
19. As a developer, I want login decision logic to live outside the form, so that
    it can be tested without driving the UI.
20. As a developer, I want the region-to-language mapping to be a pure function,
    so that I can test A1a without changing my OS locale.
21. As a developer, I want a test that asserts every UI/error string exists in
    both English and German, so that a missing translation fails the build rather
    than shipping.
22. As a developer, I want the test project to be strictly additive, so that I
    can delete it before submission and the application still builds and runs.

## Implementation Decisions

### New / modified modules (all production code in the main `C969-Project`)

- **`User` model** (new, `Database/Models.cs` or alongside it): mirrors the
  `user` table fields needed for auth (`UserId`, `UserName`, plus audit fields as
  needed). A **User** is distinct from a **Customer** (see CONTEXT.md).
- **`Language` enum** (new): `English`, `German`.
- **`Localizer`** (new, `public`): owns the translation tables and language
  resolution.
  - `Language ResolveDefault(RegionInfo region)` — maps a detected region to the
    default Language (German for German-speaking regions, English otherwise).
  - `string T(string key, Language lang)` — returns the translated string for a
    key. Backed by two in-memory dictionaries (no `.resx`). Missing key is a
    programmer error, surfaced loudly.
- **`LoginService`** (new, `public`): orchestrates one Login attempt.
  - `LoginResult Attempt(string username, string password)` where `LoginResult`
    is `Success | EmptyFields | InvalidCredentials`.
  - The credential lookup is **injected** (a delegate or small interface, e.g.
    `Func<string,string,bool>` / `IUserAuthenticator`) so the service's decision
    logic runs without a database. Empty-field check and username trim live here;
    password is passed through untrimmed.
- **`DatabaseManager.ValidateUser(string userName, string password)`** (new):
  the real credential lookup — a **parameterized** `SELECT` against the `user`
  table, **plaintext** password match, username + password only (no `active`
  check). Per ADR 0001. This is the injected implementation `LoginService` uses
  in production.
- **`LoginForm` + `LoginForm.Designer`** (new): a thin WinForms shell. On load,
  detect location, show it, set default language, and render strings via
  `Localizer`. Language toggle re-renders strings. Submit calls `LoginService`
  and maps `LoginResult` to either `DialogResult.OK` or a localized message.
- **`Program.Main`** (modified): `StartConnection()` → `new LoginForm().ShowDialog()`
  → on `DialogResult.OK`, `Application.Run(new MainForm())`; otherwise exit. The
  `MessageBox.Show("Connected to Database")` debug popup is removed.

### Behavioral contracts

- **A1a (Location):** OS `CultureInfo.CurrentCulture` / `RegionInfo.CurrentRegion`
  is the single source of truth. Detected region is displayed on the form and
  drives the default Language.
- **A1b (Language):** Default Language from detected Location; manual toggle
  overrides. Translated set = login form UI strings + login error/validation
  messages only. Invalid login yields a single generic message in the active
  Language. The rest of the app is not localized (out of scope).
- **A1c (Auth):** Username + password verified against `user`. Success → enter
  `MainForm`. Empty field → localized validation message, no DB call. Wrong
  credentials → generic localized message, stay on form.

### Test-project deletability (hard constraint)

The unit test project is **not permitted at submission time** and must be
trivially removable:

- The test project references the main project **one-way** (test → main). The
  main project never references the test project.
- Seams (`Localizer`, `LoginService`, `Language`, `LoginResult`, the injected
  authenticator abstraction) are `public`, so **no `[InternalsVisibleTo]`** is
  added to production code — there is no textual reference to the test assembly
  to remove.
- Deletion procedure = delete the test project folder + remove its one entry from
  `C969-Project.slnx`. The application must build and run unchanged afterward.

## Testing Decisions

**What makes a good test here:** it exercises external behavior of a seam, not
its internals. Tests assert *what* `LoginService.Attempt` decides and *what*
`Localizer` returns — never how the dictionaries are wired or how the form is
laid out. No test drives WinForms UI.

**Prior art:** none — there is currently no test project or test in this
solution. This spec introduces the first one (xUnit or MSTest; pick one and stay
consistent). Keep it in a separate project folder, added only to the solution.

**Modules under test:**

- **`Localizer`**
  - `ResolveDefault` returns `German` for German-speaking region(s) and `English`
    for others (covers A1a).
  - **Translation-coverage test:** every key used by the login form resolves in
    **both** `English` and `German` — a missing German string fails the test.
  - `T` returns the expected known string for representative keys.
- **`LoginService.Attempt`** (with a stubbed authenticator injected):
  - Empty username or empty password → `EmptyFields`, and the authenticator is
    **not** called.
  - Untrimmed username with surrounding spaces still authenticates (trim applied).
  - Password is passed to the authenticator exactly as given (not trimmed).
  - Valid credentials (stub returns true) → `Success`.
  - Invalid credentials (stub returns false) → `InvalidCredentials`.
- **`DatabaseManager.ValidateUser`** — optional integration test only, against a
  live `client_schedule`; not required for the unit suite and skipped when no DB
  is present.

## Out of Scope

- **`login_History.txt` logging** — the attempt-logging companion requirement is
  deferred to its own task (decision from the grilling session).
- Localizing any UI beyond the login form (`MainForm`, `CustomerForm`, and their
  messages remain English).
- Password hashing / salting, and any `active`-flag gating on login (see ADR 0001
  and the auth contract above).
- Time-zone handling — relevant to appointments, not the login form.
- User administration (creating/editing Users) — the User set is managed outside
  this application.

## Further Notes

- Respect ADR 0001: do not "upgrade" the plaintext match to hashing without
  re-seeding the `user` table and re-checking grading against `test` / `test`.
- Use CONTEXT.md vocabulary throughout implementation: **User** (logs in) is never
  a **Customer** (managed record); **Location** is a detected default, not a lock;
  **Language** defaults from Location and is overridable.
- German strings must be proofread by the developer (the reason German was chosen
  over French during grilling).
