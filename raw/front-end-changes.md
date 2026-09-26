# Retrosharp Front End Changes

## Overview

After implementing a prototype of the front end for Retrosharp (see `spec/frontend-prototype.md`). There are some things I have noticed that need to be incorporated into the design of the frontend. The importing of data from Retrosheet isn't complete, yet, however, there are six seasons now and that should begin to have enough data from which to start evaluating how it is displayed.

> **NOTE**: This document will be evolving as changes are implemented.

## Inspiration

The look and feel I was after was inspired by the designs around [Baseball Reference](https://www.baseball-reference.com/players/m/machama01.shtml), [Fangraphs](https://www.fangraphs.com/players/manny-machado/11493/stats/batting), and [Baseball Prospectus](https://www.baseballprospectus.com/player/67049) (BP might be behind a paywall). Concise, compact designs which provide the relevant information to the user. Compact tables which don't feel awfully cluttered and overwhelming. Clean designs and colors which highlight important info such as career highs.

## Improvements

There are some areas I've noticed that need to be improved

### Home Page (/)

There is still an ongoing discussion about what to place in the landing page. There is currently some prototyping being done on the homepage and likely will be a separate spec

### Players (/players)

~~I corrected the overly excessive row height on the Angular Material table. However, there are some additional items. Originally there was a requirement to bold the text of a player if they were still active. However, that's not possible to determine given the current data from Retrosheet. Retrosheet is always a season behind and wisely marks the last day they played be it the last game of the regular season or the last time they played in the postseason. Players can and have unexpectantly retire during the offseason. Therefore this bold player text needs to be removed.~~

~~There needs to be a paginator with numbers instead of sequentially hitting the next/previous buttons. The "M" players alone are 2,751. This allows a user to better jump to a player they'd like to see. Also, clicking the next/previous button scrolls the screen up. This makes for a disorienting and annoying user experience.~~

The players page now has improved changes that is bringing a better UX. However more can be done. The pagination should be on the both top and bottom of the table so to reduce the amount of scrolling the user needs to do ([Fangraphs](https://www.fangraphs.com/leaders/major-league?pos=all&stats=bat&lg=all&qual=y&type=8&season=2026&month=0&season1=2026&ind=0&pagenum=4&pageitems=30) does this). Also, the "Per page" drop down displays as "Per pa". This makes for a low quality UX.

### Player detail (/players/[id])

~~Currently it only displays their first name. It should be "[use name] [last name]" If the player's lastname/surname is missing from the data, that is an issue that needs to be addressed immediately. The "use name" should be what the player is best known like "Manny Machado" instead of their legal name like "Manuel Machado". That said, I would like to see their full legal name in the player detail page as well, however it is displayed with smaller more muted text underneath the "use name" and last name.~~

The batting statistics table currently has side scrolling in it to view the statistics. This is a cumbersome user experience and it is more than likely due to how much whitespace there is in between columns. 

### Teams (/teams)

Not yet built

### Theme and Colors

I previously intended to use a seven-color swatch however since learning of Angular's M3 theming, I have chosen to use that around a base color. Also, a dark theme will be used and be system aware. However, this is still WIP.

#### Retrosharp Light Mode

- #196DE6 - Primary color

#### Retrosharp Dark Mode

Color WIP

## Goals

1. Improve the user experience of the front end by implementing the above changes.