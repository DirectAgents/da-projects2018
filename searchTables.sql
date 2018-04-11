/****** Object:  Table [dbo].[SearchConvType]    Script Date: 04/10/2018 15:30:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SearchConvType](
	[SearchConvTypeId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Alias] [nvarchar](255) NULL,
 CONSTRAINT [PK_dbo.SearchConvType] PRIMARY KEY CLUSTERED 
(
	[SearchConvTypeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SearchProfile]    Script Date: 04/10/2018 15:30:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SearchProfile](
	[SearchProfileId] [int] NOT NULL,
	[SearchProfileName] [nvarchar](max) NULL,
	[StartDayOfWeek] [int] NOT NULL,
	[ShowSearchChannels] [bit] NOT NULL,
	[LCaccid] [nvarchar](100) NULL,
	[CallMinSeconds] [int] NOT NULL,
	[ShowRevenue] [bit] NOT NULL,
	[UseConvertedClicks] [bit] NOT NULL,
	[ShowViewThrus] [bit] NOT NULL,
	[ShowCassConvs] [bit] NOT NULL,
	[UseAllConvs] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.SearchProfile] PRIMARY KEY CLUSTERED 
(
	[SearchProfileId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Employee]    Script Date: 04/10/2018 15:30:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Employee](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](max) NULL,
	[LastName] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Employee] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SearchAccount]    Script Date: 04/10/2018 15:30:20 ******/
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
 CONSTRAINT [PK_dbo.SearchAccount] PRIMARY KEY CLUSTERED 
(
	[SearchAccountId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SearchCampaign]    Script Date: 04/10/2018 15:30:20 ******/
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
 CONSTRAINT [PK_dbo.SearchCampaign] PRIMARY KEY CLUSTERED 
(
	[SearchCampaignId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SearchDailySummary]    Script Date: 04/10/2018 15:30:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SearchDailySummary](
	[SearchCampaignId] [int] NOT NULL,
	[Date] [datetime] NOT NULL,
	[Network] [nchar](1) NOT NULL,
	[Device] [nchar](1) NOT NULL,
	[Revenue] [money] NOT NULL,
	[Cost] [money] NOT NULL,
	[Orders] [int] NOT NULL,
	[Clicks] [int] NOT NULL,
	[Impressions] [int] NOT NULL,
	[CurrencyId] [int] NOT NULL,
	[ViewThrus] [int] NOT NULL,
	[CassConvs] [int] NOT NULL,
	[CassConVal] [float] NOT NULL,
 CONSTRAINT [PK_dbo.SearchDailySummary] PRIMARY KEY CLUSTERED 
(
	[SearchCampaignId] ASC,
	[Date] ASC,
	[Network] ASC,
	[Device] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SearchConvSummary]    Script Date: 04/10/2018 15:30:20 ******/
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
 CONSTRAINT [PK_dbo.SearchConvSummary] PRIMARY KEY CLUSTERED 
(
	[SearchCampaignId] ASC,
	[Date] ASC,
	[SearchConvTypeId] ASC,
	[Network] ASC,
	[Device] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CallDailySummary]    Script Date: 04/10/2018 15:30:20 ******/
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
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  ForeignKey [FK_CallDailySummary_SearchCampaign]    Script Date: 04/10/2018 15:30:20 ******/
ALTER TABLE [dbo].[CallDailySummary]  WITH CHECK ADD  CONSTRAINT [FK_CallDailySummary_SearchCampaign] FOREIGN KEY([SearchCampaignId])
REFERENCES [dbo].[SearchCampaign] ([SearchCampaignId])
GO
ALTER TABLE [dbo].[CallDailySummary] CHECK CONSTRAINT [FK_CallDailySummary_SearchCampaign]
GO
/****** Object:  ForeignKey [FK_dbo.SearchAccount_dbo.SearchProfile_SearchProfileId]    Script Date: 04/10/2018 15:30:20 ******/
ALTER TABLE [dbo].[SearchAccount]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SearchAccount_dbo.SearchProfile_SearchProfileId] FOREIGN KEY([SearchProfileId])
REFERENCES [dbo].[SearchProfile] ([SearchProfileId])
GO
ALTER TABLE [dbo].[SearchAccount] CHECK CONSTRAINT [FK_dbo.SearchAccount_dbo.SearchProfile_SearchProfileId]
GO
/****** Object:  ForeignKey [FK_dbo.SearchCampaign_dbo.SearchAccount_AltSearchAccountId]    Script Date: 04/10/2018 15:30:20 ******/
ALTER TABLE [dbo].[SearchCampaign]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SearchCampaign_dbo.SearchAccount_AltSearchAccountId] FOREIGN KEY([AltSearchAccountId])
REFERENCES [dbo].[SearchAccount] ([SearchAccountId])
GO
ALTER TABLE [dbo].[SearchCampaign] CHECK CONSTRAINT [FK_dbo.SearchCampaign_dbo.SearchAccount_AltSearchAccountId]
GO
/****** Object:  ForeignKey [FK_dbo.SearchCampaign_dbo.SearchAccount_SearchAccountId]    Script Date: 04/10/2018 15:30:20 ******/
ALTER TABLE [dbo].[SearchCampaign]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SearchCampaign_dbo.SearchAccount_SearchAccountId] FOREIGN KEY([SearchAccountId])
REFERENCES [dbo].[SearchAccount] ([SearchAccountId])
GO
ALTER TABLE [dbo].[SearchCampaign] CHECK CONSTRAINT [FK_dbo.SearchCampaign_dbo.SearchAccount_SearchAccountId]
GO
/****** Object:  ForeignKey [FK_dbo.SearchConvSummary_dbo.SearchCampaign_SearchCampaignId]    Script Date: 04/10/2018 15:30:20 ******/
ALTER TABLE [dbo].[SearchConvSummary]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SearchConvSummary_dbo.SearchCampaign_SearchCampaignId] FOREIGN KEY([SearchCampaignId])
REFERENCES [dbo].[SearchCampaign] ([SearchCampaignId])
GO
ALTER TABLE [dbo].[SearchConvSummary] CHECK CONSTRAINT [FK_dbo.SearchConvSummary_dbo.SearchCampaign_SearchCampaignId]
GO
/****** Object:  ForeignKey [FK_dbo.SearchConvSummary_dbo.SearchConvType_SearchConvTypeId]    Script Date: 04/10/2018 15:30:20 ******/
ALTER TABLE [dbo].[SearchConvSummary]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SearchConvSummary_dbo.SearchConvType_SearchConvTypeId] FOREIGN KEY([SearchConvTypeId])
REFERENCES [dbo].[SearchConvType] ([SearchConvTypeId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SearchConvSummary] CHECK CONSTRAINT [FK_dbo.SearchConvSummary_dbo.SearchConvType_SearchConvTypeId]
GO
/****** Object:  ForeignKey [FK_dbo.SearchDailySummary_dbo.SearchCampaign_SearchCampaignId]    Script Date: 04/10/2018 15:30:20 ******/
ALTER TABLE [dbo].[SearchDailySummary]  WITH CHECK ADD  CONSTRAINT [FK_dbo.SearchDailySummary_dbo.SearchCampaign_SearchCampaignId] FOREIGN KEY([SearchCampaignId])
REFERENCES [dbo].[SearchCampaign] ([SearchCampaignId])
GO
ALTER TABLE [dbo].[SearchDailySummary] CHECK CONSTRAINT [FK_dbo.SearchDailySummary_dbo.SearchCampaign_SearchCampaignId]
GO
