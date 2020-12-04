using Hangfire.Dashboard;
using System.Globalization;

namespace DoWork.Hangfire.Filter
{
    /// <summary>
    /// 监控页面验证过滤
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            //界面设置成中文
            CultureInfo.CurrentUICulture = new CultureInfo("zh-CN");
            //登录验证
            return true;
        }
    }
}
