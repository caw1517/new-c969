# 06 — Language toggle re-renders the form

**What to build:** A User (or a grader) can switch the login form's Language on the spot, regardless of what Location was detected. Flipping the control re-renders every string on the form in place — title, labels, and button — without reopening the form and without losing anything already typed into the username or password fields.

**Language defaults from Location and is overridable** — this ticket delivers the override half. Once toggled, the manual choice wins over the detected default for the rest of the form's life.

Only the login form is localized. The rest of the application stays English.

**Blocked by:** [[05-login-form-shell-with-detected-location]]

**Status:** ready-for-agent

- [ ] A visible control on the login form switches between English and German
- [ ] Switching re-renders title, username label, password label, and login button immediately
- [ ] Switching does not clear text already entered in the username or password fields
- [ ] The manual choice overrides the Location-derived default
- [ ] Switching back returns the form to the other Language correctly — the toggle works in both directions
- [ ] No UI outside the login form changes Language
