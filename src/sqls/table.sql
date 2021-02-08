-- Album
CREATE TABLE [Album] (
    [AlbumID]        INT            IDENTITY (1, 1) NOT NULL,
    [Title]          NVARCHAR (50)  NOT NULL,
    [Desp]           NVARCHAR (100) NULL,
    [CreatedBy]      NVARCHAR (50)  NULL,
    [CreateAt]       DATETIME       CONSTRAINT [DF_Album_CreateAt] DEFAULT (getdate()) NULL,
    [IsPublic]       BIT            CONSTRAINT [DF_Album_IsPublic] DEFAULT ((1)) NULL,
    [AccessCodeHint] NVARCHAR (50)  NULL,
    [AccessCode]     NVARCHAR (50)  NULL,
    CONSTRAINT [PK_Album] PRIMARY KEY CLUSTERED ([AlbumID] ASC),
    CONSTRAINT [UX_AlbumTitle] UNIQUE NONCLUSTERED ([Title] ASC)
);


-- Photo
CREATE TABLE [Photo] (
    [PhotoID]        NVARCHAR (40)  NOT NULL,
    [Title]          NVARCHAR (50)  NOT NULL,
    [Desp]           NVARCHAR (100) NULL,
    [Width]          INT            NULL,
    [Height]         INT            NULL,
    [ThumbWidth]     INT            NULL,
    [ThumbHeight]    INT            NULL,
    [UploadedAt]     DATETIME       CONSTRAINT [DF_Photo_UploadedAt] DEFAULT (getdate()) NULL,
    [UploadedBy]     NVARCHAR (50)  NULL,
    [OrgFileName]    NVARCHAR (100) NULL,
    [PhotoUrl]       NVARCHAR (100) NOT NULL,
    [PhotoThumbUrl]  NVARCHAR (100) NULL,
    [IsOrgThumb]     BIT            NULL,
    [ThumbCreatedBy] TINYINT        NULL,
    [CameraMaker]    NVARCHAR (50)  NULL,
    [CameraModel]    NVARCHAR (100) NULL,
    [LensModel]      NVARCHAR (100) NULL,
    [AVNumber]       NVARCHAR (20)  NULL,
    [ShutterSpeed]   NVARCHAR (50)  NULL,
    [ISONumber]      INT            NULL,
    [IsPublic]       BIT            CONSTRAINT [DF_Photo_IsPublic] DEFAULT ((1)) NULL,
    [EXIFInfo]       NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_Photo] PRIMARY KEY CLUSTERED ([PhotoID] ASC)
);

-- Table: AlbumPhoto
CREATE TABLE [AlbumPhoto] (
    [AlbumID] INT           NOT NULL,
    [PhotoID] NVARCHAR (40) NOT NULL,
    CONSTRAINT [PK_AlbumPhoto] PRIMARY KEY CLUSTERED ([AlbumID] ASC, [PhotoID] ASC),
    CONSTRAINT [FK_AlbumPhoto_Photo] FOREIGN KEY ([PhotoID]) REFERENCES [Photo] ([PhotoID]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_AlbumPhoto_Album] FOREIGN KEY ([AlbumID]) REFERENCES [Album] ([AlbumID]) ON DELETE CASCADE ON UPDATE CASCADE
);


-- Table: PhotoTag
CREATE TABLE [PhotoTag] (
    [PhotoID] NVARCHAR (40) NOT NULL,
    [Tag]     NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_PhotoTag] PRIMARY KEY CLUSTERED ([PhotoID] ASC, [Tag] ASC),
    CONSTRAINT [FK_PhotoTag_Photo] FOREIGN KEY ([PhotoID]) REFERENCES [Photo] ([PhotoID]) ON DELETE CASCADE ON UPDATE CASCADE
);

-- Table: UserDetail
CREATE TABLE [UserDetail] (
    [UserID]            NVARCHAR (50) NOT NULL,
    [DisplayAs]         NVARCHAR (50) NOT NULL,
    [UploadFileMinSize] INT           NULL,
    [UploadFileMaxSize] INT           NULL,
    [AlbumCreate]       BIT           NULL,
    [AlbumChange]       TINYINT       NULL,
    [AlbumDelete]       TINYINT       NULL,
    [PhotoUpload]       BIT           NULL,
    [PhotoChange]       TINYINT       NULL,
    [PhotoDelete]       TINYINT       NULL,
    [AlbumRead]         TINYINT       NULL,
    CONSTRAINT [PK_UserDetail] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

-- Table: PhotoRating
CREATE TABLE [PhotoRating] (
    [PhotoID] NVARCHAR (40) NOT NULL,
    [User]    NVARCHAR (50) NOT NULL,
    [Rating]  TINYINT       NOT NULL,
    CONSTRAINT [PK_PhotoRating] PRIMARY KEY CLUSTERED ([PhotoID] ASC, [User] ASC),
    CONSTRAINT [FK_PhotoRating_Photo] FOREIGN KEY ([PhotoID]) REFERENCES [Photo] ([PhotoID]) ON DELETE CASCADE ON UPDATE CASCADE
);
