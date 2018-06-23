using System;
using Microsoft.AspNetCore.Mvc;

namespace VethorScan.Common.CacheProfiles
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class NeverCacheAttribute : ResponseCacheAttribute
    {
        public NeverCacheAttribute()
        {
            CacheProfileName = CacheProfilesEnum.Never.ToString();
        }
    }
}