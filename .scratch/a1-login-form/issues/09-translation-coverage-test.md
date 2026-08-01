# 09 — Translation coverage test

**What to build:** A missing German string fails the test suite rather than shipping. A test walks every key the login form actually uses — UI labels, the login button, validation messages, and the generic invalid-credentials message — and asserts each one resolves in **both** English and German.

Deliberately sequenced last among the localization work: by this point the full set of keys the form uses is settled, so the test asserts against the real surface rather than a guess. Whatever mechanism the test uses to enumerate keys, adding a new login string later without its German translation must fail this test.

This test asserts external behavior of the Localizer seam — what it returns for a key — never how its dictionaries are wired internally. No test drives the WinForms UI.

**Blocked by:** [[02-localizer-english-german]], [[06-language-toggle-rerenders-form]]

**Status:** ready-for-agent

- [ ] A test enumerates the keys used by the login form and asserts each resolves in English
- [ ] The same test asserts each resolves in German
- [ ] Verified to actually fail: temporarily remove one German string and confirm the test goes red, then restore it
- [ ] Adding a new login string without a German translation fails this test
- [ ] The test asserts returned strings only — it does not reach into dictionary internals or drive the UI
