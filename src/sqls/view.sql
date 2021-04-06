-- Photo View
CREATE VIEW [dbo].[PhotoView] AS ( select a.*, b.Tags
	from Photo as a
	left outer join ( select PhotoID, STRING_AGG(Tag, ',') as Tags from PhotoTag group by PhotoID ) as b
	on a.PhotoID = b.PhotoID );
