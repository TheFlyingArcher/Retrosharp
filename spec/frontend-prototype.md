# Retrosharp Frontend Prototype

## Overview

This is to prototype out designs and ideas for the frontend of Retrosharp. The goal is to create a user-friendly interface that allows users to easily navigate and interact with the application. This is to satisfy Feature 3 found in [project.md](project.md) and it is a part of Phase One of the project. The frontend will be built using Angular and will communicate with the backend via RESTful APIs.

The goal of the front end is to allow an end user to search baseball players, franchises, seasons, and games. The user should be able to view detailed information about each entity, including statistics, historical data, and related media. The frontend should also provide a way for users to filter and sort the data based on various criteria.

## Pages

Below these are the pages that will be in the prototype. Each page will have requirements and features that will be implemented in the frontend. The pages will be designed to be responsive and accessible, ensuring that they can be used on a variety of devices and by users with different abilities.

### Retrosharp

This refers to the application as a whole. Here, common components such as headers and footers will be implemented. The header will contain the application name, navigation links, and a search bar. The footer will contain copyright information and links to social media. These components will be consistent across all pages, providing a cohesive user experience.

#### Header

The header will contain the following items:
- Application name: Retrosharp
- Navigation links: Home, Players, Franchises, Seasons, Games
- PHASE TWO: Sign on and sign out buttons as well as a user profile link.

#### Footer

The footer will contain the following items:
- Copyright information: © 2026 Retrosharp. All rights reserved.
- My GitHub link: A link to the Retrosharp GitHub repository.
- Report issue link: A link to report issues or bugs with the application, points to GitHub issues section of repository.

### Home Page

Path: / or /home
This is the landing page for the application. It will provide an overview of the application and its features, as well as links to the other pages. The home page will also include a search bar that allows users to quickly find players, franchises, seasons, and games. The search bar will feature type ahead suggestions to help users find what they are looking for more efficiently. Upon selecting either a player, franchise, season, or game from the search results, the user will be redirected to the corresponding detail page.

#### Open Design Question

A single text search bar on the page will leave a lot of space on the page. Can other baseball content be filled on the page such as latest news items from MLB if a feed exists? Or perhaps a list of the most popular players, franchises, seasons, and games? This is an open design question that will be explored during the prototyping phase.

### Players Page

Path: /players

On this page, users will be able to view a list of baseball players grouped by the player's last name. To reduce the amount of data coming through only the player's last name and first name will be displayed in the list. Clicking on a player's name will take the user to the player detail page. There is also on top of the page the alphabet A-Z in which a user can click a letter and it drops them down to the players whose last name starts with that letter. The list of players will be paginated to improve performance and usability. Bold text will indicate the player is currently active in MLB. A cross next to their name will indicate the player is deceased.

#### Open Design Question

There is no explicit `IsActive` field within the underlying the data store. Instead, the "is active" is compute by comparing the player's last game date to the last game played the season year. If the player's last game date is within the last season year, then the player is considered active. Open question: is `PlayerLastDate` return as null? If so, then the player is considered active.

### Player Detail Page

Path: /players/[id]

The detail page will display the details of the player with that ID. The player detail page will include information such as the player's name, position, height, weight, birthdate, birth city, death date, death city, burial location, team, statistics, and related media.

There will be a table containing the following columns based on position:
- All positions: Year, Team(s), Games Played
- If the player is a pitcher: Games Started, Wins, Losses, Saves, Innings Pitched, Strikeouts, Walks, ERA
- If the player is a batter: At Bats, Hits, Home Runs, RBIs, Batting Average, On Base, Slugging, OPS, Strikeouts, Walks, Stolen Bases, and Caught Stealing

Table will be sortable on all columns. MLB careers rarely span more than 20 years, so pagination shouldn't be a necessity. If a player has played for multiple teams in a single year, the table will display each team on a separate row. Despite mid-season trades, rows shouldn't exceed 30 rows.

**A Note on Franchises:** It is exceedingly rare, however franchises have changed name and/or location during a player's playing career on that franchise. Case in point is in 2002 when the California Angels were renamed to the Anaheim Angels. In this case, the franchise is the same, but the name has changed. The player detail page will display the franchise name as it was during the player's tenure with that franchise. The franchise detail page will display all names and locations of the franchise throughout its history.

### Player Season Detail Page

Path: /players/[id]/seasons/[year]

This page will display all the games the player played in that season. The page will also display the player's statistics for that season, as well as any awards or honors the player received during that season. Clicking on a game will take the user to the game detail page for that game. This will also include the same biographical information as the player detail page.

Table includes the following columns:
- Date: The date of the game.
- Opponent: The opponent team for that game.
- Score: The score of the game.
- For position player: AB, H, 2B, 3B, HR, RBI, SO, BB, SB, CS, AVG, OBP, SLG, OPS, TB, GIDP, HBP, SH, SF, IBB
- For pitchers: GS, W, L, SV, IP, H, R, ER, HR, BB, SO, ERA, WHIP

### Franchises Page

Path: /franchises

The Franchises page will display a table of all franchises that ever have existed within Major League Baseball. Each row is the current extant franchise so Washington Nationals is featured not Montreal Expos. There will be a footnote denoting each the former names of the franchise in a comma separated list. Clicking on a franchise name will take the user to the franchise detail page. The table will be sortable on all columns. Table columns:
- Name: Name of the franchise as it is currently known. This will also contain a link to the franchise detail page.
- From: The year the franchise was established from the beginning of the franchise's history.
- Games (Played): The total number of games played by the franchise from its inception to the present.
- Wins: The total number of wins by the franchise from its inception to the present.
- Losses: The total number of losses by the franchise from its inception to the present.
- Win %: The total win percentage of the franchise from its inception to the present.
- Above .500: The total number of seasons the franchise has finished above a .500 win percentage.
- Below .500: The total number of seasons the franchise has finished below a .500 win percentage.

### Franchise Detail Page

Path: /franchises/[id]

The franchise detail page will display a table with the following columns:
- Year: The year of the season.
- Complete Franchise Name: The name of the franchise during that season which includes the city and nickname (e.g. San Diego Padres).
- League: The league the franchise played in during that season (e.g. American League or National League).
- Division: The division the franchise played in during that season (e.g. AL West, NL East). **NOTE**: Divisions were not created until 1969, so this column will be blank for seasons prior to 1969.
- Wins: The total number of wins by the franchise during that season.
- Losses: The total number of losses by the franchise during that season.
- Win %: The win percentage of the franchise during that season.
- Finish: The final standing of the franchise during that season (e.g. 1st, 2nd, 3rd, etc.). 
	- **NOTE**: For seasons prior to 1969, this column will display as "[position] in [league]" (e.g. 1st in American League, 2nd in National League, etc.) since there were no divisions prior to 1969.
	- **NOTE**: The American League did not exist until 1901. So this will simply display the ordinal position in the league (e.g. 1st, 2nd, 3rd, etc.) for seasons prior to 1901.
	- **NOTE**: The National League did not exist until 1876. So this will simply display the ordinal position in the league (e.g. 1st, 2nd, 3rd, etc.) for seasons prior to 1876.
	- **NOTE**: After 1969, the finish column will display the ordinal position in the division (e.g. "1st in AL West", "2nd in NL East", etc.)
- Divisions: The number of times the franchise won the division during that season. This will be blank for seasons prior to 1969.
- Pennants: The number of times the franchise won the league pennant during that season. Prior to 1969, this will be the number of times the franchise was the best record of the league. After 1969, this will be the number of times the franchise won the league championship series.
- GB (Games Behind): The number of games the franchise was behind the first place team during that season. Before 1960, this will be the number of games behind the best record in the league. After 1960, this will be the number of games behind the first place team in the division.
- Manager: The name of the manager of the franchise during that season.

#### Open Design Question

Investigate the Retrosheet data to see if it supports multiple managers in a season. It is quite common that a manager is fired mid-season and replaced by another manager. If the data supports multiple managers in a season, then the franchise detail page will need to display all managers for that season. This is an open design question that will be explored during the prototyping phase.

### Franchise Season Detail Page

Path: /franchises/[id]/seasons/[year]

The franchise season detail page will display all the pertient information about the franchise for that season. This includes the franchise's record, standings, and statistics for that season. The page will also display a list of all players who played for the franchise during that season, along with their individual statistics. Clicking on a player's name will take the user to the player detail page for that season. The page will also display a list of all games played by the franchise during that season, along with the scores and outcomes of each game. Clicking on a game will take the user to the game detail page for that game.

There will be two sections on the page. First section is for the position players and the second section is for the pitchers. Each section will have a table with the following columns:
- Player Name: The name of the player. This will also contain a link to the player detail page for that season.
- Games Played: The total number of games played by the player during that season.
- For position players: AB, H, 2B, 3B, HR, RBI, SO, BB, SB, CS, AVG, OBP, SLG, OPS, TB, GIDP, HBP, SH, SF, IBB
- For pitchers: GS, W, L, SV, IP, H, R, ER, HR, BB, SO, ERA, WHIP
- For position players: group the top nine players that started the most games at each position. The remaining players will be grouped together in a separate section of the table.
- For pitchers: group the top five pitchers that started the most games (the starters). The remaining pitchers will be grouped together in a separate section of the table (the relievers). The relievers will be sorted by the number of games they appeared in during the season. Include a third section highlighting the top three closers for the franchise during that season. The closers will be sorted by the number of saves they recorded during the season.

## Reference Designs

Examine the designs of these pages to get a better understanding of how to design the frontend of Retrosharp. These designs are not perfect, but they provide a good starting point for the prototyping phase.

- [Baseball Reference](https://www.baseball-reference.com/)
- [Retrosheet](https://www.retrosheet.org/)
- [MLB](https://www.mlb.com/)
- [FanGraphs](https://www.fangraphs.com/)