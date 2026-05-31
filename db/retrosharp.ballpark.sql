USE [Retrosheet]
GO

/****** Object:  Table [dbo].[Ballpark]    Script Date: 5/28/2026 3:53:39 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Ballpark]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Ballpark](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SiteCode] [varchar](6) NOT NULL,
	[ParkName] [varchar](64) NULL,
	[City] [varchar](32) NOT NULL,
	[StateProvinceCountry] [varchar](32) NULL,
	[FirstGame] [date] NOT NULL,
	[LastGame] [date] NULL,
 CONSTRAINT [PK_Ballpark] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO

SET ANSI_PADDING ON
GO

/****** Object:  Index [IX_Ballpark_SiteCode]    Script Date: 5/28/2026 3:53:39 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Ballpark]') AND name = N'IX_Ballpark_SiteCode')
CREATE NONCLUSTERED INDEX [IX_Ballpark_SiteCode] ON [dbo].[Ballpark]
(
	[SiteCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

