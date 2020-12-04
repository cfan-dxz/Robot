using System;

namespace DoWork.Hangfire.Attr
{
    /// <summary>
    /// cron表达式
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CronAttribute : Attribute
    {
        public CronAttribute(string cron)
        {
            Cron = cron;
        }

        public string Cron { get; private set; }
    }
}
