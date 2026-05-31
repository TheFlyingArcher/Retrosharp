USE [Retrosheet]
GO

/****** Object:  Table [dbo].[Franchise]    Script Date: 5/28/2026 3:54:12 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Franchise]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Franchise](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[LeagueID] [int] NULL,
	[FranchiseIdentifier] [varchar](4) NOT NULL,
	[FranchiseCode] [varchar](4) NOT NULL,
	[DivisionCode] [varchar](2) NULL,
	[FranchiseLocation] [varchar](32) NOT NULL,
	[Nickname] [varchar](64) NOT NULL,
	[AlternateNickname] [varchar](64) NULL,
	[FranchiseStart] [date] NOT NULL,
	[FranchiseEnd] [date] NULL,
	[PlayingCity] [varchar](32) NOT NULL,
	[PlayingState] [varchar](2) NOT NULL,
	[IsActive] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO

SET ANSI_PADDING ON
GO

/****** Object:  Index [IX_Franchise_CodeIentifier]    Script Date: 5/28/2026 3:54:13 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Franchise]') AND name = N'IX_Franchise_CodeIentifier')
CREATE NONCLUSTERED INDEX [IX_Franchise_CodeIentifier] ON [dbo].[Franchise]
(
	[FranchiseIdentifier] ASC,
	[FranchiseCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

/****** Object:  Index [IX_Franchise_LeagueID]    Script Date: 5/28/2026 3:54:13 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Franchise]') AND name = N'IX_Franchise_LeagueID')
CREATE NONCLUSTERED INDEX [IX_Franchise_LeagueID] ON [dbo].[Franchise]
(
	[LeagueID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Franchise_LeagueID]') AND parent_object_id = OBJECT_ID(N'[dbo].[Franchise]'))
ALTER TABLE [dbo].[Franchise]  WITH CHECK ADD  CONSTRAINT [FK_Franchise_LeagueID] FOREIGN KEY([LeagueID])
REFERENCES [dbo].[League] ([ID])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Franchise_LeagueID]') AND parent_object_id = OBJECT_ID(N'[dbo].[Franchise]'))
ALTER TABLE [dbo].[Franchise] CHECK CONSTRAINT [FK_Franchise_LeagueID]
GO

