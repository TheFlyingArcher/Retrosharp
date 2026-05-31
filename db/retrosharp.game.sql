USE [Retrosheet]
GO

/****** Object:  Table [dbo].[Game]    Script Date: 5/28/2026 3:54:26 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Game]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Game](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[GameDate] [date] NOT NULL,
	[GameNumber] [tinyint] NOT NULL,
	[GameWeekDay] [varchar](3) NULL,
	[GameDayNight] [varchar](1) NULL,
	[VisitorFranchiseID] [int] NOT NULL,
	[VisitorGameNumber] [int] NOT NULL,
	[VisitorRuns] [tinyint] NOT NULL,
	[VisitorHits] [tinyint] NULL,
	[VisitorErrors] [tinyint] NULL,
	[VisitorLineScore] [varchar](64) NULL,
	[VisitorManagerID] [int] NOT NULL,
	[HomeFranchiseID] [int] NOT NULL,
	[HomeGameNumber] [int] NOT NULL,
	[HomeTeamRuns] [tinyint] NOT NULL,
	[HomeHits] [tinyint] NULL,
	[HomeErrors] [tinyint] NULL,
	[HomeLineScore] [varchar](64) NULL,
	[HomeManagerID] [int] NOT NULL,
	[BallparkID] [int] NOT NULL,
	[GameLengthMinutes] [smallint] NULL,
	[ParkAttendance] [int] NULL,
	[UmpireHomeID] [int] NULL,
	[UmpireFirstID] [int] NULL,
	[UmpireSecondID] [int] NULL,
	[UmpireThirdID] [int] NULL,
	[UmpireLeftID] [int] NULL,
	[UmpireRightID] [int] NULL,
	[WinningPitcherID] [int] NULL,
	[LosingPitcherID] [int] NULL,
	[SavingPitcherID] [int] NULL,
	[GameWinningBatterID] [int] NULL,
	[GameNotes] [varchar](2048) NULL,
 CONSTRAINT [PK_Game] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO

/****** Object:  Index [IX_Game_Ballpark]    Script Date: 5/28/2026 3:54:26 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Game]') AND name = N'IX_Game_Ballpark')
CREATE NONCLUSTERED INDEX [IX_Game_Ballpark] ON [dbo].[Game]
(
	[BallparkID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

/****** Object:  Index [IX_Game_HomeFranchiseID]    Script Date: 5/28/2026 3:54:26 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Game]') AND name = N'IX_Game_HomeFranchiseID')
CREATE NONCLUSTERED INDEX [IX_Game_HomeFranchiseID] ON [dbo].[Game]
(
	[HomeFranchiseID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

/****** Object:  Index [IX_Game_Umpires]    Script Date: 5/28/2026 3:54:26 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Game]') AND name = N'IX_Game_Umpires')
CREATE NONCLUSTERED INDEX [IX_Game_Umpires] ON [dbo].[Game]
(
	[UmpireHomeID] ASC,
	[UmpireFirstID] ASC,
	[UmpireSecondID] ASC,
	[UmpireThirdID] ASC,
	[UmpireLeftID] ASC,
	[UmpireRightID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

/****** Object:  Index [IX_Game_VisitorFranchiseID]    Script Date: 5/28/2026 3:54:26 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Game]') AND name = N'IX_Game_VisitorFranchiseID')
CREATE NONCLUSTERED INDEX [IX_Game_VisitorFranchiseID] ON [dbo].[Game]
(
	[VisitorFranchiseID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_Ballpark]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game]  WITH CHECK ADD  CONSTRAINT [FK_Game_Ballpark] FOREIGN KEY([BallparkID])
REFERENCES [dbo].[Ballpark] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_Ballpark]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game] CHECK CONSTRAINT [FK_Game_Ballpark]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_GameWinningBatter]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game]  WITH CHECK ADD  CONSTRAINT [FK_Game_GameWinningBatter] FOREIGN KEY([GameWinningBatterID])
REFERENCES [dbo].[Person] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_GameWinningBatter]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game] CHECK CONSTRAINT [FK_Game_GameWinningBatter]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_HomeFranchise]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game]  WITH CHECK ADD  CONSTRAINT [FK_Game_HomeFranchise] FOREIGN KEY([HomeFranchiseID])
REFERENCES [dbo].[Franchise] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_HomeFranchise]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game] CHECK CONSTRAINT [FK_Game_HomeFranchise]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_HomeManager]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game]  WITH CHECK ADD  CONSTRAINT [FK_Game_HomeManager] FOREIGN KEY([HomeManagerID])
REFERENCES [dbo].[Person] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_HomeManager]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game] CHECK CONSTRAINT [FK_Game_HomeManager]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_LosingPitcher]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game]  WITH CHECK ADD  CONSTRAINT [FK_Game_LosingPitcher] FOREIGN KEY([LosingPitcherID])
REFERENCES [dbo].[Person] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_LosingPitcher]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game] CHECK CONSTRAINT [FK_Game_LosingPitcher]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_SavingPitcher]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game]  WITH CHECK ADD  CONSTRAINT [FK_Game_SavingPitcher] FOREIGN KEY([SavingPitcherID])
REFERENCES [dbo].[Person] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_SavingPitcher]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game] CHECK CONSTRAINT [FK_Game_SavingPitcher]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_UmpireFirst]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game]  WITH CHECK ADD  CONSTRAINT [FK_Game_UmpireFirst] FOREIGN KEY([UmpireFirstID])
REFERENCES [dbo].[Person] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_UmpireFirst]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game] CHECK CONSTRAINT [FK_Game_UmpireFirst]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_UmpireHome]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game]  WITH CHECK ADD  CONSTRAINT [FK_Game_UmpireHome] FOREIGN KEY([UmpireHomeID])
REFERENCES [dbo].[Person] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_UmpireHome]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game] CHECK CONSTRAINT [FK_Game_UmpireHome]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_UmpireLeft]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game]  WITH CHECK ADD  CONSTRAINT [FK_Game_UmpireLeft] FOREIGN KEY([UmpireLeftID])
REFERENCES [dbo].[Person] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_UmpireLeft]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game] CHECK CONSTRAINT [FK_Game_UmpireLeft]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_UmpireRight]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game]  WITH CHECK ADD  CONSTRAINT [FK_Game_UmpireRight] FOREIGN KEY([UmpireRightID])
REFERENCES [dbo].[Person] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_UmpireRight]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game] CHECK CONSTRAINT [FK_Game_UmpireRight]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_UmpireSecond]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game]  WITH CHECK ADD  CONSTRAINT [FK_Game_UmpireSecond] FOREIGN KEY([UmpireSecondID])
REFERENCES [dbo].[Person] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_UmpireSecond]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game] CHECK CONSTRAINT [FK_Game_UmpireSecond]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_UmpireThird]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game]  WITH CHECK ADD  CONSTRAINT [FK_Game_UmpireThird] FOREIGN KEY([UmpireThirdID])
REFERENCES [dbo].[Person] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_UmpireThird]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game] CHECK CONSTRAINT [FK_Game_UmpireThird]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_VisitorFranchise]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game]  WITH CHECK ADD  CONSTRAINT [FK_Game_VisitorFranchise] FOREIGN KEY([VisitorFranchiseID])
REFERENCES [dbo].[Franchise] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_VisitorFranchise]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game] CHECK CONSTRAINT [FK_Game_VisitorFranchise]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_VisitorManager]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game]  WITH CHECK ADD  CONSTRAINT [FK_Game_VisitorManager] FOREIGN KEY([VisitorManagerID])
REFERENCES [dbo].[Person] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_VisitorManager]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game] CHECK CONSTRAINT [FK_Game_VisitorManager]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_WinningPitcher]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game]  WITH CHECK ADD  CONSTRAINT [FK_Game_WinningPitcher] FOREIGN KEY([WinningPitcherID])
REFERENCES [dbo].[Person] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Game_WinningPitcher]') AND parent_object_id = OBJECT_ID(N'[dbo].[Game]'))
ALTER TABLE [dbo].[Game] CHECK CONSTRAINT [FK_Game_WinningPitcher]
GO

