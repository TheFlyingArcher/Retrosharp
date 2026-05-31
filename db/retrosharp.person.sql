USE [Retrosheet]
GO

/****** Object:  Table [dbo].[Person]    Script Date: 5/28/2026 3:56:39 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Person]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Person](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RetroSheetID] [varchar](16) NOT NULL,
	[Surname] [varchar](32) NULL,
	[UseName] [varchar](32) NULL,
	[FullName] [varchar](128) NULL,
	[BirthDate] [date] NULL,
	[BirthCity] [varchar](32) NULL,
	[BirthStateProvince] [varchar](32) NULL,
	[BirthCountry] [varchar](32) NULL,
	[DeathDate] [date] NULL,
	[DeathCity] [varchar](512) NULL,
	[DeathStateProvince] [varchar](32) NULL,
	[DeathCountry] [varchar](32) NULL,
	[Cemetary] [varchar](72) NULL,
	[CemetaryCity] [varchar](32) NULL,
	[CemetaryStateProv] [varchar](32) NULL,
	[CemetaryCountry] [varchar](32) NULL,
	[CemetaryNote] [varchar](1024) NULL,
	[BirthName] [varchar](128) NULL,
	[AlternateName] [varchar](128) NULL,
	[PlayerDebutDate] [date] NULL,
	[PlayerLastDate] [date] NULL,
	[CoachDebutDate] [date] NULL,
	[CoachLastDate] [date] NULL,
	[ManagerDebutDate] [date] NULL,
	[ManagerLastDate] [date] NULL,
	[UmpireDebutDate] [date] NULL,
	[UmpireLastDate] [date] NULL,
	[Bats] [varchar](2) NULL,
	[Throws] [varchar](2) NULL,
	[Height] [float] NULL,
	[Weight] [float] NULL,
	[IsHof] [bit] NOT NULL,
 CONSTRAINT [PK__Person__3214EC277C551CB9] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO

SET ANSI_PADDING ON
GO

/****** Object:  Index [IX_Person_Name]    Script Date: 5/28/2026 3:56:39 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Person]') AND name = N'IX_Person_Name')
CREATE NONCLUSTERED INDEX [IX_Person_Name] ON [dbo].[Person]
(
	[FullName] ASC
)
INCLUDE([Surname],[UseName],[BirthName],[AlternateName]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_PADDING ON
GO

/****** Object:  Index [IX_Person_RetrosheetID]    Script Date: 5/28/2026 3:56:39 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Person]') AND name = N'IX_Person_RetrosheetID')
CREATE NONCLUSTERED INDEX [IX_Person_RetrosheetID] ON [dbo].[Person]
(
	[RetroSheetID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Person__IsHof__3C69FB99]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Person] ADD  CONSTRAINT [DF__Person__IsHof__3C69FB99]  DEFAULT ((0)) FOR [IsHof]
END
GO

