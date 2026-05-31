USE [Retrosheet]
GO

/****** Object:  Table [dbo].[GameLineup]    Script Date: 5/28/2026 3:54:48 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GameLineup]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[GameLineup](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[GameID] [int] NOT NULL,
	[HomeVisitor] [varchar](1) NOT NULL,
	[LineupOrder] [tinyint] NOT NULL,
	[BatterID] [int] NOT NULL,
	[Position] [varchar](3) NULL,
 CONSTRAINT [PK_GameLineup] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO

/****** Object:  Index [IX_GameLineup_BatterID]    Script Date: 5/28/2026 3:54:48 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[GameLineup]') AND name = N'IX_GameLineup_BatterID')
CREATE NONCLUSTERED INDEX [IX_GameLineup_BatterID] ON [dbo].[GameLineup]
(
	[BatterID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

/****** Object:  Index [IX_GameLineup_GameID]    Script Date: 5/28/2026 3:54:48 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[GameLineup]') AND name = N'IX_GameLineup_GameID')
CREATE NONCLUSTERED INDEX [IX_GameLineup_GameID] ON [dbo].[GameLineup]
(
	[GameID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_GameLineup_Game]') AND parent_object_id = OBJECT_ID(N'[dbo].[GameLineup]'))
ALTER TABLE [dbo].[GameLineup]  WITH CHECK ADD  CONSTRAINT [FK_GameLineup_Game] FOREIGN KEY([GameID])
REFERENCES [dbo].[Game] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_GameLineup_Game]') AND parent_object_id = OBJECT_ID(N'[dbo].[GameLineup]'))
ALTER TABLE [dbo].[GameLineup] CHECK CONSTRAINT [FK_GameLineup_Game]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_GameLineup_Person]') AND parent_object_id = OBJECT_ID(N'[dbo].[GameLineup]'))
ALTER TABLE [dbo].[GameLineup]  WITH CHECK ADD  CONSTRAINT [FK_GameLineup_Person] FOREIGN KEY([BatterID])
REFERENCES [dbo].[Person] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_GameLineup_Person]') AND parent_object_id = OBJECT_ID(N'[dbo].[GameLineup]'))
ALTER TABLE [dbo].[GameLineup] CHECK CONSTRAINT [FK_GameLineup_Person]
GO

