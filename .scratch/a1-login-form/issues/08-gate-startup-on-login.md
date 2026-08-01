# 08 — Application startup requires a successful login

**What to build:** The whole feature, demoable. Launching the application no longer drops straight into the main window — the User is met by the login form and must authenticate before any authenticated UI exists.

Startup opens the database connection, shows the login form modally, and only constructs the main window when login succeeded. A failed or cancelled login **never constructs the authenticated UI** — the application exits cleanly, closing the database connection on the way out.

Also remove the `MessageBox.Show("Connected to Database")` debug popup. Note it lives in the database manager's connection-open method, not in the startup entry point.

**Blocked by:** [[07-wire-submit-to-login-service]]

**Status:** ready-for-agent

- [ ] Launching the application shows the login form before the main window
- [ ] Logging in successfully opens the main window
- [ ] Cancelling or closing the login form exits the application without opening the main window
- [ ] The database connection is closed on both the success and the cancel path
- [ ] The "Connected to Database" debug popup no longer appears on launch
- [ ] Repeated failed attempts keep the User on the login form; the application is only entered on success
