USE [acgallery];
GO
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