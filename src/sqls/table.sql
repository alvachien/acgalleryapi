-- Album
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
	),
	CONSTRAINT [UX_AlbumTitle] UNIQUE NONCLUSTERED 
	(
		[Title] ASC
	)
);

-- Photo
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
	)
)

