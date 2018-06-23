using System;
using Microsoft.AspNetCore.Mvc;

namespace VethorScan.Common.CacheProfiles
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DefaultCacheAttribute : ResponseCacheAttribute
    {
        public DefaultCacheAttribute()
        {
            CacheProfileName = CacheProfilesEnum.Default.ToString();
        }
    }
}