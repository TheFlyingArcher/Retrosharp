# Retrosharp

## Overview

Retrosharp is a baseball database and statistics application which sources its data from the [Retrosheet](https://www.retrosheet.org) project.
Retrosharp is designed to be a simple, easy-to-use application for baseball enthusiasts who want to explore historical baseball data and statistics.
Retrosharp operates by downloading and parsing Retrosheet data files, which contain detailed information about baseball games, players, and teams.
The application provides a user-friendly interface for browsing and analyzing this data.

## Features

1. **Data Import**: Retrosharp can import Retrosheet data files, which include game logs, player statistics, and team information.
1. **Player Search**: Users can search for players by name, team, or position to view their career statistics and game logs.
1. **Advanced Statistics**: The application provides advanced statistical analysis, such as BABIP, FIP, ISO, and WAR, allowing users to gain deeper insights into player performance.
1. **Player Export**: Users can export statistical data of players to CSV formats so that they may be used in other applications or for further analysis.
1. **Game Logs**: Retrosharp allows users to search for specific games and view detailed game logs, including play-by-play data and box scores.
1. **Team Analysis**: Users can analyze team performance over multiple seasons, comparing statistics and trends across different years.
1. **Administrative**: The application includes administrative features for managing the database, such as adding new players, updating existing records, importing datasets, and maintaining data integrity.
1. **Single Sign On**: Retrosharp supports single sign-on (SSO) authentication, allowing users to log in using their existing credentials from supported identity providers.

## Architecture

Retrosharp is an n-tiered web application with standard data, service (business logic), and presentation layers with logical separations. Retrosharp is built with
.NET 10 and C# 13.0, and it uses a SQL Server database for data storage. The application is designed to be modular and extensible,
allowing for future enhancements and additional features. The Retrosharp ETL process is governed by a service bus using the NServiceBus library backed by RabbitMQ, so that processing
Retrosheet datafiles is asynchronous, scalable, and provides detailed logging of the ETL process. The Retrosharp front end is an Angular 17 application that communicates with the backend via RESTful APIs.
The application is designed to be responsive and accessible, ensuring a seamless user experience across different devices and screen sizes.

## Acceptance Criteria

### Initial

These criteria are to get the application to a minimum viable product (MVP) state, where it can be used for basic data exploration and analysis.

1. Database setup: The application should have a SQL Server database set up with the necessary tables and relationships to store Retrosheet data.
	1. Use a code-first approach with Entity Framework Core to define the database schema and relationships.
	1. Use third normal form (3NF) to ensure data integrity and minimize redundancy wherever possible.
1. Retrosheet data import: The application should be able to import Retrosheet data files, including game logs, player statistics, and team information.
1. Statistical calculations based on raw data: The application should be able to calculate basic statistics such as batting average, on-base percentage, slugging percentage, and earned run average based on the imported data.
	1. Include hitting, pitching, and fielding
1. Player search functionality: Users should be able to search for players by name, team, or position and view their career statistics and game logs.
1. Retrosharp API and Retrosharp UI/UX are each independently deployable and can be deployed to a cloud provider of choice using containers.
1. ETL processes are initiated by receving messages on a service bus with the following information:
	1. The Retrosheet datafile to be processed
	1. The type of datafile, and the location of the datafile.
	1. The ETL process should be asynchronous and scalable, allowing for multiple datafiles to be processed simultaneously.
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

## Relavent Information

1. [Retrosheet](https://www.retrosheet.org) - The source of the baseball data used in Retrosharp.
1. [Retrosheet Event Format](https://www.retrosheet.org/eventfile.htm) - Documentation on the format of Retrosheet game events.
1. [Retrosheet Biographical Data](https://www.retrosheet.org/biofile.zip) - A zip file containing biographical data for players, coaches, umpires, and managers
1. [Retrosheet Negro League Data](https://www.retrosheet.org/NegroLeagues/NegroLeagues.html) - Retrosheet's page dedicating baseball data of Negro Leagues.
