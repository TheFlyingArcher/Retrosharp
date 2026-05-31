USE [Retrosheet]
GO

/****** Object:  Table [dbo].[GameStatistics]    Script Date: 5/28/2026 3:55:32 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GameStatistics]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[GameStatistics](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[GameID] [int] NOT NULL,
	[FranchiseID] [int] NOT NULL,
	[HomeVisitor] [varchar](1) NOT NULL,
	[PlateAppearances] [smallint] NOT NULL,
	[AtBats] [smallint] NOT NULL,
	[Hit] [smallint] NOT NULL,
	[Doubles] [smallint] NOT NULL,
	[Triples] [smallint] NOT NULL,
	[Homeruns] [smallint] NOT NULL,
	[RunsBattedIn] [smallint] NOT NULL,
	[BaseOnBalls] [smallint] NOT NULL,
	[Strikeouts] [smallint] NOT NULL,
	[SacrificeFlies] [smallint] NOT NULL,
	[SacrificeBunts] [smallint] NOT NULL,
	[IntentionalBB] [smallint] NOT NULL,
	[HitByPitches] [smallint] NOT NULL,
	[StolenBases] [smallint] NOT NULL,
	[TimesCaughtStealing] [smallint] NOT NULL,
	[Runs] [smallint] NOT NULL,
	[GroundedIntoDoublePlay] [smallint] NOT NULL,
 CONSTRAINT [PK_GameStatistics] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO

/****** Object:  Index [IX_GameStatistics_FranchiseID]    Script Date: 5/28/2026 3:55:32 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[GameStatistics]') AND name = N'IX_GameStatistics_FranchiseID')
CREATE NONCLUSTERED INDEX [IX_GameStatistics_FranchiseID] ON [dbo].[GameStatistics]
(
	[FranchiseID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

/****** Object:  Index [IX_GameStatistics_GameID]    Script Date: 5/28/2026 3:55:32 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[GameStatistics]') AND name = N'IX_GameStatistics_GameID')
CREATE NONCLUSTERED INDEX [IX_GameStatistics_GameID] ON [dbo].[GameStatistics]
(
	[GameID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_GameStatistics_Franchise]') AND parent_object_id = OBJECT_ID(N'[dbo].[GameStatistics]'))
ALTER TABLE [dbo].[GameStatistics]  WITH CHECK ADD  CONSTRAINT [FK_GameStatistics_Franchise] FOREIGN KEY([FranchiseID])
REFERENCES [dbo].[Franchise] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_GameStatistics_Franchise]') AND parent_object_id = OBJECT_ID(N'[dbo].[GameStatistics]'))
ALTER TABLE [dbo].[GameStatistics] CHECK CONSTRAINT [FK_GameStatistics_Franchise]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_GameStatistics_Game]') AND parent_object_id = OBJECT_ID(N'[dbo].[GameStatistics]'))
ALTER TABLE [dbo].[GameStatistics]  WITH CHECK ADD  CONSTRAINT [FK_GameStatistics_Game] FOREIGN KEY([GameID])
REFERENCES [dbo].[Game] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_GameStatistics_Game]') AND parent_object_id = OBJECT_ID(N'[dbo].[GameStatistics]'))
ALTER TABLE [dbo].[GameStatistics] CHECK CONSTRAINT [FK_GameStatistics_Game]
GO

