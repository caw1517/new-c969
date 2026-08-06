# 02 — Localizer with English and German tables

**What to build:** The application can answer two questions without any UI existing yet: "given a detected Location, which Language should the login form default to?" and "what is the text for this key in this Language?"

German-speaking regions resolve to German; every other region resolves to English as the fallback. Translations are backed by two in-memory dictionaries — **not** `.resx`, per the project's localization decision. Asking for a key that doesn't exist is a programmer error and must fail loudly rather than silently returning the key or an empty string.

The seam is `public` so tests can reach it without the production code referencing the test assembly. Region-to-Language resolution takes the detected region as a parameter rather than reading the OS directly, so it can be tested without changing machine locale.

German strings must be proofread by the developer — German was chosen over French specifically because it can be checked.

**Blocked by:** None — can start immediately. (Its tests need [[01-deletable-test-project-skeleton]]; whichever lands first creates the test project.)

**Status:** ready-for-agent

- [x] A Language type exists with English and German
- [x] Resolving a German-speaking region returns German
- [x] Resolving a non-German-speaking region returns English
- [x] Looking up a key returns the expected string in each Language
- [x] Looking up an unknown key fails loudly and observably
- [x] Resolution accepts a region as input — no test requires changing the OS locale
- [x] Translations live in in-memory dictionaries, with no `.resx` used for login text
