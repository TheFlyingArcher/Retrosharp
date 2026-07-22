# Retrosharp

## Overview

Retrosharp is a baseball database and statistics application which sources its data from the [Retrosheet](https://www.retrosheet.org) project.
Retrosharp is designed to be a simple, easy-to-use application for baseball enthusiasts who want to explore historical baseball data and statistics.
Retrosharp operates by downloading and parsing Retrosheet data files, which contain detailed information about baseball games, players, and teams.
The application provides a user-friendly interface for browsing and analyzing this data.

## Features

1. **Data Import** *(Phase 1)*: Retrosharp can import Retrosheet data files, which include game logs, player statistics, and team information.
1. **Player Search** *(Phase 1)*: Users can search for players by name, team, or position to view their career statistics and game logs.
1. **Data Viewing** *(Phase 1)*: Users can view imported Retrosheet data in a concise format, including game summaries, individual play-by-play events within a game, and batting, pitching, and fielding statistics for individual players and teams.
1. **Statistics** *(Phase 1)*: The application calculates statistics derivable directly from imported data, without requiring external reference data such as park factors or replacement-level baselines. This includes AVG, OBP, SLG, OPS, BABIP (for both batters and pitchers), WHIP, ERA, FIP, HR/FB, K/9, HR/9, BB/9, and FP, among others.
1. **Advanced Statistics** *(Phase 2)*: Statistics requiring external reference data or more involved calculations, such as wRC+ and WAR, are deferred.
1. **Player Export** *(Phase 2)*: Users can export statistical data of players to CSV formats so that they may be used in other applications or for further analysis.
1. **Team Analysis** *(Phase 2)*: Users can analyze team performance over multiple seasons, comparing statistics and trends across different years. This is distinct from viewing a team's statistics for a single game or season, which is covered by Data Viewing above.
1. **Administrative** *(Phase 2)*: The application includes administrative features for managing the database, such as adding new players, updating existing records, importing datasets, and maintaining data integrity.
1. **Single Sign On** *(Phase 2)*: Retrosharp supports single sign-on (SSO) authentication, allowing users to log in using their existing credentials from supported identity providers.

## Architecture

Retrosharp is an n-tiered web application with standard data, service (business logic), and presentation layers with logical separations. Retrosharp is built with .NET 10 and C# 13.0, and it uses a SQL Server database for data storage. The application is designed to be modular and extensible, allowing for future enhancements and additional features. The Retrosharp ETL process is governed by a service bus using the NServiceBus library backed by RabbitMQ, so that processing Retrosheet datafiles is asynchronous, scalable, and provides detailed logging of the ETL process. The Retrosharp front end is an Angular 17 application that communicates with the backend via RESTful APIs. The application is designed to be responsive and accessible, ensuring a seamless user experience across different devices and screen sizes.

### Data Layer

The data layer is responsible for managing the database and providing data access to the service layer. The data layer uses Entity Framework Core as the Object Relational Mapping (ORM) framework to interact with the SQL Server database.The data layer defines the database schema, relationships, and constraints using a code-first approach, ensuring that the database is created and maintained based on the application's data model. The data layer uses the repository pattern so that the service layer can interact with the database through a set of well-defined interfaces, promoting separation of concerns and testability. This also allows abstraction of the underlying database technology, making it easier to switch to a different database provider in the future if needed.

### Data Import Pipeline

Retrosheet data is imported through a series of ETL parsers, each responsible for a specific Retrosheet datafile and a specific set of database tables. These parsers must run in the following dependency order, though multiple files within the same stage may be processed concurrently with one another:

1. **Seed Data** populates the `League`, `Franchise`, and `Ballpark` tables from static Retrosheet reference data. See [seed-data.md](./seed-data.md).
1. **Person Parser** populates the `Person` table (players, managers, umpires, and coaches) from Retrosheet's biographical file (the "biofile"). See [person.md](./person.md).
1. **Game Log Parser** populates the `Game`, `GameLineup`, `GameBattingStatistics`, `GamePitchingStatistics`, and `GameFieldingStatistics` tables from Retrosheet's season-wide Game Logs files, which contain one row of aggregate, team-level statistics per game across all teams. See [game-log.md](./game-log.md).
1. **Game Event Parser** populates the `GameEvent` table, and derives the player-level `Batting`, `Pitching`, and `Fielding` tables, from Retrosheet's team-season play-by-play event files. See [game-event.md](./game-event.md).

`Game` is populated exclusively by the Game Log Parser. The Game Event Parser requires a `Game` record to already exist before it can associate play-by-play events with it, and never creates `Game` records itself.

## Deployable Components

### Retrosharp.Engine.Console

This is a console application which is intended to run in the background. This application receives incoming messages on a service bus. The main purpose of this application is to process each of the Retrosheet datafiles and populate the Retrosharp database. Each datafile is its own NServiceBus saga. The underlying service bus is RabbitMQ.

### Retrosharp.UI.Api

This is an ASP.NET Web API providing RESTful API endpoints for the Retrosharp front end. The API is responsible for handling requests from the front end, processing data, and returning responses in JSON format.
The API also handles authentication and authorization, ensuring that only authorized users can access certain endpoints.

### Retrosharp.UI.Web

This is an Angular 17 application that serves as the front end for Retrosharp. The front end provides a user-friendly interface for browsing and analyzing baseball data and statistics.

## Project Phases

### Initial Phase

This phase is to get a minimum viable product (MVP) of Retrosharp up and running. The MVP will include the following features:

1. Database setup: The application should have a SQL Server database set up with the necessary tables and relationships to store Retrosheet data.
	1. Use a code-first approach with Entity Framework Core to define the database schema and relationships.
	1. Use third normal form (3NF) to ensure data integrity and minimize redundancy wherever possible.
1. Retrosheet data import: The application should be able to import Retrosheet data files, including game logs, player statistics, and team information.
1. Statistical calculations based on raw data: The application should be able to calculate statistics derivable directly from imported data, without requiring external reference data such as park factors or replacement-level baselines. This includes hitting, pitching, and fielding statistics such as AVG, OBP, SLG, OPS, BABIP (for both batters and pitchers), WHIP, ERA, FIP, HR/FB, K/9, HR/9, BB/9, and FP, among others. Statistics requiring external reference data or more involved calculations, such as wRC+ and WAR, are deferred to a later phase.
1. Player search functionality: Users should be able to search for players by name, team, or position and view their career statistics and game logs.
1. Data viewing functionality: Users should be able to view imported Retrosheet data in a concise format, including game summaries, individual play-by-play events within a game, and batting, pitching, and fielding statistics for individual players and teams.
1. Retrosharp API and Retrosharp UI/UX are each independently deployable and can be deployed to a cloud provider of choice using containers.
1. ETL processes are initiated by receving messages on a service bus with the following information:
	1. The Retrosheet datafile to be processed
	1. The type of datafile, and the location of the datafile.
	1. The ETL process should be asynchronous and scalable, allowing for multiple datafiles to be processed simultaneously, subject to the data dependency order below.
1. Retrosheet data has a dependency order that must be respected, as described in [Data Import Pipeline](#data-import-pipeline): seed data and `Person` must be populated before `Game` can be populated by the Game Log Parser; `Game` must be populated before its derivatives (`GameEvent`, `Batting`, `Pitching`, `Fielding`) can be populated by the Game Event Parser. ETL processes within the same stage of this order may run concurrently with one another.
	1. If a process encounters a prerequisite that has not yet been satisfied (for example, a Game Event file referencing a game not yet present in `Game`), this is a retryable condition and should be handled using the standard retry/backoff policy rather than treated as a fatal error.
	1. ETL processes that could write to the same underlying record concurrently (for example, two Game Event files that both contain the same shared game) must resolve this safely using atomic, database-enforced idempotency checks. File-level or global serialization should not be relied upon to prevent this, since it would limit the scalability of processing large volumes of historical data.
1. ETL processes should provide detailed logging of the processing steps, including any errors or warnings encountered during the process.
1. ETL processes should be idempotent, ensuring that processing the same datafile multiple times does not result in duplicate records or inconsistent data.
1. ETL processes should be able to handle large datafiles efficiently, without running out of memory or crashing the application.
1. ETL processes should be able to handle datafiles with missing or incomplete data, and should log any issues encountered during processing.
1. ETL processes should be able to retry failed operations with an exponential backoff with jitter and eventually failing to a dead letter queue for manual intervention.
1. ETL processing should be its own application running in the background receiving messages from the service bus and processing Retrosheet datafiles asynchronously.
	1. This allows for a "fire and forget" approach to processing Retrosheet datafiles, where the user can initiate the ETL process and continue using the application without waiting for the processing to complete.

### Second Phase

This is the second phase of development, which will focus on adding more advanced features and improving the user experience.
This is to be implemented after the initial MVP is complete and stable and in production.

1. Adminstrative: The application should include administrative features for managing the database, such as adding new players, updating existing records, importing datasets, and maintaining data integrity.
1. Authentication/Authorization: The application should implement user authentication and authorization to restrict access to certain features based on user roles.
	1. Single Sign On (SSO) support: The application should support single sign-on (SSO) authentication, allowing users to log in using their existing credentials from supported identity providers.
	1. SSO providers initially Google and Facebook with capabilities to expand to other SSO providers as needed.
1. Maintain anonymous access to the application for users who do not wish to create an account or log in, while still providing access to basic features and data exploration.
1. Authenticated users can favorite players and teams, and have their preferences saved for future visits.
1. Administrative users can manage user accounts, including creating, updating, and deleting accounts, as well as assigning roles and permissions.
1. Administrative users can import new Retrosheet datasets and update existing data, ensuring that the application remains up-to-date with the latest baseball data.
1. A dedicated section for Retrosheet's Negro Leagues highlighting historical Black baseball players.
1. An activity feed of the ETL process detailing to administrator users each steps of the ETL process and reporting errors and warnings back to the user.
1. Player export: Users can export statistical data of players to CSV formats so that they may be used in other applications or for further analysis.
1. Advanced statistics requiring external reference data or more involved calculations, such as wRC+ and WAR.
1. Team analysis: Users can analyze team performance over multiple seasons, comparing statistics and trends across different years, including a resolution for how franchise relocations and renames are grouped for this purpose.
1. Ejection tracking: parse Retrosheet's structured `ej` comment records (manager/player/coach ejections — see [Ejections](https://www.retrosheet.org/eventfile.htm)) into a dedicated `GameEjection` table instead of leaving them as unstructured `GameComment` text. See [game-event.md](./game-event.md#future-enhancement-phase-2-gameejection) for the full data model and design notes. Noticed during Phase 1's Step 6c (Game Event Parser context records) but deliberately kept out of Phase 1 scope.
1. Positions played tracking: replace the unused `Batting.Positions` column with proper tracking of which position(s) a batter played per season (games/innings per position), since a single scalar column can't represent positional fluidity for utility players. See [game-event.md](./game-event.md#future-enhancement-phase-2-batting-positions-played) for design notes. Noticed during Phase 1's Step 6d (Batting/Pitching/Fielding derivation) but deliberately kept out of Phase 1 scope.
1. Team earned run and passed-ball/double-play/triple-play reconciliation: extend Step 6e's team-level reconciliation to also cover `GamePitchingStatistics.TeamEarnedRuns` and `GameFieldingStatistics.PassedBalls`/`DoublePlays`/`TriplePlays`, once the underlying derivation gaps behind them (situational earned-run attribution rules; denormalized "current catcher" tracking) are built. See [game-event.md](./game-event.md#future-enhancement-phase-2-team-earned-run-reconciliation-and-passed-balldouble-play-reconciliation) for design notes. Noticed during Phase 1's Step 6e (Reconciliation checks) but deliberately kept out of Phase 1 scope.

## Relavent Information

1. [Retrosheet](https://www.retrosheet.org) - The source of the baseball data used in Retrosharp.
1. [Retrosheet Event Format](https://www.retrosheet.org/eventfile.htm) - Documentation on the format of Retrosheet game events.
1. [Retrosheet Biographical Data](https://www.retrosheet.org/biofile.zip) - A zip file containing biographical data for players, coaches, umpires, and managers
1. [Retrosheet Negro League Data](https://www.retrosheet.org/NegroLeagues/NegroLeagues.html) - Retrosheet's page dedicating baseball data of Negro Leagues.
