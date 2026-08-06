# 04 — User model and credential lookup against the user table

**What to build:** The application can ask the database whether a given username and password identify a real User. This is the production credential lookup that [[03-login-service-decision-logic]] injects.

A **User** logs into the application and is never a **Customer** — Customer is a managed record that never logs in. Keep that distinction in the model naming and don't reuse Customer machinery here.

Per the project's accepted decision on password comparison, the check is a **parameterized** SELECT against the `user` table comparing the submitted password to the stored password as **plaintext**, matching on username and password only, with no `active`-flag gating. Do not introduce hashing — the provided database ships plaintext passwords and a seeded `test`/`test` account that graders expect to work against the unmodified schema.

Parameterization is not negotiable and is independent of the plaintext decision. Note this is the **first** parameterized query in the codebase — every existing query builds SQL inline with no parameters — so this ticket sets the idiom that later queries follow. Follow the existing query-method shape otherwise: static method on the database manager, reusing the shared long-lived connection.

**Blocked by:** None — can start immediately.

**Status:** done

- [x] A User model exists mirroring the `user` table fields needed for authentication, distinct from Customer
- [x] A credential-lookup method exists on the database manager returning whether the pair identifies a User
- [x] The query is parameterized — no user input is concatenated into SQL
- [x] Password comparison is plaintext; no hashing is introduced
- [x] The query matches on username and password only, with no `active` check
- [x] Verified by hand against the seeded `test`/`test` account on a live database
- [x] A wrong password for a real username returns false rather than throwing

## What landed

- `C969-Project/Database/Models.cs` — `User : AuditableModel` (`UserId`, `UserName`,
  `Active`). Deliberately **no `Password` property**: the comparison happens in SQL, so a
  plaintext password is never materialized into a managed object where it could reach a
  heap dump, a debugger watch, a log line, or a databound grid. `Active` mirrors the column
  for faithfulness but is **not** consulted at login, per ADR 0001.
- `C969-Project/Database/DatabaseManager.cs` — `ValidateUser(string userName, string password)`,
  the codebase's **first parameterized query**. `AddWithValue` for both string parameters;
  `ExecuteReader` + `reader.Read()` as the boolean, matching `GetCustomers`' idiom. No
  `try/catch` — query methods here don't have one, and a `catch` would mask a database
  outage as "invalid credentials", making a User retype a correct password forever.
- `C969-Project/Login/DatabaseUserAuthenticator.cs` — adapts the static `DatabaseManager`
  to the `IUserAuthenticator` seam from ticket 03. `DatabaseManager` stays static and never
  names the interface; the dependency points Login → Database, one way.
- `C969-Project.Tests/DatabaseUserAuthenticatorTests.cs` — 2 `[Fact]`s.

Suite: 22/22 green, 0 warnings.

### Decisions worth recording

**Case-sensitive password, via `COLLATE`.** MySQL's default collation is case-insensitive,
so a plain `WHERE password = @password` would accept `TEST` for a stored `test`. The query
appends `COLLATE utf8mb4_bin` to force an exact match. This does **not** conflict with
ADR 0001 — that ADR mandates *plaintext*, not *case-insensitive*, and `test`/`test` still
matches — and it is consistent with `LoginService` already refusing to trim the password
(spec story 16, "password compared exactly as I typed it"). The username is deliberately
left case-insensitive, as usernames conventionally are.

`COLLATE` rather than the `BINARY` operator: [MySQL deprecated `BINARY` in 8.0.27](https://dev.mysql.com/doc/relnotes/mysql/8.0/en/news-8-0-27.html)
and [expects to remove it](https://dev.mysql.com/doc/refman/9.7/en/cast-functions.html).
It still works today, but there is no reason to ship a deprecated spelling.

**Parameter idiom for the rest of the codebase.** This is the first parameterized query, so
it sets precedent: **`AddWithValue` for strings, explicit `MySqlDbType` for dates and
decimals.** The type-inference objections to `AddWithValue` don't bite on `string`, and a
cheap idiom is the one that actually gets followed; the appointment queries that bind
`DateTime` should use the explicit form.

**The integration test was declined, not forgotten.** `docs/specs/a1-login-form.md` makes a
`ValidateUser` DB test "optional integration test only… not required for the unit suite".
`ValidateUser` is a thin wrapper over `MySqlCommand` against the static `Conn` with no seam
to stub, so the manual checklist below is the substitute — not an oversight.

**AC 7 is satisfied structurally, not by test.** A non-matching `WHERE` yields zero rows and
`reader.Read()` returns `false`, with no exception in play. There is no code path that
throws on a wrong password. The live-DB step below is what actually confirms it.

## Human verification

Automated first:

```powershell
dotnet build C969-Project.slnx    # 0 warnings, 0 errors
dotnet test  C969-Project.slnx    # 22/22, no database required
```

Then by hand. **The suite proves nothing about the SQL** — `ValidateUser` has no seam to
stub, so every automated test stops at the adapter boundary. The live-DB steps are not
optional here; they are the only evidence this query runs at all.

### Confirm the seeded account exists

The `mysql` CLI is not on PATH on this machine — use MySQL Workbench, DBeaver, or whatever
you normally use, with the credentials in `C969-Project/App.config` (`localhost:3306`,
database `client_schedule`, user `sqlUser`).

- [x] **The seed row is really there and really plaintext.**
      `SELECT userId, userName, password, active FROM user WHERE userName = 'test';`
      → exactly one row, password literally `test`, not a hash, not space-padded. The
      rubric notes the provided database ships with no data, so seed this row if missing.
      If this returns nothing, every step below fails for reasons unrelated to this ticket.
- [x] **The column names match what shipped.** Run `DESCRIBE user;` and confirm `userId` /
      `userName` / `password` are spelled as the SQL assumes. `docs/Database ERD.pdf` is
      image-based and could not be read while planning, so these came from the standard WGU
      schema — a casing or naming mismatch fails at runtime, not at build.
- [x] **The shipped query agrees with the database.** Paste it in with the parameters
      substituted by hand and confirm one row:
      ```sql
      SELECT u.userId FROM user u
      WHERE u.userName = 'test' AND u.password = 'test' COLLATE utf8mb4_bin LIMIT 1;
      ```
      Re-run with `'TEST' COLLATE utf8mb4_bin` → **zero** rows (the case guard working).
      Drop the `COLLATE` clause entirely and the same query returns a row — the behavior we
      deliberately rejected. If `utf8mb4_bin` errors, the column is on a different charset;
      check `SHOW FULL COLUMNS FROM user;` and use that charset's `_bin` collation.

### Exercise `ValidateUser` by hand

No login form exists yet (ticket 05) and `Program.Main` launches straight to `MainForm`, so
there is no UI path to this method. Use a **temporary** probe and revert it: in
`C969-Project/Program.cs`, immediately after `DatabaseManager.StartConnection();` and
before `Application.Run(new MainForm());`, add

```csharp
MessageBox.Show($"test/test   -> {DatabaseManager.ValidateUser("test", "test")}\n" +
                $"test/wrong  -> {DatabaseManager.ValidateUser("test", "wrong")}\n" +
                $"nobody/test -> {DatabaseManager.ValidateUser("nobody", "test")}\n" +
                $"test/TEST   -> {DatabaseManager.ValidateUser("test", "TEST")}\n" +
                $"injection   -> {DatabaseManager.ValidateUser("' OR 1=1 -- ", "x")}");
```

then `dotnet run --project C969-Project`, read the box, **delete the block**, and confirm
`git diff` shows `Program.cs` unmodified before committing.

- [x] **`test`/`test` returns True.** The grading path. If this is False, nothing else about
      this ticket matters.
- [x] **A wrong password for a real username returns False — and does not throw.** The
      `test`/`wrong` line reads `False` and the box appears at all. An exception here would
      mean the method reaches for a row that isn't there instead of letting `reader.Read()`
      answer. This is the acceptance criterion ticked structurally above; this step is what
      actually confirms it.
- [x] **A nonexistent username returns False, indistinguishably.** `nobody`/`test` reads
      `False`, identical to the wrong-password line — nothing reveals which field was wrong.
- [x] **Case-sensitive password.** `test`/`TEST` reads `False`. If it reads `True`, either
      the `COLLATE` clause was dropped or the column's charset isn't `utf8mb4`.
- [x] **Injection is inert.** The `' OR 1=1 -- ` line reads `False`, not `True` — the
      username was matched as a literal string, not executed as SQL.

### By eye

- [x] **Nothing is concatenated into the SQL.** The `string sql = @"..."` literal contains
      `@userName` and `@password` as literal text, with no `+`, no `$"` interpolation, no
      `string.Format`. Every later query copies whatever this one looks like.
- [x] **No hashing crept in.** No `SHA`, `MD5`, `BCrypt`, or `HashData` in the diff. (Note
      `DatabaseManager.cs` has a pre-existing unused `using System.Security.Cryptography.X509Certificates;`
      at the top — that is old boilerplate, not from this ticket.) ADR 0001 stands.
- [x] **No `active` gating.** The `WHERE` clause names `userName` and `password` only.
      `User.Active` exists as a model property but is deliberately not consulted — confirm
      nobody "helpfully" added `AND u.active = 1`.
- [x] **`DatabaseManager` is still static and still doesn't know about Login.** No
      `using C969_Project.Login;` in `DatabaseManager.cs`.
- [x] **The app is unchanged at runtime.** `dotnet run --project C969-Project` still shows
      the "Connected to Database" popup and launches straight to `MainForm`. Nothing calls
      `ValidateUser` in production yet — that's ticket 07 — so behavior must be identical.
- [x] **Deletability still holds.** No `[InternalsVisibleTo]` in production code;
      `C969-Project.csproj` has no reference to the test project. Optionally do the full
      drill: delete `C969-Project.Tests/`, remove its one line from `C969-Project.slnx`,
      confirm build + run, then `git checkout` to restore.

## Notes for later tickets

- **Ticket 07** wires `new LoginService(new DatabaseUserAuthenticator())` to the form's
  submit. Ticket 03's open note (a whitespace-only password returns `EmptyFields` before any
  lookup, slightly against spec story 16 for that pathological case) is untouched here and
  still open.
- **Unrelated, worth its own ticket:** the `//Lambda - Consider using this as one of the 3
  required` comment on `CustomerDisplay.FullAddress` is inaccurate. An expression-bodied
  member is `=>` syntax, not a lambda expression, and the rubric ties its three lambdas to
  the three A7 **reports** (`docs/Rubric.md:86`). The same caveat applies to
  `DatabaseUserAuthenticator.Validate` — it should not be claimed as a rubric lambda either.
