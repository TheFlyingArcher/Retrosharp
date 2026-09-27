# Retrosharp

Retrosharp is a baseball history database. It imports the play-by-play records, game logs, franchises, and people published by [Retrosheet](https://www.retrosheet.org), and turns them into something you can browse and explore: seasons, standings, box scores, and batting, pitching, and fielding statistics for every player in its database.

## Why Retrosharp?

Retrosheet is a volunteer project that has spent decades reconstructing major league games, often one scorecard at a time. Its data is freely available, but it comes as raw files in formats built for researchers, not readers. Retrosharp is a personal project to bring that data into a proper database and give it a friendly face.

Retrosharp is about **historical context**. It is not trying to compete with [FanGraphs](https://www.fangraphs.com) or [Baseball Reference](https://www.baseball-reference.com). Those sites have real-time feeds of today's games and statistics, which Retrosharp doesn't have access to and isn't built for. Retrosharp stays on the seasons Retrosheet has already documented, and tries to make them easy and enjoyable to look through.

## What it imports

| Data | Source | What it gives Retrosharp |
|---|---|---|
| Franchises and ballparks | Retrosheet reference files | Teams through relocations and renames, and where they played |
| People | Retrosheet's biographical file | Players, coaches, managers, and umpires |
| Game logs | Retrosheet's season game logs | One record per game: date, park, lineups, umpires, and team totals |
| Game events | Retrosheet's play-by-play event files | Every play of every game, from which individual batting, pitching, and fielding statistics are derived |

Retrosharp downloads each season's files from Retrosheet itself, so importing a season is a single request rather than a manual download.

## What you can do with it

- Look up players and see their career statistics and game logs.
- Browse seasons, standings, and franchise histories.
- Open any imported game and see its box score and play-by-play.
- See statistics that can be worked out from the data itself: AVG, OBP, SLG, OPS, BABIP, ERA, WHIP, FIP, K/9, BB/9, HR/9, fielding percentage, and more.

Advanced statistics that need outside reference data (such as wRC+ and WAR), postseason games, and a dedicated Negro Leagues section are planned for a later phase. See [spec/project.md](spec/project.md) for the full roadmap.

## Architecture

Retrosharp has two halves: an **import engine** that loads Retrosheet data in the background, and a **web application** for exploring it.

```
                ┌──────────────────────┐
                │   www.retrosheet.org │
                └──────────┬───────────┘
                           │ season archives
┌──────────────┐   ┌───────▼──────────────────┐
│   RabbitMQ   │──▶│  Engine.Console (ETL)    │
│ message bus  │   │  parsers run as sagas    │
└──────▲───────┘   └───────┬──────────────────┘
       │                   │ writes
       │ start import      ▼
┌──────┴───────┐   ┌──────────────────────────┐
│ UI.Api (REST)│◀─▶│       PostgreSQL         │
└──────▲───────┘   └──────────────────────────┘
       │ JSON
┌──────┴───────┐
│ UI.Web       │
│ (Angular)    │
└──────────────┘
```

- **Import engine (`Retrosharp.Engine.Console`).** A background service that listens on a message bus. Each import (people, a season's game logs, a season's play-by-play) runs as its own long-running workflow, or saga. It downloads the Retrosheet archive, parses it, and writes to the database. Imports are asynchronous, retry with backoff when something isn't ready yet, and are safe to rerun without creating duplicates.
- **Import order.** Retrosheet data depends on itself, so imports run in stages: reference data (franchises, ballparks), then people, then game logs, then play-by-play. Files within a stage can be processed at the same time.
- **Data layer (`Retrosharp.Data`).** Entity Framework Core, code-first, with a normalized schema and the repository pattern. A one-shot migration container (`Retrosharp.Data.Migration`) creates the schema and loads the reference data before anything else starts.
- **Service layer (`Retrosharp.Service`).** Business logic and statistics calculations, kept separate from both the database and the API.
- **Web API (`Retrosharp.UI.Api`).** An ASP.NET Core REST API for players, games, teams, seasons, and standings, and for starting imports.
- **Web front end (`Retrosharp.UI.Web`).** An Angular single-page application with light and dark themes.

More detail is in [docs/architecture.md](docs/architecture.md) and the design specs under [spec/](spec).

## Technology stack

| Area | Technology |
|---|---|
| Language and runtime | C# on .NET 10 |
| Database | PostgreSQL 16, via Entity Framework Core 10 (Npgsql) |
| Messaging and workflows | NServiceBus 10 on RabbitMQ, with SQL persistence for sagas |
| Parsing | CsvHelper, plus custom parsers for Retrosheet's event file format |
| Object mapping | Mapster |
| Web API | ASP.NET Core with OpenAPI |
| Front end | Angular 22, Angular Material, Bootstrap 5, TypeScript |
| Testing | xUnit, NServiceBus.Testing |
| Deployment | Docker Compose; runs on x64 and ARM64, including a 4 GB Raspberry Pi |

## Project layout

```
src/
  engine/   Retrosharp.Engine.Console        import engine (and tests)
  lib/      Retrosharp                       Retrosheet file formats and parsers
            Retrosharp.Data                  EF Core model and repositories
            Retrosharp.Data.Migration        schema migrations and seed data
            Retrosharp.Service(.Interface)   business logic and statistics
  ui/       Retrosharp.UI.Api                REST API
            Retrosharp.UI.Web                Angular front end
spec/       design specs and the project roadmap
docs/       architecture, deployment, and implementation notes
```

## Getting started

The back end runs as a set of Docker containers: PostgreSQL, RabbitMQ, the migration job, the import engine, and the API.

```bash
cp .env.example .env
```

Set real values for the passwords in `.env` (it is git-ignored), then start the stack:

```bash
docker compose up -d --build
```

The API is then available at `http://localhost:5197`. To run the front end during development:

```bash
cd src/ui/Retrosharp.UI.Web
npm install
npm start
```

See [docs/deployment.md](docs/deployment.md) for full setup, importing data, and running on a Raspberry Pi.

## Contributing

`main` is branch-protected, so all changes go through a pull request. See [AGENTS.md](AGENTS.md) and the [pull request template](.github/pull_request_template.md).

## License

Retrosharp is licensed under the [GNU General Public License v3.0](LICENSE).

## Data attribution

All baseball data in Retrosharp comes from Retrosheet. As Retrosheet requires:

> The information used here was obtained free of charge from and is copyrighted by Retrosheet. Interested parties may contact Retrosheet at 20 Sunset Rd., Newark, DE 19711.

Retrosheet is an all-volunteer, 501(c)(3) organization. If you find its work valuable, consider [volunteering or donating](https://www.retrosheet.org).
