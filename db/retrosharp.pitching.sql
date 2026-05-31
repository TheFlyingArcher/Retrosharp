USE [Retrosheet]
GO

/****** Object:  Table [dbo].[Pitching]    Script Date: 5/28/2026 3:57:42 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Pitching]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Pitching](
	[ID] [int] NOT NULL,
	[PersonID] [int] NOT NULL,
	[FranchiseID] [int] NOT NULL,
	[SeasonYear] [smallint] NOT NULL,
	[Position] [varchar](2) NOT NULL,
	[GamesPitched] [smallint] NOT NULL,
	[GamesStarted] [smallint] NOT NULL,
	[GamesFinished] [smallint] NOT NULL,
	[CompleteGames] [smallint] NOT NULL,
	[Shutouts] [smallint] NOT NULL,
	[Saves] [smallint] NOT NULL,
	[InningsPitched] [smallint] NOT NULL,
	[Hits] [smallint] NOT NULL,
	[Runs] [smallint] NOT NULL,
	[EarnedRuns] [smallint] NOT NULL,
	[BaseOnBalls] [smallint] NOT NULL,
	[Strikeouts] [smallint] NOT NULL,
	[IntentionalBB] [smallint] NOT NULL,
	[HitBatsmen] [smallint] NOT NULL,
	[Balks] [smallint] NOT NULL,
	[WildPitches] [smallint] NOT NULL
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Pitching__GamesP__797309D9]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Pitching] ADD  DEFAULT ((0)) FOR [GamesPitched]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Pitching__GamesS__7A672E12]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Pitching] ADD  DEFAULT ((0)) FOR [GamesStarted]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Pitching__GamesF__7B5B524B]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Pitching] ADD  DEFAULT ((0)) FOR [GamesFinished]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Pitching__Comple__7C4F7684]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Pitching] ADD  DEFAULT ((0)) FOR [CompleteGames]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Pitching__Shutou__7D439ABD]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Pitching] ADD  DEFAULT ((0)) FOR [Shutouts]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Pitching__Saves__7E37BEF6]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Pitching] ADD  DEFAULT ((0)) FOR [Saves]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Pitching__Inning__7F2BE32F]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Pitching] ADD  DEFAULT ((0)) FOR [InningsPitched]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Pitching__Hits__00200768]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Pitching] ADD  DEFAULT ((0)) FOR [Hits]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Pitching__Runs__01142BA1]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Pitching] ADD  DEFAULT ((0)) FOR [Runs]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Pitching__Earned__02084FDA]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Pitching] ADD  DEFAULT ((0)) FOR [EarnedRuns]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Pitching__BaseOn__02FC7413]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Pitching] ADD  DEFAULT ((0)) FOR [BaseOnBalls]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Pitching__Strike__03F0984C]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Pitching] ADD  DEFAULT ((0)) FOR [Strikeouts]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Pitching__Intent__04E4BC85]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Pitching] ADD  DEFAULT ((0)) FOR [IntentionalBB]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Pitching__HitBat__05D8E0BE]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Pitching] ADD  DEFAULT ((0)) FOR [HitBatsmen]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Pitching__Balks__06CD04F7]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Pitching] ADD  DEFAULT ((0)) FOR [Balks]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Pitching__WildPi__07C12930]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Pitching] ADD  DEFAULT ((0)) FOR [WildPitches]
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Pitching_FranchiseID]') AND parent_object_id = OBJECT_ID(N'[dbo].[Pitching]'))
ALTER TABLE [dbo].[Pitching]  WITH CHECK ADD  CONSTRAINT [FK_Pitching_FranchiseID] FOREIGN KEY([FranchiseID])
REFERENCES [dbo].[Franchise] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Pitching_FranchiseID]') AND parent_object_id = OBJECT_ID(N'[dbo].[Pitching]'))
ALTER TABLE [dbo].[Pitching] CHECK CONSTRAINT [FK_Pitching_FranchiseID]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Pitching_PersonID]') AND parent_object_id = OBJECT_ID(N'[dbo].[Pitching]'))
ALTER TABLE [dbo].[Pitching]  WITH CHECK ADD  CONSTRAINT [FK_Pitching_PersonID] FOREIGN KEY([PersonID])
REFERENCES [dbo].[Person] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Pitching_PersonID]') AND parent_object_id = OBJECT_ID(N'[dbo].[Pitching]'))
ALTER TABLE [dbo].[Pitching] CHECK CONSTRAINT [FK_Pitching_PersonID]
GO

