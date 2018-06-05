/****** Object:  Table [dbo].[Album]    Script Date: 10/16/2016 7:02:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Album](
	[AlbumID] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](50) NOT NULL,
	[Desp] [nvarchar](100) NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[CreateAt] [datetime] NULL,
	[IsPublic] [bit] NULL,
	[AccessCodeHint] [nvarchar](50) NULL,
	[AccessCode] [nvarchar](50) NULL,
 CONSTRAINT [PK_Album] PRIMARY KEY CLUSTERED 
(
	[AlbumID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
 CONSTRAINT [UX_AlbumTitle] UNIQUE NONCLUSTERED 
(
	[Title] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[AlbumPhoto]    Script Date: 10/16/2016 7:02:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AlbumPhoto](
	[AlbumID] [int] NOT NULL,
	[PhotoID] [nvarchar](40) NOT NULL,
 CONSTRAINT [PK_AlbumPhoto] PRIMARY KEY CLUSTERED 
(
	[AlbumID] ASC,
	[PhotoID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
/****** Object:  Table [dbo].[Photo]    Script Date: 10/16/2016 7:02:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Photo](
	[PhotoID] [nvarchar](40) NOT NULL,
	[Title] [nvarchar](50) NOT NULL,
	[Desp] [nvarchar](100) NULL,
	[Width] [int] NULL,
	[Height] [int] NULL,
	[ThumbWidth] [int] NULL,
	[ThumbHeight] [int] NULL,
	[UploadedAt] [datetime] NULL,
	[UploadedBy] [nvarchar](50) NULL,
	[OrgFileName] [nvarchar](100) NULL,
	[PhotoUrl] [nvarchar](100) NOT NULL,
	[PhotoThumbUrl] [nvarchar](100) NULL,
	[IsOrgThumb] [bit] NULL,
	[ThumbCreatedBy] [tinyint] NULL,
	[CameraMaker] [nvarchar](50) NULL,
	[CameraModel] [nvarchar](100) NULL,
	[LensModel] [nvarchar](100) NULL,
	[AVNumber] [nvarchar](20) NULL,
	[ShutterSpeed] [nvarchar](50) NULL,
	[ISONumber] [int] NULL,
	[IsPublic] [bit] NULL,
	[EXIFInfo] [nvarchar](max) NULL,
 CONSTRAINT [PK_Photo] PRIMARY KEY CLUSTERED 
(
	[PhotoID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
ALTER TABLE [dbo].[Album] ADD  CONSTRAINT [DF_Album_CreateAt]  DEFAULT (getdate()) FOR [CreateAt]
GO
ALTER TABLE [dbo].[Album] ADD  CONSTRAINT [DF_Album_IsPublic]  DEFAULT ((1)) FOR [IsPublic]
GO
ALTER TABLE [dbo].[Photo] ADD  CONSTRAINT [DF_Photo_UploadedAt]  DEFAULT (getdate()) FOR [UploadedAt]
GO
ALTER TABLE [dbo].[Photo] ADD  CONSTRAINT [DF_Photo_IsPublic]  DEFAULT ((1)) FOR [IsPublic]
GO
ALTER TABLE [dbo].[AlbumPhoto]  WITH CHECK ADD  CONSTRAINT [FK_AlbumPhoto_Album] FOREIGN KEY([AlbumID])
REFERENCES [dbo].[Album] ([AlbumID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AlbumPhoto] CHECK CONSTRAINT [FK_AlbumPhoto_Album]
GO
ALTER TABLE [dbo].[AlbumPhoto]  WITH CHECK ADD  CONSTRAINT [FK_AlbumPhoto_Photo] FOREIGN KEY([PhotoID])
REFERENCES [dbo].[Photo] ([PhotoID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AlbumPhoto] CHECK CONSTRAINT [FK_AlbumPhoto_Photo]
GO

/* UPDATED at 2018.01.07, For TAGS */
/****** Object:  Table [dbo].[PhotoTag]    Script Date: 1/7/2018 1:22:20 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PhotoTag](
	[PhotoID] [nvarchar](40) NOT NULL,
	[Tag] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_PhotoTag] PRIMARY KEY CLUSTERED 
(
	[PhotoID] ASC,
	[Tag] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[PhotoTag]  WITH CHECK ADD  CONSTRAINT [FK_PhotoTag_Photo] FOREIGN KEY([PhotoID])
REFERENCES [dbo].[Photo] ([PhotoID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[PhotoTag] CHECK CONSTRAINT [FK_PhotoTag_Photo]
GO

/* UPDATED at 2018.6.5 */
/****** Object:  Table [dbo].[PhotoRating]    Script Date: 6/5/2018 1:22:20 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PhotoRating](
	[PhotoID] [nvarchar](40) NOT NULL,
	[User] [nvarchar](50) NOT NULL,
	[Rating] [tinyint] NOT NULL
 CONSTRAINT [PK_PhotoRating] PRIMARY KEY CLUSTERED 
(
	[PhotoID] ASC,
	[User] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[PhotoRating]  WITH CHECK ADD  CONSTRAINT [FK_PhotoRating_Photo] FOREIGN KEY([PhotoID])
REFERENCES [dbo].[Photo] ([PhotoID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[PhotoRating] CHECK CONSTRAINT [FK_PhotoRating_Photo]
GO

/* Add hint to access code */
IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Album' AND COLUMN_NAME = 'AccessCodeHint')
BEGIN

	ALTER TABLE [dbo].[Album]
	ADD [AccessCodeHint] [nvarchar](50) NULL;

END
GO

/****** Object:  Table [dbo].[UserDetail]    Script Date: 2018-06-05 2:12:37 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[UserDetail](
	[UserID] [nvarchar](50) NOT NULL,
	[DisplayAs] [nvarchar](50) NOT NULL,
	[UploadFileMinSize] [int] NULL,
	[UploadFileMaxSize] [int] NULL,
	[AlbumCreate] [bit] NULL,
	[AlbumChange] [tinyint] NULL,
	[AlbumDelete] [tinyint] NULL,
	[PhotoUpload] [bit] NULL,
	[PhotoChange] [tinyint] NULL,
	[PhotoDelete] [tinyint] NULL,
	[AlbumRead] [tinyint] NULL,
 CONSTRAINT [PK_UserDetail] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
