﻿using System.Linq;
using Microsoft.AspNetCore.Mvc;
using GalleryAPI.Models;

namespace GalleryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly GalleryContext _context;

        public StatisticsController(GalleryContext context)
        {
            _context = context;
        }

        // GET: api/Statistics
        [HttpGet]
        [ResponseCache(Duration = 1200)]
        public IActionResult Get()
        {
            var vmResult = new StatisticsInfo();
            vmResult.AlbumAmount = _context.Albums.Count();
            vmResult.PhotoAmount = _context.Photos.Count();

            // Top 5 albums
            var rst = (from alm in _context.AlbumPhotos
                      group alm by alm.AlbumID into almpts                      
                      select new
                      {
                          AlbumID = almpts.Key,
                          PhotoCount = almpts.Count()
                      } 
                      into almptcnts
                      orderby almptcnts.PhotoCount descending
                      select almptcnts).Take(5);
            foreach(var relem in rst)
            {
                vmResult.PhotoAmountInTop5Album.Add(relem.AlbumID, relem.PhotoCount);
            }

            // Top 5 tags
            var rst2 = (from tag in _context.PhotoTags
                        group tag by tag.TagString into tagcnts
                        select new
                        {
                            TagString = tagcnts.Key,
                            PhotoCount = tagcnts.Count()
                        }
                        into tagstrcnts
                        orderby tagstrcnts.PhotoCount descending
                        select tagstrcnts).Take(5);
            foreach(var relem2 in rst2)
            {
                vmResult.PhotoAmountInTop5Tag.Add(relem2.TagString, relem2.PhotoCount);
            }

            return new ObjectResult(vmResult);
        }
    }
}
