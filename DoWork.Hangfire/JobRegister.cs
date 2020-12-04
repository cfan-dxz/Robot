using DoWork.Hangfire.Attr;
using DoWork.Hangfire.Common;
using DoWork.Hangfire.Filter;
using DoWork.Hangfire.Utils;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using Hangfire.MySql.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace DoWork.Hangfire
{
    public static class JobRegister
    {
        /// <summary>
        /// 获取所有job
        /// </summary>
        /// <returns></returns>
        private static List<Type> getJobList()
        {
            var jobList = new List<Type>();
            var libs = DependencyContext.Default.CompileLibraries.Where(lib => !lib.Serviceable && lib.Type == "project");
            foreach (var lib in libs)
            {
                var assembly = Assembly.Load(lib.Name);
                if (assembly != null)
                {
                    var typeList = assembly.GetTypes().Where(x => x.BaseType == typeof(JobBase) || x.CustomAttributes.Any(a => a.AttributeType == typeof(RecurringJobAttribute)));
                    if (typeList.Any())
                    {
                        jobList.AddRange(typeList);
                    }
                }
            }
            return jobList;
        }

        /// <summary>
        /// 注册所有job
        /// </summary>
        /// <param name="services"></param>
        public static void AddHangfireJob(this IServiceCollection services)
        {
            services.AddHangfire((serviceProvider, config) =>
            {
                var appConfig = serviceProvider.GetService<IConfiguration>().GetSection("Hangfire");
                var storageType = (appConfig["StorageType"] ?? "memory").ToLower(); //数据库类型:默认memory
                var storage = appConfig["Storage"]; //数据库
                if ("mssql".Equals(storageType))
                {
                    config.UseSqlServerStorage(storage); //mssql
                }
                else if ("mysql".Equals(storageType))
                {
                    config.UseStorage(new MySqlStorage(storage)); //mysql
                }
                else
                {
                    config.UseMemoryStorage(); //memory
                }
                config.UseFilter(new NotReentryServerHangfireFilter());
            });
            //JobList
            getJobList()?.ForEach(type => services.AddScoped(type));
            //JobPerform
            services.AddScoped<JobPerform>();
        }

        /// <summary>
        /// 初始化job
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="authFilter"></param>
        public static void UseHangfireJob(this IApplicationBuilder builder, IDashboardAuthorizationFilter authFilter)
        {
            var serviceProvider = builder.ApplicationServices;
            var appConfig = serviceProvider.GetService<IConfiguration>().GetSection("Hangfire");
            var isEnable = appConfig.GetValue<bool>("IsEnable"); //是否开启任务
            if (!isEnable)
            {
                return;
            }

            //找到所有使用的队列
            var jobList = getJobList();
            var queueList = new List<string>() { "default" };
            Action<string> logQueue = (queue) =>
            {
                if (!queueList.Any(q => q == queue))
                {
                    queueList.Add(queue);
                }
            };
            foreach (Type type in jobList)
            {
                if (type.BaseType == typeof(JobBase))
                {
                    var execute = type.GetMethod("Execute");
                    var execQueue = execute.GetCustomAttribute<QueueAttribute>();
                    if (execQueue != null) logQueue(execQueue.Queue);
                }
                else
                {
                    var jobAttr = type.GetCustomAttribute<RecurringJobAttribute>();
                    if (jobAttr != null) logQueue(jobAttr.Queue);
                }
            }

            //设置HangfireServer
            builder.UseHangfireServer(new BackgroundJobServerOptions
            {
                WorkerCount = Environment.ProcessorCount * 5, //并发数量
                ServerName = appConfig["ServerName"], //服务器名称
                Queues = queueList.ToArray() //队列
            });
            builder.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { authFilter ?? new HangfireAuthorizationFilter() },
                DisplayStorageConnectionString = false,
                DashboardTitle = "任务管理"
            });

            //执行init
            foreach (Type type in jobList)
            {
                if (type.BaseType == typeof(JobBase))
                {
                    var instance = serviceProvider.GetService(type);
                    var init = type.GetMethod("Init").MakeGenericMethod(type);
                    init.Invoke(instance, null);
                }
                else
                {
                    var jobAttr = type.GetCustomAttribute<RecurringJobAttribute>();
                    if (jobAttr == null) continue;
                    var timeZone = TimeZoneHelper.GetCurrentTimeZone(); //时区
                    var methods = type.GetMethods().Where(m => m.CustomAttributes.Any(a => a.AttributeType == typeof(CronAttribute)));
                    foreach (var method in methods)
                    {
                        var cron = method.GetCustomAttribute<CronAttribute>().Cron; //cron表达式
                        var jobId = $"{type.ToGenericTypeString()}.{method.Name}"; //jobid
                        //作业名称
                        var nameAttr = method.GetCustomAttribute<DisplayNameAttribute>();
                        var jobName = nameAttr != null ? nameAttr.DisplayName : jobId;
                        RecurringJob.AddOrUpdate<JobPerform>(jobId, p => p.Execute(jobName, type.AssemblyQualifiedName, method.Name), cron, timeZone, jobAttr.Queue);
                    }
                }
            }
        }

        /// <summary>
        /// 初始化job
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="serviceProvider"></param>
        public static void UseHangfireJob(this IApplicationBuilder builder)
        {
            UseHangfireJob(builder, null);
        }
    }
}
