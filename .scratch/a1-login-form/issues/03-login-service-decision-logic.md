# 03 — LoginService decision logic with injected authenticator

**What to build:** One login attempt can be decided in isolation, with no database and no form. Given a username and password, the service returns exactly one of three outcomes: the attempt succeeded, a required field was blank, or the credentials were rejected.

Empty-field checking happens here and short-circuits — when a field is blank, no credential lookup is attempted at all. The username is trimmed of surrounding whitespace before lookup, so an accidental leading or trailing space doesn't fail an otherwise-correct login. The password is passed through exactly as typed, untrimmed, so a deliberate trailing space in a password is honored.

The credential lookup itself is **injected** — a delegate or small interface — so the decision logic runs against a stub. The real database-backed implementation arrives in [[04-user-model-and-validate-user]] and is wired up in [[07-wire-submit-to-login-service]].

Wrong username and wrong password must be indistinguishable in the outcome, so nothing downstream can reveal which field was valid.

The seam is `public` so tests reach it without production code referencing the test assembly.

**Blocked by:** None — can start immediately. (Its tests need [[01-deletable-test-project-skeleton]]; whichever lands first creates the test project.)

**Status:** ready-for-agent

- [ ] An attempt with a valid username and password reports success
- [ ] An attempt with rejected credentials reports invalid credentials
- [ ] A blank username reports the empty-field outcome and performs no credential lookup
- [ ] A blank password reports the empty-field outcome and performs no credential lookup
- [ ] A username with surrounding spaces still authenticates
- [ ] The password reaches the authenticator byte-for-byte as supplied, including trailing spaces
- [ ] Wrong username and wrong password produce the same outcome value
- [ ] The authenticator is injected — every test above runs with no database present
