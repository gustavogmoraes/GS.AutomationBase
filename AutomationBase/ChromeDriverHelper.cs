using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace AutomationBase
{
    public static void Teste()
    {
        var driver = ChromeDriverHelper.GetDriverBuilder()
                                       .AllowRunningInsecureContent()
                                       .Build();
        //driver.Navigate()
    }

    public static class ChromeDriverHelper 
    {
        public static ChromeDriverBuilder GetDriverBuilder() => new ChromeDriverBuilder();

        public static void WaitForPageToLoad(this ChromeDriver chromeDriver, TimeSpan? timeout = null)
        {
            var js = (IJavaScriptExecutor)chromeDriver;
            var wait = new WebDriverWait(chromeDriver, timeout ?? TimeSpan.FromSeconds(120));

            wait.Until(wd => js.ExecuteScript("return document.readyState").ToString() == "complete");  
        }

        public static void WaitForCondition(this ChromeDriver chromeDriver, Func<IWebDriver, bool> condition, TimeSpan? timeout = null)
        {
            var wait = new WebDriverWait(chromeDriver, timeout ?? TimeSpan.FromSeconds(120));

            wait.Until(condition);
        }

        public static void WaitForElementToBeDisplayed(this ChromeDriver chromeDriver, By by, TimeSpan? timeout = null)
        {
            var wait = new WebDriverWait(chromeDriver, timeout ?? TimeSpan.FromSeconds(120));

            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.VisibilityOfAllElementsLocatedBy(by));
        }

        public static void WaitSpecificTime(TimeSpan time)
        {
            Thread.Sleep(time);
        }

        public static void EnableHeadlessDownloadOnCurrentPage(this ChromeDriver chromeDriver, string downloadPath = null)
        {
            var parametros = new Dictionary<string, object>
            {
                { "behavior", "allow" },
                { "downloadPath", downloadPath ?? AppDomain.CurrentDomain.BaseDirectory }
            };

            chromeDriver.ExecuteChromeCommand("Page.setDownloadBehavior", parametros);
        }

        public static void ScrollToElement(this ChromeDriver chromedriver, IWebElement element)
        {
            _ = ((IJavaScriptExecutor)chromedriver).ExecuteScript($"window.scroll({element.Location.X}, {element.Location.Y})");
        }

        public static By MultipleClasses(string expectedElementTag = null, params string[] classes)
        {
            if (classes.Length == 0)
            {
                return null;
            }

            //string.Join()
            return By.XPath($"//{expectedElementTag ?? "div"}[contains(@class, '{classes.First()}') and contains(.//span, 'someText')]");
        }
    }

    public class ChromeDriverBuilder
    {
        private string ChromeDriverPath { get; set; }

        private ChromeOptions _options { get; set; }
        private ChromeOptions Options => _options ?? (_options = new ChromeOptions());

        private ChromeDriverService _service { get; set; }
        private ChromeDriverService Service =>
            _service ?? (_service = ChromeDriverService.CreateDefaultService(ChromeDriverPath ?? AppDomain.CurrentDomain.BaseDirectory));

        public ChromeDriverBuilder Headless()
        {
            Options.AddArguments("headless", "disable-gpu", "no-sandbox", "disable-extensions"); // Headless
            Options.AddArguments("--proxy-server='direct://'", "--proxy-bypass-list=*"); // Speed

            Service.HideCommandPromptWindow = true;

            return this;
        }

        public ChromeDriverBuilder DisablePopupBlocking()
        {
            Options.AddUserProfilePreference("disable-popup-blocking", "true");

            return this;
        }

        public ChromeDriverBuilder AllowRunningInsecureContent()
        {
            Options.AddArguments("allow-running-insecure-content", "ignore-certificate-errors");

            return this;
        }

        public ChromeDriverBuilder SetDownloadPath(string downloadFilepath)
        {
            Options.AddUserProfilePreference("download.default_directory", downloadFilepath);
            Options.AddUserProfilePreference("download.prompt_for_download", false);
            Options.AddUserProfilePreference("intl.accept_languages", "nl");

            DisablePopupBlocking();

            return this;
        }

        public ChromeDriverBuilder WithOptions(ChromeOptions options)
        {
            _options = options;

            return this;
        }

        public ChromeDriver Build()
        {
            if(!Options.Arguments.Contains("headless"))
            {
                Options.AddArgument("start-maximized");
            }

            return new ChromeDriver(Service, Options, TimeSpan.FromSeconds(180));
        }
    }
}
