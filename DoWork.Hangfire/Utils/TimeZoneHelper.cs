using System;
using System.Runtime.InteropServices;

namespace DoWork.Hangfire.Utils
{
    public class TimeZoneHelper
    {
        /// <summary>
        /// 获取当前时区
        /// </summary>
        /// <returns></returns>
        public static TimeZoneInfo GetCurrentTimeZone()
        {
            string timeZoneId;
            //windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                timeZoneId = "China Standard Time";
            }
            else //linux
            {
                timeZoneId = "Asia/Shanghai";
            }
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
    }
}
