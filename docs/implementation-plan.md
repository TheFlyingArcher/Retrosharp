# Retrosharp MVP Implementation Plan

## Overview
This document outlines the complete implementation plan for the Retrosharp MVP (Minimum Viable Product) as defined by the initial acceptance criteria in the project specification.

## Project Understanding
Implement the initial acceptance criteria for Retrosharp, a baseball statistics application that imports and processes Retrosheet data. The system requires:

1. A complete EF Core database schema with 3NF normalization
2. ETL processing via NServiceBus/RabbitMQ to import Retrosheet data files
3. Statistical calculations for hitting/pitching/fielding
4. Player search functionality
5. Containerized deployments for API and ETL engine
6. Robust error handling with retries and dead letter queue support

## Key Assumptions
- SQL Server database is available (connection string configured in appsettings.json)
- RabbitMQ is available for NServiceBus message transport
- Retrosheet data files follow documented formats (game logs, bio files)
- The existing models and file parsers (GameLog, BioFile) are accurate for Retrosheet format
- Docker is used for containerization
- Mapster is the chosen mapping library (already referenced)
- Statistics need to be calculated from raw game/season data, not stored pre-computed
- Player search will be REST API endpoints returning JSON

## Implementation Approach
The implementation follows a bottom-up approach starting with the data layer, then business logic, then presentation:

1. **Data Layer**: Complete the database schema with all necessary models, relationships, and migrations
2. **Repository Pattern**: Implement the repository pattern for all entities
3. **Service Layer**: Build out service layer with ETL processing logic and statistical calculations
4. **Message Bus**: Configure NServiceBus in the Engine.Console project to handle saga-based ETL workflows with retry policies and DLQ
5. **API Layer**: Create REST API controllers for player search and statistics retrieval
6. **Deployment**: Ensure both the API and Engine projects have proper Dockerfiles and can be independently deployed

### Architectural Principles
- Use EF Core code-first with proper foreign keys and navigation properties
- Implement idempotent ETL handlers that check for existing records
- Calculate statistics on-demand in service layer from raw data
- Use NServiceBus saga pattern for long-running ETL workflows
- Implement exponential backoff with jitter using NServiceBus recoverability policies

## Key Files and Their Roles

### Data Layer
- **[lib/Retrosharp.Data/Context/RetrosharpContext.cs](../src/lib/Retrosharp.Data/Context/RetrosharpContext.cs)** - EF Core DbContext with DbSet properties and relationship configuration
- **[lib/Retrosharp.Data/Model/](../src/lib/Retrosharp.Data/Model/)** - Database models with navigation properties and proper 3NF relationships
- **[lib/Retrosharp.Data/](../src/lib/Retrosharp.Data/)** - Repository implementations for each entity type

### Service Layer
- **[lib/Retrosharp.Service/](../src/lib/Retrosharp.Service/)** - Service layer implementations for business logic and ETL
- **[lib/Retrosharp.Service/ETL/](../src/lib/Retrosharp.Service/ETL/)** - ETL file parsing services

### ETL Engine
- **[engine/Retrosharp.Engine.Console/Program.cs](../src/engine/Retrosharp.Engine.Console/Program.cs)** - NServiceBus endpoint configuration with RabbitMQ transport
- **[engine/Retrosharp.Engine.Console/Saga/GameLogSaga.cs](../src/engine/Retrosharp.Engine.Console/Saga/GameLogSaga.cs)** - Saga handlers for ETL workflow orchestration

### API Layer
- **[ui/Retrosharp.UI.Api/Program.cs](../src/ui/Retrosharp.UI.Api/Program.cs)** - API configuration including NServiceBus client for message sending
- **[ui/Retrosharp.UI.Api/Controllers/](../src/ui/Retrosharp.UI.Api/Controllers/)** - REST API controllers for player search and statistics

## Risks & Open Questions
1. **Retrosheet Data Format**: Edge cases in Retrosheet data format may not be covered in initial parsing logic
2. **Large File Processing**: May require streaming/batching approach to avoid memory issues with large files
3. **Database Indexing**: Performance testing needed to validate indexing strategy for search operations
4. **NServiceBus Licensing**: Currently using community/trial - need to verify licensing for production use
5. **Migration Strategy**: Container deployment needs automated migration application strategy
6. **Fielding Statistics**: Calculation complexity may vary by position-specific metrics
7. **Performance**: Statistics calculations on large datasets may need caching strategy in future phases

## Implementation Steps

### Step 1: Complete Database Schema ✅ COMPLETED
**Objective**: Update all DbModel classes with proper navigation properties and configure relationships in RetrosharpContext

**Tasks**:
- [x] Update all DbModel classes with proper navigation properties
- [x] Configure relationships in RetrosharpContext.OnModelCreating
- [x] Add missing DbSet properties to RetrosharpContext
- [x] Add indices for common query patterns (player search, franchise lookups)

**Deliverables**:
- RetrosharpContext with complete DbSet properties and relationship configuration
- All models have bidirectional navigation properties
- Composite indexes for optimal query performance

---

### Step 2: Create EF Core Migrations ✅ COMPLETED
**Objective**: Generate initial migration for database schema

**Tasks**:
- [x] Add EF Core design-time tools to Data.Migration project
- [x] Create RetrosharpContextFactory for design-time DbContext creation
- [x] Generate initial migration
- [x] Test migration applies successfully

**Deliverables**:
- RetrosharpContextFactory for migrations
- InitialCreate migration file
- Ability to apply migrations via `dotnet ef database update`

---

### Step 3: Implement Missing Repositories ✅ COMPLETED
**Objective**: Create repository implementations for all entities

**Tasks**:
- [x] Create BattingRepository, PitchingRepository, FieldingRepository
- [x] Create PersonRepository with search capabilities
- [x] Create FranchiseRepository, LeagueRepository, BallparkRepository
- [x] Create GameStatisticsRepository
- [x] Register all repositories in IocRegistrations

**Deliverables**:
- Complete repository implementations for all 10 entities
- Search methods for PersonRepository (by name, by Retrosheet ID)
- Entity-specific query methods (franchise by code, league by code, etc.)
- All repositories registered for dependency injection

---

### Step 4: Implement Service Layer ✅ COMPLETED
**Objective**: Build business logic services and ETL file processing

**Tasks**:
- [x] Complete GameLogFileService to transform GameLog records to database models
- [x] Complete BioFileService to transform biographical data to PersonModel
- [x] Create GameService for game-related business logic
- [x] Create BattingService, PitchingService for statistics calculations
- [x] Create PlayerService for player search and career stats aggregation
- [x] Register all services in IocRegistrations

**Deliverables**:
- PersonService with search and career statistics
- GameService with ETL processing logic for game logs
- BattingService with statistics retrieval
- PlayerStatisticsService with advanced metrics calculation
- Complete BioFileService and GameLogFileService implementations

---

### Step 5: Configure NServiceBus in Engine.Console ⏳ IN PROGRESS
**Objective**: Set up NServiceBus endpoint with RabbitMQ transport and proper configuration

**Tasks**:
- [ ] Set up NServiceBus endpoint with RabbitMQ transport
- [ ] Configure routing for GameLog messages
- [ ] Configure error queue and audit queue
- [ ] Configure recoverability with exponential backoff and jitter
- [ ] Implement proper logging
- [ ] Create appsettings.json with connection strings and NServiceBus configuration

**Deliverables**:
- Program.cs with NServiceBus host builder configuration
- RabbitMQ transport configuration
- Error handling and retry policies
- Logging configuration

---

### Step 6: Implement GameLogSaga Handlers 🔜 PENDING
**Objective**: Complete saga implementation for ETL workflow orchestration

**Tasks**:
- [ ] Handle GameLogStart: parse file, validate, transform to models
- [ ] Implement idempotent insert/update logic
- [ ] Handle errors with proper logging
- [ ] Send GameLogComplete or handle cancellation
- [ ] Ensure saga data persistence
- [ ] Implement lookups for franchises, people, ballparks by their codes
- [ ] Handle duplicate game detection

**Deliverables**:
- Complete GameLogSaga implementation
- Idempotent ETL processing
- Proper error handling and logging
- Integration with GameService and FileService

---

### Step 7: Create REST API Controllers for Players 🔜 PENDING
**Objective**: Build API endpoints for player operations

**Tasks**:
- [ ] Create PlayersController with search endpoint
- [ ] Create endpoint for player career statistics
- [ ] Create endpoint for player season statistics
- [ ] Add proper error handling and validation
- [ ] Add XML documentation for Swagger/OpenAPI

**Deliverables**:
- PlayersController with:
  - GET /api/players/search?name={searchTerm}
  - GET /api/players/{id}
  - GET /api/players/{id}/stats/career
  - GET /api/players/{id}/stats/season/{year}

---

### Step 8: Create REST API Controllers for Games and Teams 🔜 PENDING
**Objective**: Build API endpoints for game and team operations

**Tasks**:
- [ ] Create GamesController for game retrieval
- [ ] Create TeamsController for franchise statistics
- [ ] Add filtering and pagination support
- [ ] Add XML documentation for Swagger/OpenAPI

**Deliverables**:
- GamesController with:
  - GET /api/games?date={date}
  - GET /api/games/{id}
  - GET /api/games?franchiseId={id}
- TeamsController with:
  - GET /api/teams
  - GET /api/teams/{id}
  - GET /api/teams/{id}/stats

---

### Step 9: Configure NServiceBus in UI.Api 🔜 PENDING
**Objective**: Set up send-only endpoint for initiating ETL jobs

**Tasks**:
- [ ] Set up NServiceBus send-only endpoint
- [ ] Create controller for initiating ETL jobs (admin endpoint)
- [ ] Configure routing to Engine endpoint
- [ ] Add authorization for admin endpoints

**Deliverables**:
- AdminController with:
  - POST /api/admin/etl/gamelog (initiate game log processing)
  - POST /api/admin/etl/bio (initiate biographical data import)
- NServiceBus configuration in API project

---

### Step 10: Docker Configuration 🔜 PENDING
**Objective**: Ensure containerized deployment readiness

**Tasks**:
- [ ] Verify Dockerfile for Retrosharp.UI.Api
- [ ] Verify Dockerfile for Retrosharp.Engine.Console
- [ ] Create docker-compose.yml with SQL Server and RabbitMQ services
- [ ] Add healthcheck endpoints
- [ ] Test local deployment with docker-compose

**Deliverables**:
- Updated Dockerfiles for both applications
- docker-compose.yml for complete stack
- Health check endpoints
- Documentation for running locally with Docker

---

### Step 11: Integration Tests 🔜 PENDING
**Objective**: Validate ETL processing end-to-end

**Tasks**:
- [ ] Test GameLogFileService parsing with sample files
- [ ] Test GameLogSaga workflow end-to-end
- [ ] Test idempotency of ETL handlers
- [ ] Test error handling and retry logic

**Deliverables**:
- Integration test project
- Test fixtures with sample Retrosheet data
- Tests for complete ETL workflow
- Tests for error scenarios

---

### Step 12: End-to-End Testing 🔜 PENDING
**Objective**: Validate complete system functionality

**Tasks**:
- [ ] Build solution and fix any compilation errors
- [ ] Test database migrations
- [ ] Test API endpoints
- [ ] Test ETL message processing
- [ ] Validate statistics calculations
- [ ] Performance testing with realistic data volumes

**Deliverables**:
- Successful build
- All tests passing
- Documentation of any known issues or limitations
- Performance benchmarks

---

## Success Criteria

The MVP will be considered complete when:

1. ✅ Database schema is complete with all entities and relationships
2. ✅ EF Core migrations can create the database from scratch
3. ✅ All repositories are implemented with proper query methods
4. ✅ Service layer provides business logic and ETL processing
5. ⏳ NServiceBus endpoint processes messages from RabbitMQ
6. 🔜 GameLogSaga successfully imports game data with idempotency
7. 🔜 BioFile import works end-to-end
8. 🔜 Player search API returns accurate results
9. 🔜 Statistics API calculates and returns advanced metrics
10. 🔜 Both API and Engine can be deployed in containers
11. 🔜 ETL process handles errors gracefully with retries
12. 🔜 System can process realistic data volumes without issues

## Next Steps

Continue with **Step 5** to complete NServiceBus configuration in the Engine.Console project. This involves:

1. Creating a proper Program.cs with NServiceBus host builder
2. Configuring RabbitMQ transport with connection string from appsettings
3. Setting up error queue, audit queue, and recoverability policies
4. Registering all dependencies (DbContext, repositories, services)
5. Configuring logging for NServiceBus operations

See [progress.md](./progress.md) for detailed status of completed work.
