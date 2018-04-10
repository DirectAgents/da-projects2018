
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 09/12/2012 22:32:18
-- Generated from EDMX file: C:\GitHub\da-projects\1\Cake.Model\Staging\CakeStaging.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [Cake];
GO
IF SCHEMA_ID(N'staging') IS NULL EXECUTE(N'CREATE SCHEMA [staging]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[staging].[CakeAdvertisers]', 'U') IS NOT NULL
    DROP TABLE [staging].[CakeAdvertisers];
GO
IF OBJECT_ID(N'[staging].[CakeAffiliates]', 'U') IS NOT NULL
    DROP TABLE [staging].[CakeAffiliates];
GO
IF OBJECT_ID(N'[staging].[CakeConversions]', 'U') IS NOT NULL
    DROP TABLE [staging].[CakeConversions];
GO
IF OBJECT_ID(N'[staging].[CakeOffers]', 'U') IS NOT NULL
    DROP TABLE [staging].[CakeOffers];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'CakeAdvertisers'
CREATE TABLE [staging].[CakeAdvertisers] (
    [Advertiser_Id] int  NOT NULL,
    [AdvertiserName] nvarchar(255)  NULL,
    [AccountManager_Id] int  NULL,
    [AccountManagerName] nvarchar(255)  NULL,
    [AdManagerName] nvarchar(255)  NULL
);
GO

-- Creating table 'CakeAffiliates'
CREATE TABLE [staging].[CakeAffiliates] (
    [Affiliate_Id] int  NOT NULL,
    [AffiliateName] nvarchar(255)  NULL,
    [AccountManager_Id] int  NULL,
    [StatusName] nvarchar(255)  NULL,
    [Website] nvarchar(255)  NULL,
    [AccountManagerName] nvarchar(255)  NULL,
    [Currency] nchar(3)  NULL
);
GO

-- Creating table 'CakeConversions'
CREATE TABLE [staging].[CakeConversions] (
    [Conversion_Id] int  NOT NULL,
    [ConversionDate] datetime  NULL,
    [Affiliate_Id] int  NULL,
    [Offer_Id] int  NULL,
    [Advertiser_Id] int  NULL,
    [Campaign_Id] int  NULL,
    [Creative_Id] int  NULL,
    [CreativeName] nvarchar(255)  NULL,
    [Subid1] nvarchar(255)  NULL,
    [ConversionType] nvarchar(50)  NULL,
    [PricePaid] decimal(19,4)  NULL,
    [PriceReceived] decimal(19,4)  NULL,
    [IpAddress] nvarchar(50)  NULL,
    [PricePaidCurrencyId] int  NULL,
    [PricePaidFormattedAmount] nvarchar(50)  NULL,
    [PriceReceivedCurrencyId] int  NULL,
    [PriceReceivedFormattedAmount] nvarchar(50)  NULL,
    [Deleted] bit  NOT NULL
);
GO

-- Creating table 'CakeOffers'
CREATE TABLE [staging].[CakeOffers] (
    [Offer_Id] int  NOT NULL,
    [OfferName] nvarchar(255)  NULL,
    [Advertiser_Id] nvarchar(255)  NULL,
    [VerticalName] nvarchar(255)  NULL,
    [OfferType] nvarchar(255)  NULL,
    [StatusName] nvarchar(255)  NULL,
    [DefaultPriceFormat] nvarchar(255)  NULL,
    [DefaultPayout] nvarchar(255)  NULL,
    [PriceReceived] nvarchar(255)  NULL,
    [Secure] nvarchar(255)  NULL,
    [OfferLink] nvarchar(255)  NULL,
    [ThumbnailImageUrl] nvarchar(255)  NULL,
    [ExpirationDate] nvarchar(255)  NULL,
    [CookieDays] nvarchar(255)  NULL,
    [CookieDaysImpressions] nvarchar(255)  NULL,
    [DateCreated] nvarchar(255)  NULL,
    [Currency] nchar(3)  NULL,
    [AllowedCountries] nvarchar(max)  NULL,
    [Xml] nvarchar(max)  NULL,
    [AllowedMediaTypeNames] nvarchar(max)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Advertiser_Id] in table 'CakeAdvertisers'
ALTER TABLE [staging].[CakeAdvertisers]
ADD CONSTRAINT [PK_CakeAdvertisers]
    PRIMARY KEY CLUSTERED ([Advertiser_Id] ASC);
GO

-- Creating primary key on [Affiliate_Id] in table 'CakeAffiliates'
ALTER TABLE [staging].[CakeAffiliates]
ADD CONSTRAINT [PK_CakeAffiliates]
    PRIMARY KEY CLUSTERED ([Affiliate_Id] ASC);
GO

-- Creating primary key on [Conversion_Id] in table 'CakeConversions'
ALTER TABLE [staging].[CakeConversions]
ADD CONSTRAINT [PK_CakeConversions]
    PRIMARY KEY CLUSTERED ([Conversion_Id] ASC);
GO

-- Creating primary key on [Offer_Id] in table 'CakeOffers'
ALTER TABLE [staging].[CakeOffers]
ADD CONSTRAINT [PK_CakeOffers]
    PRIMARY KEY CLUSTERED ([Offer_Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------