# Retrosharp Seed Data

## Overview

Retrosharp requires certain static data to be present. This data is virtually unchanging thus constructing a parser is unnecessary. 

## Leagues

This data represent professional baseball leagues that have existed in the United States. The data is sourced from Retrosheet's [team IDs](https://https://www.retrosheet.org/TeamIDs.htm) file.

NOTE: This does not include amateur leagues, semi-professional leagues, or leagues that were not considered professional baseball leagues. This also does not include leagues for minor leagues such as Pacific Coast League.

```text
LeagueCode,LeagueName
AA,American Association
AL,American League
FL,Federal League
NA,National Association
NL,National League
PL,Players League
UA,Union Association
```

## Ballparks

This data represents ballparks used to play professional baseball within the United States. The data is sourced from Retrosheet's [ballpark IDs](https://www.retrosheet.org/ballparks.zip) file. If the end date is empty, that means the ballpark is still in use by a franchise. If the end date is populated, that means the ballpark is no longer in use by a franchise.

NOTE: It comes in a ZIP file and contains a CSV file with the ballpark data.

Please reference the [ballparks.csv](../docs/csv/ballparks.csv) present within this repository.

## Franchises

This data represents current and historical franchises that have existed within the United States. The data is sourced from Retrosheet's [Team Nicknames History](https://www.retrosheet.org/Nickname.htm) page containing the format of the text file. The actual file is called [CurrentNames.csv](https://www.retrosheet.org/CurrentNames.csv) and is a CSV file.

A copy of the franchise data can be found in the [franchises.csv](../docs/csv/franchises.csv) file present within this repository. It is the same data as the Retrosheet's CurrentNames.csv file but with a more representative filename.
