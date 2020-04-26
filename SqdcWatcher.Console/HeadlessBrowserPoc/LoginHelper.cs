#region

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

#endregion

namespace SqdcWatcher.HeadlessBrowserPoc
{
    public class LoginHelper
    {
        public void Login()
        {
            var options = new FirefoxOptions();
            options.AddArguments("P", "headless-sqdc");
            using (var driver = new FirefoxDriver(GetAppDirectory(), options))
            {
                driver.Navigate().GoToUrl("https://tangerine.ca");
                new WebDriverWait(driver, TimeSpan.FromHours(1));
                Thread.Sleep(10000000);
            }
        }

        private static string GetAppDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}