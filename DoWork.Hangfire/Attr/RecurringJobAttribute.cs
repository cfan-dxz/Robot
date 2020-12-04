using System;

namespace DoWork.Hangfire.Attr
{
    /// <summary>
    /// 定时作业
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RecurringJobAttribute : Attribute
    {
        public RecurringJobAttribute() { }

        public RecurringJobAttribute(string queue)
        {
            Queue = queue;
        }

        private string _queue;
        /// <summary>
        /// 队列
        /// </summary>
        public string Queue { get => _queue ?? "default"; private set => _queue = value; }
    }
}
