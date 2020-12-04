using System;
using System.ComponentModel;

namespace DoWork.Hangfire.Common
{
    /// <summary>
    /// 作业执行者
    /// </summary>
    public class JobPerform
    {
        private IServiceProvider _serviceProvider { get; }

        public JobPerform(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 执行入口
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="typeName"></param>
        /// <param name="methodName"></param>
        [DisplayName("{0}")]
        public void Execute(string jobName, string typeName, string methodName)
        {
            var type = Type.GetType(typeName);
            if (type != null)
            {
                var instance = _serviceProvider.GetService(type);
                var method = type.GetMethod(methodName);
                method.Invoke(instance, null);
            }
        }
    }
}
