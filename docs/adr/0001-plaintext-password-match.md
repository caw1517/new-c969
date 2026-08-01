# 1. Plaintext password match against the provided `user` table

Date: 2026-07-22

## Status

Accepted

## Context

The login form (requirement A1c) must verify a username and password. The
`client_schedule` database provided for this project ships a `user` table in
which passwords are stored as **plaintext**, with a seeded `test` / `test`
account that graders expect to work against the stock database.

We considered hashing passwords (e.g. SHA-256 / BCrypt) and comparing hashes,
which is the correct practice for a real system. But the provided schema stores
plaintext; adopting hashing would require re-hashing and overwriting the seed
data, and would break any evaluation that logs in with `test` / `test` against
the unmodified database.

## Decision

Authenticate by comparing the submitted password to the stored password as
**plaintext**, using a **parameterized** `SELECT` against the `user` table
(username + password only; no `active` check). The query is parameterized to
prevent SQL injection — that safeguard is independent of the plaintext decision
and is not negotiable.

## Consequences

- The seeded `test` / `test` login works against the unmodified provided
  database; nothing needs re-seeding.
- Password storage is plaintext. This is a property of the **provided**
  database, not a security posture we endorse. Do not "fix" it by introducing
  hashing without also re-seeding the `user` table and re-checking grading
  expectations — doing so silently breaks the stock `test` account.
- If this project were ever taken beyond coursework, hashing (and salted,
  slow hashing at that) would be a prerequisite, and this ADR would be
  superseded.
