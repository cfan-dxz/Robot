using DoWork.Hangfire.Utils;
using Hangfire;
using System;

namespace DoWork.Hangfire
{
    /// <summary>
    /// 作业基类
    /// </summary>
    public abstract class JobBase
    {
        //当前时区
        private static TimeZoneInfo currentTimeZone = TimeZoneHelper.GetCurrentTimeZone();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Init<T>() where T : JobBase
        {
            //设置了Cron才执行
            if (!string.IsNullOrWhiteSpace(CronExpression))
            {
                RecurringJob.AddOrUpdate<T>(o => o.Execute(), CronExpression, currentTimeZone);
            }
        }

        /// <summary>
        /// Cron表达式
        /// </summary>
        protected virtual string CronExpression
        {
            get;
        }

        /// <summary>
        /// 执行方法
        /// </summary>
        public virtual void Execute()
        {
        }
    }
}
