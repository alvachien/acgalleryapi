-- Photo View
CREATE VIEW [dbo].[PhotoView] AS ( select a.*, b.Tags
	from Photo as a
	left outer join ( select PhotoID, STRING_AGG(Tag, ',') as Tags from PhotoTag group by PhotoID ) as b
	on a.PhotoID = b.PhotoID
		where a.IsPublic = 1);

-- Album Photo View
CREATE VIEW AlbumPhotoView AS (
	select a.AlbumID, c.*, b.Tags from AlbumPhoto as a
	left outer join ( select PhotoID, STRING_AGG(Tag, ',') as Tags from PhotoTag group by PhotoID ) as b
		on a.PhotoID = b.PhotoID
	inner join Photo as c
		on a.PhotoID = c.PhotoID );
