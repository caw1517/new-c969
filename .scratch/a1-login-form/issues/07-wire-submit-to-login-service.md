# 07 — Submitting the login form authenticates the User

**What to build:** A User can type a username and password, submit, and get a real answer. The form hands the entry to the login service with the database-backed credential lookup wired in as the authenticator.

Three outcomes, all visible to the User in the active Language:

- **Correct credentials** — the form closes reporting success, ready for startup to let the User through.
- **A blank username or password** — a validation message in the active Language, and no database call is made.
- **Wrong credentials** — a single generic message in the active Language. It reads the same whether the username or the password was wrong, so nothing reveals which field was valid. The User stays on the form with the ability to correct their entry without restarting the application.

The User can also cancel or close the form, which reports a non-success outcome so startup can exit.

All messages come from the Localizer. Language switching still works, and a message shown after switching appears in the newly chosen Language.

**Blocked by:** [[03-login-service-decision-logic]], [[04-user-model-and-validate-user]], [[05-login-form-shell-with-detected-location]]

**Status:** ready-for-agent

- [ ] Submitting the seeded `test`/`test` credentials closes the form reporting success
- [ ] Submitting a wrong password shows a localized message and leaves the form open with the User able to retry
- [ ] Submitting a nonexistent username shows the identical message to the wrong-password case
- [ ] Submitting a blank username shows a localized validation message and makes no database call
- [ ] Submitting a blank password shows a localized validation message and makes no database call
- [ ] Every message shown appears in German when the form is in German
- [ ] Cancelling or closing the form reports a non-success outcome
- [ ] The form contains no credential-checking logic of its own — it delegates and maps the outcome
