# Retrosharp Architecture Documentation

## System Overview

Retrosharp is an n-tiered web application designed for baseball statistics and historical data analysis. The system imports and processes data from the Retrosheet project, providing search, analysis, and visualization capabilities through a RESTful API.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                        Client Layer                         │
│                    (Angular 17 - Future)                    │
└────────────────────────────┬────────────────────────────────┘
							 │ HTTPS/JSON
┌────────────────────────────▼────────────────────────────────┐
│                   Retrosharp.UI.Api                         │
│                    (ASP.NET Core)                           │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────┐   │
│  │   Players    │  │    Games     │  │     Admin       │   │
│  │  Controller  │  │  Controller  │  │   Controller    │   │
│  └──────┬───────┘  └──────┬───────┘  └────────┬────────┘   │
│         │                 │                    │            │
│         │                 │                    │ Send Msg   │
│         └────────┬────────┘                    │            │
└──────────────────┼─────────────────────────────┼────────────┘
				   │ Service Layer               │
┌──────────────────▼─────────────────────────────▼────────────┐
│              Retrosharp.Service                             │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────┐   │
│  │   Person     │  │    Game      │  │    Player       │   │
│  │   Service    │  │   Service    │  │  Statistics     │   │
│  └──────┬───────┘  └──────┬───────┘  └────────┬────────┘   │
│         │                 │                    │            │
│         └────────┬────────┘                    │            │
└──────────────────┼─────────────────────────────┼────────────┘
				   │ Repository Pattern          │
┌──────────────────▼─────────────────────────────▼────────────┐
│               Retrosharp.Data                               │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────┐   │
│  │   Person     │  │   Batting    │  │    Game         │   │
│  │  Repository  │  │  Repository  │  │  Repository     │   │
│  └──────┬───────┘  └──────┬───────┘  └────────┬────────┘   │
│         │                 │                    │            │
│         └────────┬────────┘                    │            │
│                  │ Entity Framework Core       │            │
│         ┌────────▼─────────────────────────────▼────────┐   │
│         │        RetrosharpContext (DbContext)         │   │
│         └──────────────────┬───────────────────────────┘   │
└────────────────────────────┼───────────────────────────────┘
							 │ ADO.NET
┌────────────────────────────▼────────────────────────────────┐
│                      SQL Server                             │
│                   (Database Layer)                          │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                   Message Bus (RabbitMQ)                    │
└────────────────────┬────────────────────┬───────────────────┘
					 │                    │
┌────────────────────▼────────────────────▼───────────────────┐
│            Retrosharp.Engine.Console                        │
│                 (NServiceBus Endpoint)                      │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              GameLogSaga                             │   │
│  │  ┌────────────┐  ┌────────────┐  ┌─────────────┐    │   │
│  │  │ GameLog    │  │ GameLog    │  │  GameLog    │    │   │
│  │  │   Start    │  │  Complete  │  │   Cancel    │    │   │
│  │  └─────┬──────┘  └─────┬──────┘  └──────┬──────┘    │   │
│  └────────┼───────────────┼────────────────┼───────────┘   │
│           │               │                │               │
│  ┌────────▼───────────────▼────────────────▼───────────┐   │
│  │         GameLogFileService (ETL Parser)            │   │
│  └────────────────────────┬───────────────────────────┘   │
│                           │                               │
│                           │ Uses Services & Repositories  │
└───────────────────────────┼───────────────────────────────┘
							│
					┌───────▼────────┐
					│  Retrosheet    │
					│  Data Files    │
					└────────────────┘
```

## Layer Descriptions

### 1. Presentation Layer (`Retrosharp.UI.Api`)

**Technology**: ASP.NET Core 10, Minimal APIs / MVC Controllers  
**Responsibilities**:
- Expose RESTful API endpoints for client consumption
- Handle HTTP request/response lifecycle
- Authentication and authorization
- Input validation
- Response formatting (JSON)
- Send messages to ETL engine via NServiceBus (send-only endpoint)

**Key Components**:
- **Controllers**: Handle HTTP requests and orchestrate service calls
- **Models/DTOs**: Request/response data transfer objects
- **Middleware**: Authentication, logging, error handling
- **NServiceBus Client**: Send-only endpoint for initiating ETL jobs

**Dependencies**:
- Retrosharp.Service (business logic)
- Retrosharp.Service.Interface (service contracts)
- NServiceBus (for messaging)

---

### 2. Service Layer (`Retrosharp.Service`)

**Technology**: .NET 10 Class Library  
**Responsibilities**:
- Implement business logic and rules
- Coordinate between repositories and external systems
- Perform data transformations
- Calculate advanced statistics
- ETL file parsing and processing
- Ensure data integrity and idempotency

**Key Components**:

**Business Services**:
- `PersonService`: Person management, search, career statistics
- `GameService`: Game management, ETL processing logic
- `BattingService`: Batting statistics management
- `PlayerStatisticsService`: Advanced metrics calculation and aggregation

**ETL Services**:
- `GameLogFileService`: Parse Retrosheet game log files (CSV)
- `BioFileService`: Parse Retrosheet biographical files (CSV)

**Key Patterns**:
- **Service Pattern**: Encapsulate business logic
- **Idempotency**: Check for existing records before insert
- **Dependency Injection**: All dependencies injected via constructor
- **Async/Await**: All I/O operations are asynchronous

**Dependencies**:
- Retrosharp.Data (repository access)
- Retrosharp.Service.Interface (service contracts)
- CsvHelper (file parsing)
- Mapster (object mapping)

---

### 3. Data Layer (`Retrosharp.Data`)

**Technology**: Entity Framework Core 10, SQL Server  
**Responsibilities**:
- Database access and persistence
- Entity mapping (ORM)
- Query execution
- Transaction management
- Connection pooling

**Key Components**:

**DbContext**:
- `RetrosharpContext`: EF Core context with DbSet properties and relationship configuration

**Repositories** (10 total):
- `PersonRepository`: Person CRUD and search operations
- `BattingRepository`: Batting statistics access
- `PitchingRepository`: Pitching statistics access
- `FieldingRepository`: Fielding statistics access
- `FranchiseRepository`: Franchise/team access
- `LeagueRepository`: League access
- `BallparkRepository`: Ballpark access
- `GameRepository`: Game access
- `GameLineupRepository`: Game lineup access
- `GameStatisticsRepository`: Game-level statistics access

**Key Patterns**:
- **Repository Pattern**: Abstract data access logic
- **Unit of Work**: Provided by DbContext
- **Generic Repository**: BaseRepository<TM, TC> for common operations
- **Specification Pattern**: Entity-specific query methods

**Dependencies**:
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.SqlServer
- Mapster (for projection)

---

### 4. Domain/Contract Layer (`Retrosharp`)

**Technology**: .NET 10 Class Library  
**Responsibilities**:
- Define domain entities (contracts)
- Define data transfer objects
- Define message types for NServiceBus
- Define file format classes for Retrosheet data
- Provide shared interfaces and abstractions

**Key Components**:

**Contracts** (Domain Entities):
- `Person`: Player, manager, umpire, coach
- `Game`: Baseball game with all details
- `Batting`: Batting statistics (with `BattingStatistics` for advanced metrics)
- `Pitching`: Pitching statistics
- `Fielding`: Fielding statistics
- `Franchise`: Team/franchise
- `League`: Baseball league
- `Ballpark`: Stadium/venue
- `GameLineup`: Game batting order
- `GameStatistics`: Team-level game statistics

**Messages** (NServiceBus):
- `GameLogStart`: Initiate game log processing
- `GameLogComplete`: Signal successful completion
- `GameLogCancel`: Cancel/rollback processing

**File Formats** (Retrosheet):
- `GameLog`: Retrosheet game log file format
- `BioFile`: Retrosheet biographical file format
- Mapping classes for CsvHelper

**Configuration**:
- `RetrosharpConfiguration`: Application configuration
- `ContainerRegistration`: Dependency injection auto-registration

---

### 5. ETL Engine (`Retrosharp.Engine.Console`)

**Technology**: .NET 10 Console App, NServiceBus, RabbitMQ  
**Responsibilities**:
- Process ETL messages asynchronously
- Orchestrate long-running workflows via sagas
- Parse Retrosheet data files
- Transform and load data into database
- Handle errors and retries
- Provide observability through logging

**Key Components**:

**Sagas**:
- `GameLogSaga`: Orchestrates game log file processing workflow
  - Receives `GameLogStart` message
  - Parses file via `GameLogFileService`
  - Transforms data via `GameService`
  - Sends `GameLogComplete` on success

**Message Handlers**:
- Saga handlers implement `IAmStartedByMessages<T>` and `IHandleMessages<T>`

**Configuration**:
- NServiceBus endpoint configuration
- RabbitMQ transport
- Error queue and audit queue
- Recoverability policies (exponential backoff)
- Saga persistence

**Key Patterns**:
- **Saga Pattern**: Long-running process coordination
- **Message-Driven**: Event-sourced workflow
- **Idempotency**: Process same message multiple times safely
- **Retry with Backoff**: Automatic retry on transient failures
- **Dead Letter Queue**: Manual intervention for persistent failures

**Dependencies**:
- NServiceBus
- NServiceBus.RabbitMQ
- NServiceBus.Persistence.Sql
- Retrosharp.Service (business logic)
- Retrosharp.Data (data access)

---

### 6. Database (`SQL Server`)

**Technology**: SQL Server (LocalDB for dev, full SQL Server for production)  
**Schema**: Code-first via Entity Framework Core migrations

**Tables** (10):
- `Person`: People (players, managers, umpires, coaches)
- `Franchise`: Teams/franchises
- `League`: Baseball leagues
- `Ballpark`: Stadiums/venues
- `Game`: Baseball games
- `GameLineup`: Game batting orders
- `GameStatistics`: Team-level game statistics
- `Batting`: Player batting statistics by season
- `Pitching`: Player pitching statistics by season
- `Fielding`: Player fielding statistics by season

**Normalization**: Third Normal Form (3NF)
- Minimal redundancy
- Foreign key integrity
- Proper indexing for performance

**Relationships**:
- Person has many Batting, Pitching, GameLineup entries
- Game belongs to two Franchises (home and visitor)
- Game belongs to one Ballpark
- Game has many People (managers, umpires, pitchers)
- Franchise belongs to one League
- Franchise has many Games, BattingRecords, PitchingRecords

---

### 7. Message Bus (RabbitMQ)

**Technology**: RabbitMQ  
**Transport**: NServiceBus.RabbitMQ

**Message Types**:
- **Command Messages**: GameLogStart, GameLogCancel
- **Event Messages**: GameLogComplete

**Queues**:
- **Main Queue**: `Retrosharp.Engine` (endpoint queue)
- **Error Queue**: `error` (failed messages)
- **Audit Queue**: `audit` (successfully processed messages)

**Patterns**:
- **Publish/Subscribe**: Event notifications
- **Send/Receive**: Command handling
- **Dead Letter Queue**: Failed message handling

---

## Cross-Cutting Concerns

### Dependency Injection

**Pattern**: Constructor Injection  
**Container**: Microsoft.Extensions.DependencyInjection  
**Registration**: Automatic discovery via `IRegister` interface

Each project provides an `IocRegistrations` class:
```csharp
public class IocRegistrations : IRegister
{
	public async Task Register(IServiceCollection services)
	{
		// Register services, repositories, etc.
	}
}
```

The `ContainerRegistration.RegisterContainer()` method scans all assemblies and invokes registration classes.

### Logging

**Framework**: Microsoft.Extensions.Logging  
**Sinks**: Console, Debug (configurable for file, Application Insights, etc.)

**Usage**:
```csharp
public class PersonService
{
	private readonly ILogger<PersonService> _logger;

	public PersonService(ILogger<PersonService> logger)
	{
		_logger = logger;
	}

	public async Task DoWork()
	{
		_logger.LogInformation("Processing started");
		_logger.LogWarning("Something unexpected");
		_logger.LogError(ex, "An error occurred");
	}
}
```

### Object Mapping

**Library**: Mapster  
**Usage**: Map between database models and domain entities

**Configuration**: Convention-based (auto-mapping by property name)

**Example**:
```csharp
// Repository uses ProjectToType<T>() for query projection
var people = await Context.People
	.Where(p => p.Surname.Contains(searchTerm))
	.ProjectToType<Person>()  // Maps PersonModel -> Person
	.ToListAsync();

// Service uses mapper for object transformation
var model = Mapper.Map<PersonModel>(person);
```

### Error Handling

**Patterns**:
- Try/catch with specific exception types
- Transaction rollback on error
- Logging all exceptions
- Return null for not found (repositories)
- Throw exceptions for validation failures (services)

**NServiceBus Error Handling**:
- Immediate retries (configurable)
- Delayed retries with exponential backoff
- Move to error queue after max retries
- Manual intervention via ServicePulse or custom tooling

### Configuration

**Source**: appsettings.json  
**Access**: IConfiguration interface  
**Custom**: RetrosharpConfiguration class

**Structure**:
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=...;Database=Retrosharp;..."
  },
  "RabbitMQ": {
	"ConnectionString": "host=localhost;..."
  },
  "Logging": {
	"LogLevel": {
	  "Default": "Information",
	  "NServiceBus": "Information"
	}
  }
}
```

---

## Data Flow Examples

### Example 1: Player Search

```
1. Client HTTP GET /api/players/search?name=Ruth
2. PlayersController.Search(name: "Ruth")
3. PersonService.SearchByNameAsync("Ruth")
4. PersonRepository.SearchByNameAsync("Ruth")
5. EF Core executes SQL query with LIKE
6. Database returns PersonModel records
7. Mapster projects to Person entities
8. Returns to service -> controller -> client as JSON
```

### Example 2: ETL Game Log Processing

```
1. Admin HTTP POST /api/admin/etl/gamelog { "file": "GL2023.TXT" }
2. AdminController sends GameLogStart message
3. NServiceBus sends to RabbitMQ queue
4. Engine.Console receives message
5. GameLogSaga.Handle(GameLogStart) invoked
6. GameLogFileService.ParseFileAsync() reads CSV
7. CsvHelper parses file to GameLog objects
8. GameService.ProcessGameLogsAsync() transforms data
9. For each game:
   - Look up franchise IDs
   - Look up person IDs (managers, umpires)
   - Look up ballpark ID
   - Check if game already exists (idempotency)
   - Create Game entity
   - GameRepository.CreateAsync() saves to database
10. Saga sends GameLogComplete message
11. Saga marks as complete
12. Message moved to audit queue
```

### Example 3: Career Statistics Calculation

```
1. Client HTTP GET /api/players/123/stats/career
2. PlayersController.GetCareerStats(id: 123)
3. PlayerStatisticsService.CalculateCareerBattingStatsAsync(123)
4. BattingRepository.GetByPersonIdAsync(123)
5. EF Core query retrieves all Batting records for person
6. Service aggregates statistics:
   - Sum all AtBats, Hits, Homeruns, etc.
   - Creates BattingStatistics object
7. BattingStatistics properties calculate advanced metrics:
   - AVG = Hits / AtBats
   - OBP = (H+BB+HBP) / (AB+BB+HBP+SF)
   - SLG = TotalBases / AtBats
   - BABIP, ISO, OPS calculated
8. Return BattingStatistics -> controller -> client as JSON
```

---

## Security Considerations

### Authentication & Authorization
- JWT bearer tokens (configured in API)
- Microsoft Identity platform integration (Azure AD)
- Admin endpoints require elevated privileges
- Future: Role-based access control (RBAC)

### Data Protection
- Sensitive data in configuration (connection strings) via User Secrets or environment variables
- HTTPS enforced for API endpoints
- SQL injection prevention via parameterized queries (EF Core)

### API Security
- Input validation on all endpoints
- Rate limiting (future)
- CORS configuration for allowed origins
- API versioning for backward compatibility

---

## Performance Considerations

### Database
- **Indexes**: Composite indexes on commonly queried columns
- **Connection Pooling**: EF Core default behavior
- **Lazy Loading**: Disabled to prevent N+1 queries
- **Projections**: Use ProjectToType<T>() to select only needed columns
- **Batching**: Bulk inserts for ETL operations (future optimization)

### Caching
- **Current**: None implemented
- **Future**:
  - Response caching for GET endpoints
  - Distributed cache (Redis) for frequently accessed statistics
  - In-memory cache for lookup tables (franchises, leagues, ballparks)

### Async Operations
- All I/O operations use async/await
- Prevents thread blocking
- Scales better under load

### ETL Performance
- **Current**: Process games sequentially
- **Future Optimizations**:
  - Parallel processing with Parallel.ForEachAsync
  - Batch database inserts
  - Streaming file parsing for large files
  - Progress reporting

---

## Scalability

### Horizontal Scaling
- **API**: Stateless, can run multiple instances behind load balancer
- **ETL Engine**: Can run multiple competing consumers
- **Database**: Read replicas for analytics queries
- **Message Bus**: RabbitMQ clustering for high availability

### Vertical Scaling
- Increase CPU/RAM for database server
- Increase worker threads for ETL engine
- Optimize queries and indexes

---

## Monitoring & Observability

### Logging
- Structured logging with Microsoft.Extensions.Logging
- Log levels: Trace, Debug, Information, Warning, Error, Critical
- Centralized log aggregation (future: Application Insights, ELK stack)

### Metrics
- NServiceBus metrics (message throughput, errors, retries)
- API metrics (request count, latency, error rate)
- Database metrics (connection count, query performance)

### Health Checks
- API health endpoint
- Database connectivity check
- RabbitMQ connectivity check
- Future: Readiness and liveness probes for Kubernetes

---

## Deployment Architecture

### Development
```
┌─────────────────┐
│  Developer PC   │
│  - SQL LocalDB  │
│  - RabbitMQ     │
│    (Docker)     │
│  - API (IIS Ex) │
│  - Engine (CLI) │
└─────────────────┘
```

### Production (Containerized)
```
┌───────────────────────────────────────────┐
│             Load Balancer                 │
└───────┬────────────────────┬──────────────┘
		│                    │
┌───────▼──────┐    ┌────────▼──────┐
│  API (Pod 1) │    │  API (Pod 2)  │
└───────┬──────┘    └────────┬──────┘
		│                    │
		└──────────┬─────────┘
┌──────────────────▼──────────────────┐
│         RabbitMQ Cluster            │
└──────────────────┬──────────────────┘
				   │
┌──────────────────▼──────────────────┐
│    ETL Engine (Consumer Group)      │
│  ┌──────────┐  ┌──────────┐         │
│  │Engine-1  │  │Engine-2  │         │
│  └──────────┘  └──────────┘         │
└──────────────────┬──────────────────┘
				   │
┌──────────────────▼──────────────────┐
│       SQL Server (Primary)          │
│   ┌────────────────────────┐        │
│   │  Read Replica (Future) │        │
│   └────────────────────────┘        │
└─────────────────────────────────────┘
```

---

## Technology Stack Summary

| Layer | Technology | Version |
|-------|-----------|---------|
| Runtime | .NET | 10.0 |
| Language | C# | 13.0 |
| API Framework | ASP.NET Core | 10.0 |
| ORM | Entity Framework Core | 10.0.8 |
| Database | SQL Server | 2022+ |
| Message Bus | RabbitMQ | 3.x |
| Messaging Framework | NServiceBus | 10.2.0 |
| Object Mapping | Mapster | 10.0.7 |
| CSV Parsing | CsvHelper | Latest |
| Logging | Microsoft.Extensions.Logging | 10.0.8 |
| Dependency Injection | Microsoft.Extensions.DependencyInjection | 10.0.8 |
| Containerization | Docker | Latest |
| Orchestration | Docker Compose / Kubernetes (future) | Latest |

---

## Future Enhancements

### Phase 2 Features (from specification)
- Single Sign-On (Google, Facebook)
- User accounts and authentication
- Favorite players/teams
- Administrative UI for data management
- Negro Leagues dedicated section
- ETL activity feed/dashboard
- Roles and permissions

### Technical Improvements
- Unit testing (xUnit)
- Integration testing
- API versioning
- GraphQL endpoint (alternative to REST)
- Real-time updates via SignalR
- Advanced caching strategy
- Read replicas for reporting
- Kubernetes deployment
- CI/CD pipeline
- Automated database backups
- Disaster recovery plan

---

**For implementation details, see [implementation-plan.md](./implementation-plan.md) and [progress.md](./progress.md)**
