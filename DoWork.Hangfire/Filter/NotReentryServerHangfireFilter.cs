using Hangfire;
using Hangfire.Server;
using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace DoWork.Hangfire.Filter
{
    /// <summary>
    /// 每个job都不可重入过滤
    /// </summary>
    public class NotReentryServerHangfireFilter : IServerFilter
    {
        //执行中的job
        private static ConcurrentDictionary<string, DateTime> _jobPerforming = new ConcurrentDictionary<string, DateTime>();

        //jobid
        private string buildJobId(BackgroundJob job)
        {
            var str = $"{job.Job.Type.FullName}.{job.Job.Method.Name}.{string.Join('.', job.Job.Args)}";
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(str));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        /// <summary>
        /// 执行开始
        /// </summary>
        /// <param name="filterContext"></param>
        public void OnPerforming(PerformingContext filterContext)
        {
            var jobId = buildJobId(filterContext.BackgroundJob);
            if (_jobPerforming.ContainsKey(jobId))
            {
                if (_jobPerforming.TryGetValue(jobId, out DateTime startTime))
                {
                    if (startTime.AddHours(2) > DateTime.Now)
                    {
                        //存在正在执行且小于2小时则取消
                        filterContext.Canceled = true;
                        return;
                    }
                    else
                    {
                        _jobPerforming.TryRemove(jobId, out DateTime value);
                    }
                }
            }
            _jobPerforming.TryAdd(jobId, DateTime.Now);
        }

        /// <summary>
        /// 执行结束
        /// </summary>
        /// <param name="filterContext"></param>
        public void OnPerformed(PerformedContext filterContext)
        {
            var jobId = buildJobId(filterContext.BackgroundJob);
            if (_jobPerforming.ContainsKey(jobId))
            {
                _jobPerforming.TryRemove(jobId, out DateTime value);
            }
        }
    }
}
