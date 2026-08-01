# 05 — Login form shell showing detected Location in the default Language

**What to build:** A modal login form that a User can open and read. On load it detects the User's Location from the operating system, displays that Location so the User can confirm the app located them correctly, and uses it to pick the default Language. Every piece of text on the form — window title, username label, password label, and the login button — is rendered through the Localizer in that Language.

A User in a German-speaking region sees the form in German. A User anywhere else sees English.

**Location is a detected default, not a lock** — this ticket only establishes the default; overriding it is [[06-language-toggle-rerenders-form]].

The form is not yet reachable from application startup and the submit button does nothing yet — that is [[07-wire-submit-to-login-service]] and [[08-gate-startup-on-login]]. It is demoable by launching it directly.

Keep the form a thin shell: no decision logic lives here. The OS culture/region is the single source of truth for detection. Follow the existing modal-dialog idiom used by the customer form, including the designer-generated file layout.

**Blocked by:** [[02-localizer-english-german]]

**Status:** ready-for-agent

- [ ] A modal login form exists with username and password inputs and a login button
- [ ] The password input masks what is typed
- [ ] The detected Location is visible on the form
- [ ] The default Language is derived from the detected Location
- [ ] Title, username label, password label, and login button text all come from the Localizer — no hard-coded English in the form
- [ ] The form can be launched and visually inspected in both Languages
- [ ] No credential or validation logic is implemented in the form itself
