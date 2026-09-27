# Change Request Approval

A small web application for creating, reviewing, approving, and rejecting technical change requests.

The project models a decision that happens before implementation: whether a proposed technical change should proceed. It is deliberately not a pull-request or task-management tool.

## Workflow

```text
Draft → Submitted → Approved
                  → Rejected
```

A draft may be incomplete while it is being written. Before it can be sent to review, it must contain a title, description, and technical impact. A reviewer can approve with an optional comment or reject with a required comment. Approved and rejected requests are final.

## Run locally

Prerequisites: .NET 10 SDK and Node.js with npm.

```bash
npm install
npm run build
dotnet run --project src/ChangeRequestApproval.Web
```

Open <http://localhost:5154>.

Run the automated tests with:

```bash
dotnet test
```

## Structure

```text
src/ChangeRequestApproval.Web/
  Domain/       ChangeRequest and its workflow rules
  Api/          HTTP request contracts
  Persistence/  EF Core DbContext
  Frontend/     TypeScript source
  wwwroot/      Files served to the browser

tests/ChangeRequestApproval.Tests/
  Domain unit tests and HTTP workflow tests
```

The [architecture diagrams](docs/architecture.md) show the normal request path, the workflow-test setup, and the responsibility boundaries. The [planning and review index](docs/rebuild-planning/00-index-and-workflow.md) links requirements, API contract, tests, and traceability.

## Technical decisions

- One ASP.NET Core application serves both the HTTP API and the small browser client. The problem does not justify distributed deployment or separate frontend infrastructure.
- The frontend uses plain TypeScript and the browser DOM API. A framework would add more concepts than this small interface needs.
- `ChangeRequest` owns the workflow rules and state transitions. Endpoints coordinate HTTP, persistence, and error responses; they do not decide whether a transition is valid.
- EF Core with SQLite provides local persistence with no separate database installation. Workflow tests replace it with an isolated SQLite in-memory database.
- Tests focus on state transitions, invalid transitions, required data at send-to-review time, and the HTTP workflow. They do not attempt exhaustive testing of framework behavior.

## What I deliberately did not build

- Authentication, roles, and user identity. A real system would use authenticated creator and reviewer identities rather than manually entered names.
- Integration with GitHub, Azure DevOps, pull requests, or task tracking.
- Notifications, attachments, audit-log infrastructure, search, reporting, or paging.
- A generic workflow engine. The four states and their rules are explicit because the workflow is small.
- A frontend framework, microservices, messaging, or other infrastructure without a concrete need.

## Documentation

- [Business rules](docs/rebuild-planning/03-requirements-and-business-rules.md)
- [User journeys and business flow](docs/rebuild-planning/04-user-journeys-and-business-flow.md)
- [State and domain model](docs/rebuild-planning/05-state-and-domain-model.md)
- [API contract](docs/rebuild-planning/07-api-contract.md)
- [Test strategy and manual test cases](docs/rebuild-planning/08-test-strategy-and-manual-test-cases.md)
