using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace AutomationBase
{

    public static class ChromeDriverHelper 
    {
        public static ChromeDriverBuilder GetDriverBuilder() => new ChromeDriverBuilder();

        public static Dictionary<string, string> GetChromeDriverVersionsAvailableForDownload()
        {
            using (var client = new HttpClient())
            using (var response = client.GetAsync("https://chromedriver.chromium.org/downloads").Result)
            using (var content = response.Content)
            {
                var result = content.ReadAsStringAsync().Result;
                var substring = result.Substring(result.IndexOf("If you are using", StringComparison.Ordinal),
                    result.IndexOf("For older version of Chrome", StringComparison.Ordinal));
                var splitted = substring.Split(new[] {"please download"}, StringSplitOptions.None);

                var links = splitted.Where((t, i) => i != 0)
                                    .Select(t => t.Between(@"a href=", " ").Replace("\"", string.Empty))
                                    .ToList();

                return links.ToDictionary(x => x.Between("path=", "."), x => x?.Replace("index.html?path=", string.Empty) + "chromedriver_win32.zip");
            }
        }

        public static void CheckUpdateChromeDriver()
        {
            var browserVersion = GetChromeBrowserVersion(DevOpsHelper.GetOsPlatform());
            var driverVersion = GetChromeDriverVersion(AppDomain.CurrentDomain.BaseDirectory);

            var availableVersions = GetChromeDriverVersionsAvailableForDownload();
            if (browserVersion == driverVersion) return;
            if (!availableVersions.ContainsKey(browserVersion))
            {
                throw new Exception("Update is not available");
            }

            WebHelper.DownloadFile(availableVersions[browserVersion], Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chromedriver_win32.zip")).Wait();
            CompressionHelper.ExtractZip(AppDomain.CurrentDomain.BaseDirectory, "chromedriver_win32.zip");
        }

        /// <summary>
        /// Gets chrome driver version.
        /// </summary>
        /// <param name="chromeDriverPath">Null means solution base directory</param>
        /// <param name="chromeDriverFileName"></param>
        /// <returns></returns>
        public static string GetChromeDriverVersion(string chromeDriverPath = null, string chromeDriverFileName = "chromedriver.exe")
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(chromeDriverPath ?? AppDomain.CurrentDomain.BaseDirectory, chromeDriverFileName),
                    Arguments = "-v",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();
                return TreatVersionString(line);
            }

            return null;
        }

        private static string TreatVersionString(string versao)
        {
            return versao.Split('.').First().Replace("ChromeDriver", string.Empty).Trim();
        }

        public static string GetChromeBrowserVersion(OSPlatform osPlatform)
        {
            if (osPlatform == OSPlatform.Windows)
            {
                var path = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe", "", null) ??
                           Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe", "", null);

                return path != null ? TreatVersionString(FileVersionInfo.GetVersionInfo(path.ToString()).FileVersion) : null;
            }
            else if (osPlatform == OSPlatform.OSX)
            {
                return string.Empty;
            }
            else if (osPlatform == OSPlatform.Linux)
            {
                return string.Empty;
            }

            return null;
        }

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

        /// <summary>
        /// Find element by multiple classes
        /// </summary>
        /// <param name="expectedElementTag">A div, a, p</param>
        /// <param name="classes">Order does not matter</param>
        /// <returns></returns>
        public static By MultipleClasses(string expectedElementTag = "div", params string[] classes)
        {
            if (classes.Length == 0)
            {
                return null;
            }

            var filter = "and contains(@class, '" + string.Join("') and contains(@class, '", classes) + "')";

            return By.XPath($"//{expectedElementTag}[contains(@class, '{classes.First()}') {filter}]");
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
