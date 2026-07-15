# Game Log Parser

## Overview

Game logs from Retrosheet contain comprehensive information about each game played in a season, including the teams, players, scores, and other relevant statistics. The Game Log Parser is responsible for parsing these game logs and populating the Retrosharp database with the extracted information. The Game Log Parser is only for the game itself. It does not contain individual player statistics or play-by-play information. The Game Event Parser is responsible for parsing the play-by-play event files and populating the database with individual player statistics.

## Format

This is how the datafile is formatted:

```text
Field(s)  Meaning
    1     Date in the form "yyyymmdd"
    2     Number of game:
             "0" -- a single game
             "1" -- the first game of a double (or triple) header
                    including separate admission doubleheaders
             "2" -- the second game of a double (or triple) header
                    including separate admission doubleheaders
             "3" -- the third game of a triple-header
             "A" -- the first game of a double-header involving 3 teams
             "B" -- the second game of a double-header involving 3 teams
    3     Day of week  ("Sun","Mon","Tue","Wed","Thu","Fri","Sat")
  4-5     Visiting team and league
    6     Visiting team game number
          For this and the home team game number, ties are counted as
          games and suspended games are counted from the starting
          rather than the ending date.
  7-8     Home team and league
    9     Home team game number
10-11     Visiting and home team score (unquoted)
   12     Length of game in outs (unquoted).  A full 9-inning game would
          have a 54 in this field.  If the home team won without batting
          in the bottom of the ninth, this field would contain a 51.
   13     Day/night indicator ("D" or "N")
   14     Completion information.  If the game was completed at a
          later date (either due to a suspension or an upheld protest)
          this field will include:
             "yyyymmdd,park,vs,hs,len" Where
          yyyymmdd -- the date the game was completed
          park -- the park ID where the game was completed
          vs -- the visitor score at the time of interruption
          hs -- the home score at the time of interruption
          len -- the length of the game in outs at time of interruption
          All the rest of the information in the record refers to the
          entire game.
   15     Forfeit information:
             "V" -- the game was forfeited to the visiting team
             "H" -- the game was forfeited to the home team
             "T" -- the game was ruled a no-decision
   16     Protest information:
             "P" -- the game was protested by an unidentified team
             "V" -- a disallowed protest was made by the visiting team
             "H" -- a disallowed protest was made by the home team
             "X" -- an upheld protest was made by the visiting team
             "Y" -- an upheld protest was made by the home team
          Note: two of these last four codes can appear in the field
          (if both teams protested the game).
   17     Park ID
   18     Attendance (unquoted)
   19     Time of game in minutes (unquoted)
20-21     Visiting and home line scores.  For example:
             "010000(10)0x"
          Would indicate a game where the home team scored a run in
          the second inning, ten in the seventh and didn't bat in the
          bottom of the ninth.
22-38     Visiting team offensive statistics (unquoted) (in order):
             at-bats
             hits
             doubles
             triples
             homeruns
             RBI
             sacrifice hits.  This may include sacrifice flies for years
                prior to 1954 when sacrifice flies were allowed.
             sacrifice flies (since 1954)
             hit-by-pitch
             walks
             intentional walks
             strikeouts
             stolen bases
             caught stealing
             grounded into double plays
             awarded first on catcher's interference
             left on base
39-43     Visiting team pitching statistics (unquoted)(in order):
             pitchers used ( 1 means it was a complete game )
             individual earned runs
             team earned runs
             wild pitches
             balks
44-49     Visiting team defensive statistics (unquoted) (in order):
             putouts.  Note: prior to 1931, this may not equal 3 times
                the number of innings pitched.  Prior to that, no
                putout was awarded when a runner was declared out for
                being hit by a batted ball.
             assists
             errors
             passed balls
             double plays
             triple plays
50-66     Home team offensive statistics
67-71     Home team pitching statistics
72-77     Home team defensive statistics
78-79     Home plate umpire ID and name
80-81     1B umpire ID and name
82-83     2B umpire ID and name
84-85     3B umpire ID and name
86-87     LF umpire ID and name
88-89     RF umpire ID and name
          If any umpire positions were not filled for a particular game
          the fields will be "","(none)".
90-91     Visiting team manager ID and name
92-93     Home team manager ID and name
94-95     Winning pitcher ID and name
96-97     Losing pitcher ID and name
98-99     Saving pitcher ID and name--"","(none)" if none awarded
100-101   Game Winning RBI batter ID and name--"","(none)" if none
          awarded
102-103   Visiting starting pitcher ID and name
104-105   Home starting pitcher ID and name
106-132   Visiting starting players ID, name and defensive position,
          listed in the order (1-9) they appeared in the batting order.
133-159   Home starting players ID, name and defensive position
          listed in the order (1-9) they appeared in the batting order.
  160     Additional information.  This is a grab-bag of informational
          items that might not warrant a field on their own.  The field 
          is alpha-numeric. Some items are represented by tokens such as:
             "HTBF" -- home team batted first.
             Note: if "HTBF" is specified it would be possible to see
             something like "01002000x" in the visitor's line score.
          Changes in umpire positions during a game will also appear in 
          this field.  These will be in the form:
             umpchange,inning,umpPosition,umpid with the latter three
             repeated for each umpire.
          These changes occur with umpire injuries, late arrival of 
          umpires or changes from completion of suspended games. Details
          of suspended games are in field 14.
  161     Acquisition information:
             "Y" -- we have the complete game
             "N" -- we don't have any portion of the game
             "D" -- the game was derived from box score and game story
             "P" -- we have some portion of the game.  We may be missing
                    innings at the beginning, middle and end of the game.
 
Missing fields will be NULL.
```

## Prerequisites

1. The `Person` table needs to be populated with player, umpire, manager, and coach information before the Game Log Parser can be run. This is because the Game Log Parser will reference the `Person` table to associate players with their respective statistics and game events.
1. The `League` table needs to be populated with league information before the Game Log Parser can be run. This is because the Game Log Parser will reference the `League` table to associate games with their respective leagues and seasons.
1. The `Franchise` table needs to be populated with franchise information before the Game Log Parser can be run. This is because the Game Log Parser will reference the `Franchise` table to associate games with their respective franchises and teams.

## Requirements

1. The Game Log Parser should be able to read and process Retrosheet's game log data, extracting relevant information such as game outcomes, and team performance.
1. Each game shall be placed into their respective tables in the database
	1. `Game` - this table contains game information such as the teams played, stadium, weather conditions, umpires, and other relevant metadata.
	1. `GameLineup` - this table contains the starting lineup for each team, including player positions and batting order.
	1. `GameBattingStatistics` - this table contains the total statistics for each game, and each team that played in the game, including runs scored, hits, errors, and other relevant statistics.
	1. `GamePitchingStatistics` - this table contains the total pitching statistics for each game, and each team that played in the game, including innings pitched, strikeouts, walks, and other relevant statistics.
	1. `GameFieldingStatistics` - this table contains the total fielding statistics for each game, and each team that played in the game, including putouts, assists, errors, and other relevant statistics.
1. The Game Log Parser should be able to handle large volumes of data and process multiple game logs in a batch mode.
1. The file format is `glYYYY.TXT` where `YYYY` is the year of the game logs. The Game Log Parser should be able to process multiple files in a batch mode, allowing for efficient processing of large volumes of data. Despite having a `.txt` extension, the file is actually CSV formatted.
1. An API endpoint exists to place a message on the service bus to initiate the processing of a game log file. This allows for external systems or users to trigger the processing of game log files as needed.

## Acceptance Criteria

1. The Game Log Parser should be able to successfully read and process Retrosheet's game log, extracting relevant information and populating the Retrosharp database.
1. Error handling in the Game Log Parser should be robust, with clear error messages and logging to facilitate debugging and troubleshooting.
1. Retryable errors should be retried a configurable amount of time and initial retry wait period. There should be an exponential backoff with jitter for retries.
	1. The Polly library might be of assistance
1. Each datafile parse should be idempotent, meaning that if the same datafile is processed multiple times, it should not result in duplicate entries in the database.
1. Each datafile parse should be atomic, meaning that if unrecoverable error(s) occurs during processing, the database should not be left in an inconsistent state. No partial parses!
1. Follow the [Format](#format) section of this document to ensure that the data is extracted and stored in the correct format in the database.
