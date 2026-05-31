USE [Retrosheet]
GO

/****** Object:  Table [dbo].[Batting]    Script Date: 5/28/2026 3:53:59 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Batting]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Batting](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PersonID] [int] NOT NULL,
	[FranchiseID] [int] NOT NULL,
	[SeasonYear] [smallint] NULL,
	[PlateAppearances] [smallint] NOT NULL,
	[AtBats] [smallint] NOT NULL,
	[Hit] [smallint] NOT NULL,
	[Doubles] [smallint] NOT NULL,
	[Triples] [smallint] NOT NULL,
	[Homeruns] [smallint] NOT NULL,
	[BaseOnBalls] [smallint] NOT NULL,
	[Strikeouts] [smallint] NOT NULL,
	[SacrificeFlies] [smallint] NOT NULL,
	[SacrificeBunts] [smallint] NOT NULL,
	[IntentionalBB] [smallint] NOT NULL,
	[HitByPitches] [smallint] NOT NULL,
	[StolenBases] [smallint] NOT NULL,
	[TimesCaughtStealing] [smallint] NOT NULL,
	[Runs] [smallint] NOT NULL,
	[Positions] [smallint] NOT NULL,
	[GroundedIntoDoublePlay] [smallint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO

/****** Object:  Index [IX_Batting_Franchise]    Script Date: 5/28/2026 3:54:00 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Batting]') AND name = N'IX_Batting_Franchise')
CREATE NONCLUSTERED INDEX [IX_Batting_Franchise] ON [dbo].[Batting]
(
	[FranchiseID] ASC
)
INCLUDE([SeasonYear]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

/****** Object:  Index [IX_Batting_Person]    Script Date: 5/28/2026 3:54:00 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Batting]') AND name = N'IX_Batting_Person')
CREATE NONCLUSTERED INDEX [IX_Batting_Person] ON [dbo].[Batting]
(
	[PersonID] ASC
)
INCLUDE([SeasonYear]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

/****** Object:  Index [IX_Batting_Year]    Script Date: 5/28/2026 3:54:00 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Batting]') AND name = N'IX_Batting_Year')
CREATE NONCLUSTERED INDEX [IX_Batting_Year] ON [dbo].[Batting]
(
	[SeasonYear] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Batting__PlateAp__693CA210]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Batting] ADD  DEFAULT ((0)) FOR [PlateAppearances]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Batting__AtBats__6A30C649]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Batting] ADD  DEFAULT ((0)) FOR [AtBats]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Batting__Hit__6B24EA82]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Batting] ADD  DEFAULT ((0)) FOR [Hit]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Batting__Doubles__6C190EBB]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Batting] ADD  DEFAULT ((0)) FOR [Doubles]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Batting__Triples__6D0D32F4]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Batting] ADD  DEFAULT ((0)) FOR [Triples]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Batting__Homerun__6E01572D]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Batting] ADD  DEFAULT ((0)) FOR [Homeruns]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Batting__BaseOnB__6EF57B66]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Batting] ADD  DEFAULT ((0)) FOR [BaseOnBalls]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Batting__Strikeo__6FE99F9F]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Batting] ADD  DEFAULT ((0)) FOR [Strikeouts]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Batting__Sacrifi__70DDC3D8]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Batting] ADD  DEFAULT ((0)) FOR [SacrificeFlies]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Batting__Sacrifi__71D1E811]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Batting] ADD  DEFAULT ((0)) FOR [SacrificeBunts]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Batting__Intenti__72C60C4A]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Batting] ADD  DEFAULT ((0)) FOR [IntentionalBB]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Batting__HitByPi__73BA3083]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Batting] ADD  DEFAULT ((0)) FOR [HitByPitches]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Batting__StolenB__74AE54BC]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Batting] ADD  DEFAULT ((0)) FOR [StolenBases]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Batting__TimesCa__75A278F5]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Batting] ADD  DEFAULT ((0)) FOR [TimesCaughtStealing]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Batting__Runs__76969D2E]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Batting] ADD  DEFAULT ((0)) FOR [Runs]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Batting__Positio__778AC167]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Batting] ADD  DEFAULT ((0)) FOR [Positions]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Batting__Grounde__787EE5A0]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Batting] ADD  DEFAULT ((0)) FOR [GroundedIntoDoublePlay]
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Batting_FranchiseID]') AND parent_object_id = OBJECT_ID(N'[dbo].[Batting]'))
ALTER TABLE [dbo].[Batting]  WITH CHECK ADD  CONSTRAINT [FK_Batting_FranchiseID] FOREIGN KEY([FranchiseID])
REFERENCES [dbo].[Franchise] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Batting_FranchiseID]') AND parent_object_id = OBJECT_ID(N'[dbo].[Batting]'))
ALTER TABLE [dbo].[Batting] CHECK CONSTRAINT [FK_Batting_FranchiseID]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Batting_PersonID]') AND parent_object_id = OBJECT_ID(N'[dbo].[Batting]'))
ALTER TABLE [dbo].[Batting]  WITH CHECK ADD  CONSTRAINT [FK_Batting_PersonID] FOREIGN KEY([PersonID])
REFERENCES [dbo].[Person] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Batting_PersonID]') AND parent_object_id = OBJECT_ID(N'[dbo].[Batting]'))
ALTER TABLE [dbo].[Batting] CHECK CONSTRAINT [FK_Batting_PersonID]
GO

