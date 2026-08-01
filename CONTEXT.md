# Context: C969 Scheduling Application

Glossary of domain terms. Definitions only — no implementation details.

## Glossary

### User
A person who **logs in to and operates** the application (e.g. the seeded `test`
account). Authenticated by username + password at startup. A User is *not* a
Customer — Users run the app; Customers are records the app manages. The set of
Users is fixed/administered outside this application.

### Customer
A client **record managed within** the application (name, address, active
status, etc.). Customers are created, edited, and viewed by a User. A Customer
never logs in.

### Location
The User's detected geographic region, derived from the operating system's
current culture at startup. Drives the **default** display Language and is shown
on the login form. It is a default, not a lock — see Language.

### Language
The language the login form's text and error messages are displayed in.
Defaults from the detected Location (English, or German when the region calls
for it) and can be overridden by the User via a toggle on the login form.
Supported: English (default) and German.

### Login attempt
A single try to authenticate — a submitted username/password pair, resulting in
success or failure.
