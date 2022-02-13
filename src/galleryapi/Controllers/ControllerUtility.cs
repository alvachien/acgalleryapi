using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GalleryAPI.Controllers
{
    internal class ControllerUtility
    {
        internal static String GetUserID(ControllerBase ctrl)
        {
            if (ctrl.User != null)
                return ctrl.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return String.Empty;
        }

        internal static String GetUserID(IHttpContextAccessor _httpContextAccessor)
        {
            if (_httpContextAccessor == null
                || _httpContextAccessor.HttpContext == null
                || _httpContextAccessor.HttpContext.User == null)
                return null;

            return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
