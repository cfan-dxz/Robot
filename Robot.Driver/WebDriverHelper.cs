using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Robot.Driver
{
    public class WebDriverHelper
    {
        public static IWebDriver GetWebDriver(WebDriverOption option)
        {
            if (option.Type == 0)
            {
                ChromeOptions chromeOptions = new ChromeOptions();
                chromeOptions.AddArguments("--no-sandbox"); //停用沙箱以在Linux中正常运行
                if (option.NoGui)
                {
                    chromeOptions.AddArguments("--headless"); //开启无gui模式
                }
                if (option.IsProxy)
                {
                    //设置代理ip
                    chromeOptions.Proxy = GetProxy();
                }
                return new ChromeDriver(AppContext.BaseDirectory, chromeOptions, TimeSpan.FromSeconds(option.TimeOut));
            }
            else
            {
                #region Firefox

                var firefoxOptions = new FirefoxOptions();
                if (option.NoGui)
                {
                    firefoxOptions.AddArguments("--headless"); //开启无gui模式
                }
                if (option.IsProxy)
                {
                    //设置代理ip
                    firefoxOptions.Proxy = GetProxy();
                }
                return new FirefoxDriver(AppContext.BaseDirectory, firefoxOptions, TimeSpan.FromSeconds(option.TimeOut));

                #endregion
            }
        }

        //获取代理
        public static Proxy GetProxy()
        {
            Proxy proxy = null;
            int count = 0;
            while (true)
            {
                count++;
                var ipResult = HttpHelper.Get<IpIdea>("http://tiqu.linksocket.com:81/abroad?num=1&type=2&lb=1&sb=0&flow=1&regions=us&n=0", null);
                //var ipResult = HttpHelper.Get<IpIdea>("http://tiqu.linksocket.com:81/abroad?num=1&type=2&lb=1&sb=0&flow=1&regions=china&n=1", null);
                if (ipResult.success)
                {
                    var ipResultItem = ipResult.data.FirstOrDefault();
                    proxy = new Proxy
                    {
                        HttpProxy = $"{ipResultItem.ip}:{ipResultItem.port}",
                        SslProxy = $"{ipResultItem.ip}:{ipResultItem.port}",
                        Kind = ProxyKind.Manual
                    };
                    Console.WriteLine($"设置代理ip：[{proxy.HttpProxy}]，");
                    break;
                }
                if (count >= 5)
                {
                    Console.WriteLine($"尝试5次未成功获取代理ip，");
                    break;
                }
                Thread.Sleep(2000);
            }
            return proxy;
        }

        public static IWebDriver GetRemoteWebDriver()
        {
            var host = "http://192.168.8.73";
            IWebDriver webDriver = new RemoteWebDriver(new Uri($"{host}:4444/wd/hub"), new ChromeOptions());
            return webDriver;
        }
    }

    #region ipidea

    public class IpIdea
    {
        public int code { get; set; }

        public List<IpIdeaItem> data { get; set; }

        public string msg { get; set; }

        public bool success { get; set; }
    }

    public class IpIdeaItem
    {
        public string ip { get; set; }

        public int port { get; set; }
    }

    #endregion

    public class IpResult
    {
        public int code { get; set; }

        public IpIdeaItem data { get; set; }

        public string msg { get; set; }
    }

    public class IpResultItem
    {
        public string ip { get; set; }

        public string port { get; set; }

        public string protocol { get; set; }
    }

    public class WebDriverOption
    {
        public int Type { get; set; }

        public bool NoGui { get; set; }

        public bool IsProxy { get; set; }

        public int TimeOut { get; set; }
    }
}
