# 04 — User model and credential lookup against the user table

**What to build:** The application can ask the database whether a given username and password identify a real User. This is the production credential lookup that [[03-login-service-decision-logic]] injects.

A **User** logs into the application and is never a **Customer** — Customer is a managed record that never logs in. Keep that distinction in the model naming and don't reuse Customer machinery here.

Per the project's accepted decision on password comparison, the check is a **parameterized** SELECT against the `user` table comparing the submitted password to the stored password as **plaintext**, matching on username and password only, with no `active`-flag gating. Do not introduce hashing — the provided database ships plaintext passwords and a seeded `test`/`test` account that graders expect to work against the unmodified schema.

Parameterization is not negotiable and is independent of the plaintext decision. Note this is the **first** parameterized query in the codebase — every existing query builds SQL inline with no parameters — so this ticket sets the idiom that later queries follow. Follow the existing query-method shape otherwise: static method on the database manager, reusing the shared long-lived connection.

**Blocked by:** None — can start immediately.

**Status:** ready-for-agent

- [ ] A User model exists mirroring the `user` table fields needed for authentication, distinct from Customer
- [ ] A credential-lookup method exists on the database manager returning whether the pair identifies a User
- [ ] The query is parameterized — no user input is concatenated into SQL
- [ ] Password comparison is plaintext; no hashing is introduced
- [ ] The query matches on username and password only, with no `active` check
- [ ] Verified by hand against the seeded `test`/`test` account on a live database
- [ ] A wrong password for a real username returns false rather than throwing
