# Architecture diagrams

These diagrams show the responsibilities around the application. They deliberately show framework-provided work separately from project code.

## 1. A normal API request

```mermaid
sequenceDiagram
    participant Client as Browser or API client
    participant AspNet as ASP.NET Core
    participant Endpoint as Endpoint in Program.cs
    participant Domain as ChangeRequest
    participant Ef as EF Core / DbContext
    participant Db as SQLite database

    Client->>AspNet: POST /change-requests/{id}/submit-for-review
    AspNet->>AspNet: Create request scope
    AspNet->>AspNet: Read route id and resolve DbContext from DI
    AspNet->>Endpoint: Call handler(id, database)
    Endpoint->>Ef: FindAsync(id)
    Ef->>Db: SELECT change request
    Db-->>Ef: Stored data
    Ef-->>Endpoint: ChangeRequest object
    Endpoint->>Domain: SubmitForReview()
    Domain-->>Endpoint: Status becomes Submitted, or throws a rule error
    Endpoint->>Ef: SaveChangesAsync()
    Ef->>Db: UPDATE status
    Endpoint-->>AspNet: 200 OK + ChangeRequest
    AspNet-->>Client: JSON response
    AspNet->>AspNet: Dispose request DbContext
```

For this endpoint, the client sends only the URL and HTTP method; it does not send a JSON body or a `DbContext`. ASP.NET Core creates the `DbContext` from its dependency-injection registration for this request.

## 2. An acceptance test

```mermaid
sequenceDiagram
    participant Xunit as xUnit
    participant Test as ChangeRequestWorkflowTests
    participant Factory as ChangeRequestApiFactory
    participant Host as ASP.NET Core test host
    participant Domain as ChangeRequest business rules
    participant Ef as EF Core / DbContext
    participant Db as SQLite in memory

    Xunit->>Factory: Create once for this test class
    Factory->>Db: Open empty in-memory database
    Factory->>Host: Start Program.cs with test DbContext configuration
    Xunit->>Test: Run method marked [Fact]
    Test->>Factory: CreateClient()
    Factory-->>Test: HttpClient connected to test host
    Test->>Host: POST create, PUT draft details, POST send-to-review, GET by id
    Host->>Domain: Create draft, update details, call SubmitForReview()
    Domain-->>Host: Draft or Submitted state
    Host->>Ef: Read and save ChangeRequest
    Ef->>Db: Read and write test data
    Db-->>Ef: Data
    Ef-->>Host: ChangeRequest object
    Host-->>Test: HTTP responses
    Test->>Test: Assert expected status codes and JSON values
    Xunit->>Factory: Dispose after tests
    Factory->>Db: Close connection and remove test data
```

The acceptance test uses the same endpoints and domain rules as the real application, but it does not start a visible server and does not touch the local `change-requests.db` file.

## 3. Responsibility map

```mermaid
flowchart LR
    Client[Browser or API client]

    subgraph Project[Project code]
        Api[Program.cs endpoints]
        Request[CreateChangeRequestRequest]
        Domain[ChangeRequest<br/>business rules]
        Persistence[ChangeRequestDbContext]
    end

    subgraph Frameworks[Framework-provided code]
        AspNet[ASP.NET Core<br/>HTTP, JSON binding, DI]
        Ef[Entity Framework Core<br/>object/database mapping]
        Xunit[xUnit<br/>test discovery and fixtures]
        TestHost[WebApplicationFactory<br/>test web host]
    end

    Db[(SQLite)]

    Client --> AspNet
    AspNet --> Request
    AspNet --> Api
    Api --> Domain
    Api --> Persistence
    Persistence --> Ef
    Ef --> Db
    Xunit --> TestHost
    TestHost --> AspNet
```

`ChangeRequest` owns workflow rules. `Program.cs` coordinates HTTP requests with the domain and persistence. ASP.NET Core supplies HTTP, JSON binding, and dependency injection; EF Core supplies database mapping; xUnit and `WebApplicationFactory` exist only for tests.
