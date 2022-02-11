using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GalleryAPI.test.common
{
    public static class DataSetupUtility
    {
        /// <summary>
        /// Testing data
        /// User A
        ///     [Public] Album 1
        ///         Photo A1001
        ///         Photo A1002
        ///     [Private] Album 2
        ///         Photo A2001
        ///         Photo A2002
        /// User B
        ///     [Private] Album 11
        ///         Photo B1101
        ///         Photo B1102
        ///         Photo B1103
        /// Public Photos Without User
        ///         Photo ZZ001
        ///         Photo ZZ002
        ///         Photo ZZ003
        /// </summary>
        public const string UserA = "USERA";
        public const string UserB = "USERB";
        public const int Album1ID = 1;
        public const int Album2ID = 2;
        public const int Album11ID = 11;

        #region Create tables and Views
        public static void CreateDatabaseTables(DatabaseFacade database)
        {
            // 1. Album
            database.ExecuteSqlRaw(@"CREATE TABLE ALBUM (
	            ALBUMID INTEGER PRIMARY KEY AUTOINCREMENT,
	            TITLE nvarchar(50) NOT NULL,
	            DESP nvarchar(100) NULL,
	            CREATEDBY nvarchar(50) NULL,
	            CREATEDAT date NULL DEFAULT CURRENT_DATE,
	            UPDATEDBY nvarchar(50) NULL,
	            UPDATEDAT date NULL DEFAULT CURRENT_DATE,
                ISPUBLIC bit NULL DEFAULT 1,
                ACCESSCODEHINT nvarchar(50) NULL,
                ACCESSCODE nvarchar(50) NULL,
                CONSTRAINT UK_AlbumTitle UNIQUE (TITLE) )"
            );

            // 2. Photo
            database.ExecuteSqlRaw(@"CREATE TABLE PHOTO (
	            PHOTOID NVARCHAR(40) PRIMARY KEY,
	            TITLE nvarchar(50) NOT NULL,
	            DESP nvarchar(100) NULL,
                WIDTH INT NULL,
                HEIGHT INT NULL,
                THUMBWIDTH INT NULL,
                THUMBHEIGHT INT NULL,
                UPLOADEDAT date NULL DEFAULT CURRENT_DATE,
                UPLOADEDBY NVARCHAR (50)  NULL,
                ORGFILENAME NVARCHAR (100) NULL,
                PHOTOURL NVARCHAR (100) NOT NULL,
                PHOTOTHUMBURL NVARCHAR (100) NULL,
                ISORGTHUMB BIT NULL,
                THUMBCREATEDBY TINYINT NULL,
                CAMERAMAKER NVARCHAR (50)  NULL,
                CAMERAMODEL NVARCHAR (100) NULL,
                LENSMODEL NVARCHAR (100) NULL,
                AVNUMBER NVARCHAR (20)  NULL,
                SHUTTERSPEED NVARCHAR (50)  NULL,
                ISONUMBER INT NULL,
                ISPUBLIC BIT DEFAULT 1, 
                EXIFINFO TEXT NULL )"
            );

            // 3. Album Photo
            database.ExecuteSqlRaw(@"CREATE TABLE ALBUMPHOTO (
                ALBUMID INT NOT NULL,
                PHOTOID nvarchar(40) NOT NULL,
                CONSTRAINT PK_ALBUMPHOTO PRIMARY KEY (ALBUMID, PHOTOID),
                CONSTRAINT FK_ALBUMPHOTO_ALBUM FOREIGN KEY (ALBUMID) REFERENCES ALBUM (ALBUMID) ON DELETE CASCADE ON UPDATE CASCADE,
                CONSTRAINT FK_ALBUMPHOTO_PHOTO FOREIGN KEY (PHOTOID) REFERENCES PHOTO (PHOTOID) ON DELETE CASCADE ON UPDATE CASCADE )"
            );

            // 4. Photo Tag
            database.ExecuteSqlRaw(@"CREATE TABLE PHOTOTAG (
                PHOTOID NVARCHAR(40) NOT NULL,
                TAG NVARCHAR(50) NOT NULL,
                CONSTRAINT PK_PHOTOTAG PRIMARY KEY (PHOTOID, TAG),
                CONSTRAINT FK_PHOTOTAG_PHOTO FOREIGN KEY (PHOTOID) REFERENCES PHOTO (PHOTOID) ON DELETE CASCADE ON UPDATE CASCADE )"
            );

            // 5. User Detail
            database.ExecuteSqlRaw(@"CREATE TABLE USERDETAIL(
                USERID NVARCHAR(50) PRIMARY KEY,
                DISPLAYAS NVARCHAR(50) NOT NULL,
                UPLOADFILEMINSIZE INT NULL,
                UPLOADFILEMAXSIZE INT NULL,
                ALBUMCREATE BIT NULL,
                ALBUMCHANGE TINYINT NULL,
                ALBUMDELETE TINYINT NULL,
                PHOTOUPLOAD BIT NULL,
                PHOTOCHANGE TINYINT NULL,
                PHOTODELETE TINYINT NULL,
                ALBUMREAD TINYINT NULL
                )"
            );

            // 6. Photo Rating
            database.ExecuteSqlRaw(@"CREATE TABLE PHOTORATING ( 
                PHOTOID NVARHCAR(40) NOT NULL,
                USER NVARCHAR(50) NOT NULL,
                RATING TINYINT NOT NULL,
                CONSTRAINT PK_PHOTORATING PRIMARY KEY (PHOTOID, USER ),
                CONSTRAINT FK_PHOTORATING_PHOTO FOREIGN KEY (PHOTOID) REFERENCES PHOTO (PHOTOID) ON DELETE CASCADE ON UPDATE CASCADE )"
            );
        }

        public static void CreateDatabaseViews(DatabaseFacade database)
        {
            database.ExecuteSqlRaw(@"CREATE VIEW PHOTOVIEW AS 
                select a.*, b.Tags from PHOTO as a
	            left outer join ( select PhotoID, STRING_AGG(Tag, ',') as Tags from PhotoTag group by PhotoID ) as b
	            on a.PhotoID = b.PhotoID "
            );
            database.ExecuteSqlRaw(@"CREATE VIEW ALBUMPHOTOVIEW AS 
               select a.AlbumID, c.*, b.Tags from AlbumPhoto as a
	            left outer join ( select PhotoID, STRING_AGG(Tag, ',') as Tags from PhotoTag group by PhotoID ) as b
		            on a.PhotoID = b.PhotoID
	            inner join Photo as c
		            on a.PhotoID = c.PhotoID"
            );
        }
        #endregion
    }
}
