# 01 — Deletable test project skeleton

**What to build:** A unit test project exists alongside the application and can be run, so that later tickets have somewhere to put their tests. Critically, it can be deleted before submission with no trace: removing the project folder and its single solution entry leaves an application that builds and runs unchanged.

This is a prefactor. It delivers no user-facing behavior — it makes the later tickets easy.

Pick one test framework (xUnit or MSTest) and stay consistent with it for every later ticket. The test project targets the same framework as the application so it can reference a WinForms project. The reference is one-way: test → main, never the reverse, and no `[InternalsVisibleTo]` is ever added to production code.

**Blocked by:** None — can start immediately.

**Status:** ready-for-agent

- [x] A test project exists in its own folder, added to the solution with a single entry
- [x] It references the main project; the main project has no reference to it
- [x] At least one trivial test exists and passes when the suite is run
- [x] No `[InternalsVisibleTo]` attribute appears anywhere in production code
- [x] Deletion procedure verified by hand: delete the test folder, remove its one solution entry, and the application still builds and launches
