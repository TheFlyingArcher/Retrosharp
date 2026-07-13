# Person Parser

## Overview

The Person parser is resposible for parsing Retrosheet's biographical information datafile, known as the "biofile" on Retrosheet.
The biofile contains information about players, umpires, managers, and coaches, including their names, birthdates, birthplaces, and other relevant biographical details.
The biofile also contains just about every player to have played professional baseball in the United States dating back to the 1800s.

## Format and Data Source

The format of the biofile can be [viewed](https://www.retrosheet.org/biofile.htm) on Retrosheet. The newer file format is to be used instead of the legacy format.
The datafile is of CSV format.

## Data Considerations

Because the biographical information file (biofile) contains historical information, some of it may be incomplete or in incompatible formats. For example, birth or death
dates may not be in standard YYYYMMDD format.

## Acceptance Criteria

1. The Person parser should be able to successfully read and process Retrosheet's biographical information datafile, extracting relevant information and populating the `Person` table in the Retrosharp database.
1. The parser should maintain idempotent data. The same record should not be duplicated or inserted multiple times.
1. The parser should be able to handle incomplete or incompatible date formats using the following methods:
	1. Check every date field for standard YYYYMMDD format.
	1. If the day is 00, then the day should be set to 01 (first of the month).
	1. If the month is 00, then the month should be set to 01 (January)
	1. If year is missing, set the date field to NULL for unknown date
1. The HOF (Hall-of-famer) field should be set to true if field says "HOF" which indicates the player is in the Hall of Fame, otherwise false.
1. Because this file contains every player to have played professional baseball in the United States, there will have to be updates to the `Person` table as new players are added to the database. The parser should be able to handle updates to existing records in the `Person` table, ensuring that the most up-to-date information is stored in the database.
1. The parser should be atomic in nature. If an unrecoverable error occurs during processing, the database should not be left in an inconsistent state. No partial parses!
1. At the end of the parse, it should be logged and reported how many records were added and updated in the database
