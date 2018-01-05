USE [charterbiletixDB_green]
GO

/****** Объект: Table [dbo].[Catalog_Product] Дата скрипта: 05.01.2018 8:29:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Catalog_Product] (
    [Id]                             BIGINT             IDENTITY (1, 1) NOT NULL,
    [BrandId]                        BIGINT             NULL,
    [CreatedById]                    BIGINT             NULL,
    [CreatedOn]                      DATETIMEOFFSET (7) NOT NULL,
    [Description]                    NVARCHAR (MAX)     NULL,
    [DisplayOrder]                   INT                NOT NULL,
    [HasOptions]                     BIT                NOT NULL,
    [IsDeleted]                      BIT                NOT NULL,
    [IsFeatured]                     BIT                NOT NULL,
    [IsPublished]                    BIT                NOT NULL,
    [IsVisibleIndividually]          BIT                NOT NULL,
    [MetaDescription]                NVARCHAR (MAX)     NULL,
    [MetaKeywords]                   NVARCHAR (MAX)     NULL,
    [MetaTitle]                      NVARCHAR (MAX)     NULL,
    [Name]                           NVARCHAR (MAX)     NULL,
    [NormalizedName]                 NVARCHAR (MAX)     NULL,
    [OldPrice]                       DECIMAL (18, 2)    NULL,
    [Price]                          DECIMAL (18, 2)    NOT NULL,
    [PublishedOn]                    DATETIMEOFFSET (7) NULL,
    [SeoTitle]                       NVARCHAR (MAX)     NULL,
    [ShortDescription]               NVARCHAR (MAX)     NULL,
    [Sku]                            NVARCHAR (MAX)     NULL,
    [Specification]                  NVARCHAR (MAX)     NULL,
    [ThumbnailImageId]               BIGINT             NULL,
    [UpdatedById]                    BIGINT             NULL,
    [UpdatedOn]                      DATETIMEOFFSET (7) NOT NULL,
    [RatingAverage]                  FLOAT (53)         NULL,
    [ReviewsCount]                   INT                NOT NULL,
    [SpecialPrice]                   DECIMAL (18, 2)    NULL,
    [SpecialPriceEnd]                DATETIMEOFFSET (7) NULL,
    [SpecialPriceStart]              DATETIMEOFFSET (7) NULL,
    [IsAllowToOrder]                 BIT                NOT NULL,
    [IsCallForPricing]               BIT                NOT NULL,
    [StockQuantity]                  INT                NULL,
    [VendorId]                       BIGINT             NULL,
    [TaxClassId]                     BIGINT             NULL,
    [Via]                            NVARCHAR (MAX)     NULL,
    [Currency]                       NVARCHAR (MAX)     NULL,
    [Provider]                       NVARCHAR (MAX)     NULL,
    [IsRoundTrip]                    BIT                NULL,
    [Status]                         NVARCHAR (50)      NULL,
    [ReturnDepartureDate]            DATETIMEOFFSET (7) NULL,
    [ReturnLandingDate]              DATETIMEOFFSET (7) NULL,
    [ReturnFlightNumber]             NVARCHAR (50)      NULL,
    [ReturnCarrierId]                BIGINT             NULL,
    [ReturnAircraftId]               BIGINT             NULL,
    [ReturnTerminal]                 NVARCHAR (MAX)     NULL,
    [ReturnVia]                      NVARCHAR (MAX)     NULL,
    [FlightNumber]                   NVARCHAR (100)     NULL,
    [SoldSeats]                      INT                NULL,
    [SaleRtOnly]                     BIT                NULL,
    [AdminPayLater]                  BIT                NULL,
    [AdminRoundTrip]                 BIT                NULL,
    [AdminRoundTripOperatorId]       BIGINT             NULL,
    [AdminPayLaterRule]              NVARCHAR (100)     NULL,
    [AdminBlackList]                 NVARCHAR (MAX)     NULL,
    [AdminPasExpirityRule]           INT                NULL,
    [AdminIsSpecialOffer]            BIT                NULL,
    [AdminNotifyAgencies]            BIT                NULL,
    [AdminNotifyLastPassanger]       BIT                NULL,
    [AdminIsLastMinute]              BIT                NULL,
    [AdminReturnPayLater]            BIT                NULL,
    [AdminReturnPayLaterRule]        NVARCHAR (100)     NULL,
    [AdminReturnBlackList]           NVARCHAR (MAX)     NULL,
    [AdminReturnPasExpirityRule]     INT                NULL,
    [AdminReturnIsSpecialOffer]      BIT                NULL,
    [AdminReturnNotifyAgencies]      BIT                NULL,
    [AdminReturnNotifyLastPassanger] BIT                NULL,
    [AdminReturnIsLastMinute]        BIT                NULL,
    [ReservationNumber]              NVARCHAR (100)     NULL,
    [FlightClass]                    NVARCHAR (100)     NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_Catalog_Product_BrandId]
    ON [dbo].[Catalog_Product]([BrandId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Catalog_Product_CreatedById]
    ON [dbo].[Catalog_Product]([CreatedById] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Catalog_Product_ThumbnailImageId]
    ON [dbo].[Catalog_Product]([ThumbnailImageId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Catalog_Product_UpdatedById]
    ON [dbo].[Catalog_Product]([UpdatedById] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Catalog_Product_TaxClassId]
    ON [dbo].[Catalog_Product]([TaxClassId] ASC);


GO
ALTER TABLE [dbo].[Catalog_Product]
    ADD CONSTRAINT [PK_Catalog_Product] PRIMARY KEY CLUSTERED ([Id] ASC);


GO
ALTER TABLE [dbo].[Catalog_Product]
    ADD CONSTRAINT [FK_Catalog_Product_Core_User_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Core_User] ([Id]);


GO
ALTER TABLE [dbo].[Catalog_Product]
    ADD CONSTRAINT [FK_Catalog_Product_Core_User_UpdatedById] FOREIGN KEY ([UpdatedById]) REFERENCES [dbo].[Core_User] ([Id]);


GO
ALTER TABLE [dbo].[Catalog_Product]
    ADD CONSTRAINT [FK_Catalog_Product_Tax_TaxClass_TaxClassId] FOREIGN KEY ([TaxClassId]) REFERENCES [dbo].[Tax_TaxClass] ([Id]);


GO
ALTER TABLE [dbo].[Catalog_Product]
    ADD CONSTRAINT [FK_Catalog_Product_Catalog_Brand_ReturnCarrierId] FOREIGN KEY ([ReturnCarrierId]) REFERENCES [dbo].[Catalog_Brand] ([Id]);


GO
ALTER TABLE [dbo].[Catalog_Product]
    ADD CONSTRAINT [FK_ReturnAircraftId] FOREIGN KEY ([ReturnAircraftId]) REFERENCES [dbo].[Tax_TaxClass] ([Id]);


GO
ALTER TABLE [dbo].[Catalog_Product]
    ADD CONSTRAINT [FK_Catalog_Product_Catalog_Brand_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [dbo].[Catalog_Brand] ([Id]);


GO
ALTER TABLE [dbo].[Catalog_Product]
    ADD CONSTRAINT [FK_Catalog_Product_Core_Media_ThumbnailImageId] FOREIGN KEY ([ThumbnailImageId]) REFERENCES [dbo].[Core_Media] ([Id]);


