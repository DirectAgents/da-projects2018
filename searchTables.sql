/****** Object:  Table [dbo].[CallDailySummary]    Script Date: 6/22/2018 6:10:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CallDailySummary](
	[SearchCampaignId] [int] NOT NULL,
	[Date] [date] NOT NULL,
	[Calls] [int] NOT NULL,
 CONSTRAINT [PK_CallDailySummary] PRIMARY KEY CLUSTERED 
(
	[SearchCampaignId] ASC,
	[Date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[ClientReport]    Script Date: 6/22/2018 6:10:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ClientReport](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[StartDayOfWeek] [int] NOT NULL CONSTRAINT [DF_ClientReport_StartDayOfWeek]  DEFAULT ((1)),
	[SearchProfileId] [int] NULL,
	[ProgCampaignId] [int] NULL,
 CONSTRAINT [PK_ClientReport] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[SearchAccount]    Script Date: 6/22/2018 6:10:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SearchAccount](
	[SearchAccountId] [int] IDENTITY(1,1) NOT NULL,
	[AdvertiserId] [int] NULL,
	[Name] [nvarchar](255) NULL,
	[Channel] [nvarchar](50) NULL,
	[AccountCode] [nvarchar](50) NULL,
	[ExternalId] [nvarchar](50) NULL,
	[SearchProfileId] [int] NULL,
	[RevPerOrder] [decimal](14, 2) NULL,
	[MinSynchDate] [date] NULL,
 CONSTRAINT [PK_SearchAccount] PRIMARY KEY CLUSTERED 
(
	[SearchAccountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[SearchCampaign]    Script Date: 6/22/2018 6:10:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SearchCampaign](
	[SearchCampaignId] [int] IDENTITY(1,1) NOT NULL,
	[SearchCampaignName] [nvarchar](255) NOT NULL,
	[AdvertiserId] [int] NULL,
	[Channel] [nvarchar](255) NULL,
	[ExternalId] [int] NULL,
	[SearchAccountId] [int] NULL,
	[AltSearchAccountId] [int] NULL,
	[LCcmpid] [nvarchar](100) NULL,
 CONSTRAINT [PK_SearchCampaign] PRIMARY KEY CLUSTERED 
(
	[SearchCampaignId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[SearchConvSummary]    Script Date: 6/22/2018 6:10:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SearchConvSummary](
	[SearchCampaignId] [int] NOT NULL,
	[Date] [datetime] NOT NULL,
	[SearchConvTypeId] [int] NOT NULL,
	[Network] [nchar](1) NOT NULL,
	[Device] [nchar](1) NOT NULL,
	[Conversions] [float] NOT NULL,
	[ConVal] [decimal](18, 6) NOT NULL,
 CONSTRAINT [PK_SearchConvSummary] PRIMARY KEY CLUSTERED 
(
	[SearchCampaignId] ASC,
	[Date] ASC,
	[SearchConvTypeId] ASC,
	[Network] ASC,
	[Device] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[SearchConvType]    Script Date: 6/22/2018 6:10:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SearchConvType](
	[SearchConvTypeId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Alias] [nvarchar](255) NULL,
 CONSTRAINT [PK_SearchConvType] PRIMARY KEY CLUSTERED 
(
	[SearchConvTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
 CONSTRAINT [IX_SearchConvType] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[SearchDailySummary]    Script Date: 6/22/2018 6:10:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SearchDailySummary](
	[SearchCampaignId] [int] NOT NULL,
	[Date] [datetime] NOT NULL,
	[Revenue] [money] NOT NULL,
	[Cost] [money] NOT NULL,
	[Orders] [int] NOT NULL,
	[Clicks] [int] NOT NULL,
	[Impressions] [int] NOT NULL,
	[CurrencyId] [int] NOT NULL,
	[Network] [nchar](1) NOT NULL,
	[Device] [nchar](1) NOT NULL,
	[ViewThrus] [int] NOT NULL CONSTRAINT [DF_SearchDailySummary_ViewThrus]  DEFAULT ((0)),
	[CassConvs] [int] NOT NULL CONSTRAINT [DF_SearchDailySummary_CassConvs]  DEFAULT ((0)),
	[CassConVal] [float] NOT NULL CONSTRAINT [DF_SearchDailySummary_CassConVal]  DEFAULT ((0)),
 CONSTRAINT [PK_SearchDailySummary] PRIMARY KEY CLUSTERED 
(
	[SearchCampaignId] ASC,
	[Date] ASC,
	[Network] ASC,
	[Device] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[SearchProfile]    Script Date: 6/22/2018 6:10:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SearchProfile](
	[SearchProfileId] [int] NOT NULL,
	[SearchProfileName] [nvarchar](max) NULL,
	[StartDayOfWeek] [int] NOT NULL CONSTRAINT [DF_SearchProfile_StartDayOfWeek]  DEFAULT ((1)),
	[ShowSearchChannels] [bit] NOT NULL CONSTRAINT [DF_SearchProfile_ShowSearchChannels]  DEFAULT ((0)),
	[LCaccid] [nvarchar](100) NULL,
	[CallMinSeconds] [int] NOT NULL CONSTRAINT [DF_SearchProfile_CallMinSeconds]  DEFAULT ((0)),
	[ShowRevenue] [bit] NOT NULL CONSTRAINT [DF_SearchProfile_ShowRevenue]  DEFAULT ((0)),
	[UseConvertedClicks] [bit] NOT NULL CONSTRAINT [DF_SearchProfile_UseConvertedClicks]  DEFAULT ((1)),
	[ShowViewThrus] [bit] NOT NULL CONSTRAINT [DF_SearchProfile_ShowViewThrus]  DEFAULT ((0)),
	[ShowCassConvs] [bit] NOT NULL CONSTRAINT [DF_SearchProfile_ShowCassConvs]  DEFAULT ((0)),
	[UseAllConvs] [bit] NOT NULL CONSTRAINT [DF_SearchProfile_UseAllConvs]  DEFAULT ((0)),
 CONSTRAINT [PK_SearchProfile] PRIMARY KEY CLUSTERED 
(
	[SearchProfileId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
ALTER TABLE [dbo].[CallDailySummary]  WITH CHECK ADD  CONSTRAINT [FK_CallDailySummary_SearchCampaign] FOREIGN KEY([SearchCampaignId])
REFERENCES [dbo].[SearchCampaign] ([SearchCampaignId])
GO
ALTER TABLE [dbo].[CallDailySummary] CHECK CONSTRAINT [FK_CallDailySummary_SearchCampaign]
GO
ALTER TABLE [dbo].[SearchAccount]  WITH CHECK ADD  CONSTRAINT [FK_SearchAccount_SearchProfile] FOREIGN KEY([SearchProfileId])
REFERENCES [dbo].[SearchProfile] ([SearchProfileId])
GO
ALTER TABLE [dbo].[SearchAccount] CHECK CONSTRAINT [FK_SearchAccount_SearchProfile]
GO
ALTER TABLE [dbo].[SearchCampaign]  WITH CHECK ADD  CONSTRAINT [FK_SearchCampaign_SearchAccount] FOREIGN KEY([SearchAccountId])
REFERENCES [dbo].[SearchAccount] ([SearchAccountId])
GO
ALTER TABLE [dbo].[SearchCampaign] CHECK CONSTRAINT [FK_SearchCampaign_SearchAccount]
GO
ALTER TABLE [dbo].[SearchCampaign]  WITH CHECK ADD  CONSTRAINT [FK_SearchCampaign_SearchAccount1] FOREIGN KEY([AltSearchAccountId])
REFERENCES [dbo].[SearchAccount] ([SearchAccountId])
GO
ALTER TABLE [dbo].[SearchCampaign] CHECK CONSTRAINT [FK_SearchCampaign_SearchAccount1]
GO
ALTER TABLE [dbo].[SearchConvSummary]  WITH CHECK ADD  CONSTRAINT [FK_SearchConvSummary_SearchCampaign] FOREIGN KEY([SearchCampaignId])
REFERENCES [dbo].[SearchCampaign] ([SearchCampaignId])
GO
ALTER TABLE [dbo].[SearchConvSummary] CHECK CONSTRAINT [FK_SearchConvSummary_SearchCampaign]
GO
ALTER TABLE [dbo].[SearchConvSummary]  WITH CHECK ADD  CONSTRAINT [FK_SearchConvSummary_SearchConvType] FOREIGN KEY([SearchConvTypeId])
REFERENCES [dbo].[SearchConvType] ([SearchConvTypeId])
GO
ALTER TABLE [dbo].[SearchConvSummary] CHECK CONSTRAINT [FK_SearchConvSummary_SearchConvType]
GO
ALTER TABLE [dbo].[SearchDailySummary]  WITH CHECK ADD  CONSTRAINT [FK_SearchDailySummary_SearchCampaign] FOREIGN KEY([SearchCampaignId])
REFERENCES [dbo].[SearchCampaign] ([SearchCampaignId])
GO
ALTER TABLE [dbo].[SearchDailySummary] CHECK CONSTRAINT [FK_SearchDailySummary_SearchCampaign]
GO
