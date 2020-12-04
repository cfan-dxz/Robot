using OpenQA.Selenium;
using System;
using System.Linq;
using System.Threading;

namespace Robot.Driver
{
    public class AmzAutoTest
    {
        //关键字
        private string keyword { get; set; }
        //asin
        private string asin { get; set; }

        public void TimingJob()
        {
            //input
            while (true)
            {
                Console.Write($"输入关键字：");
                keyword = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(keyword)) break;
            }
            while (true)
            {
                Console.Write($"输入ASIN：");
                asin = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(asin)) break;
            }
            //是否使用代理
            Console.Write($"是否使用代理(1:使用,0:不使用)：");
            bool isProxy = Console.ReadLine() == "1";
            //起始页
            Console.Write($"起始页：");
            int.TryParse(Console.ReadLine(), out int inputPage);
            //默认执行1次
            int maxCount = 1;
            Console.Write($"输入执行次数：");
            var maxInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(maxInput) && int.TryParse(maxInput, out int maxRes))
            {
                maxCount = maxRes;
            }

            //循环执行
            int count = 0;
            while (count < maxCount)
            {
                try
                {
                    Do(isProxy, inputPage);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"执行出错：{e.Message}");
                }
                count++;
                Console.WriteLine($"共执行{count}次");
            }
        }

        public void Do(bool isProxy, int beginPage)
        {
            //关键字：smart coffee warmer for desk，smart coffee warmer
            //var keyword = "smart coffee warmer for desk";
            ////指定asin
            //var asin = "B07DJS1LHN"; //B07YJPDCM8

            //初始化webdriver
            var webDriver = WebDriverHelper.GetWebDriver(new WebDriverOption
            {
                Type = 0, //0:谷歌,1:火狐
                NoGui = false, //无界面
                IsProxy = isProxy, //是否代理
                TimeOut = 180 //超时时间s
            });

            try
            {
                //ip查询
                webDriver.Navigate().GoToUrl("http://icanhazip.com/");
                var ip = webDriver.FindElements(By.CssSelector("pre"))?.FirstOrDefault();
                Console.WriteLine($"当前ip：{ip?.Text}；");

                int page = beginPage; //起始页
                var maxPage = 20; //最大页数
                bool isSuccess = false;
                Action action = () =>
                {
                    //打开链接
                    page++;
                    var link = $"https://www.amazon.com/s?k={keyword.Replace(" ", "+")}&page={page}";
                    webDriver.Navigate().GoToUrl(link);
                    //等待
                    //Thread.Sleep(2000);
                    Console.WriteLine($"打开搜索关键字-第{page}页：{link}；");
                    var resultList = webDriver.FindElements(By.CssSelector($"div[data-component-type='s-search-result'][data-asin='{asin}']"));
                    Console.WriteLine($"找到ASIN[{asin}]：{resultList.Count}条结果；");
                    foreach (var item in resultList)
                    {
                        var html = item.Text ?? string.Empty;
                        if (html.Contains("Sponsored") || html.Contains("推广")) //广告
                        {
                            Console.WriteLine($"找到ASIN[{asin}]的广告链接，尝试点击...");
                            //点击链接
                            var arr = item.FindElements(By.CssSelector("a.a-link-normal"));
                            foreach (var a in arr)
                            {
                                var ad = a.FindElements(By.CssSelector("div.a-section"));
                                isSuccess = ad.Count > 0 ? elemClick(ad.First()) : elemClick(a);
                                if (isSuccess)
                                {
                                    executeScript(webDriver);
                                    break;
                                }
                            }
                            if (!isSuccess) throw new Exception($"当次执行无效。");
                            break;
                        }
                    }
                };

                //搜索结果
                while (!isSuccess)
                {
                    action();
                    if (page >= maxPage)
                    {
                        throw new Exception($"执行到第{page}页数未找到对应的链接。");
                    }
                }

                //停留5秒
                Console.WriteLine("停留10秒；");
                Thread.Sleep(10000);
                Console.WriteLine("当次执行完成。");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                webDriver.Quit();
            }

            //WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
            //var elem = wait.Until<IWebElement>((wr) =>
            //{
            //    var a = wr.FindElement(By.XPath("//*[@id=\"search\"]/div[1]/div[2]/div/span[4]/div[2]/div[3]/div/span/div/div/div/div/span/a"));
            //    a.Click();
            //    return a;
            //});
            //elem?.Click();
        }

        //元素点击
        public bool elemClick(IWebElement elem)
        {
            try
            {
                elem?.Click();
                Console.WriteLine("点击成功！");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"点击失败：{e.Message}；");
                return false;
            }
        }

        //执行脚本
        public bool executeScript(IWebDriver webDriver)
        {
            try
            {
                IJavaScriptExecutor executor = (IJavaScriptExecutor)webDriver;
                executor.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"滚动失败：{e.Message}；");
                return false;
            }
        }
    }
}
