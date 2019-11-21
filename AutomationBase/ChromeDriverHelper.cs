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
                var splitted = substring.Split(new[] { "please download" }, StringSplitOptions.None);

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

            if (!string.IsNullOrEmpty(driverVersion))
            {
                if (browserVersion == driverVersion) return;

                if (!availableVersions.ContainsKey(browserVersion))
                {
                    throw new Exception("Update is not available");
                }
            }

            var zipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chromedriver_win32.zip");
            WebHelper.DownloadFile(availableVersions[browserVersion], zipPath).Wait();

            CompressionHelper.ExtractZip(AppDomain.CurrentDomain.BaseDirectory, "chromedriver_win32.zip");
            File.Delete(zipPath);
        }

        /// <summary>
        /// Gets chrome driver version.
        /// </summary>
        /// <param name="chromeDriverPath">Null means solution base directory</param>
        /// <param name="chromeDriverFileName"></param>
        /// <returns></returns>
        public static string GetChromeDriverVersion(string chromeDriverPath = null, string chromeDriverFileName = "chromedriver.exe")
        {
            if (!File.Exists(Path.Combine(chromeDriverPath, chromeDriverFileName)))
            {
                return string.Empty;
            }

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
            if (string.IsNullOrEmpty(versao))
            {
                return null;
            }

            return versao.Split('.')
                         .First()
                         .Replace("ChromeDriver", string.Empty)
                         .Replace("Version=", string.Empty)
                         .Replace("Google Chrome ", string.Empty)
                         .Trim();
        }

        public static string TryGetChromePathOnWindows()
        {
            var chromeAppend = @"Google\Chrome\Application\";

            if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)) &&
                Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Google\")) &&
                Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Google\Chrome\")))
            {
                var programPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), chromeAppend);

                var program = Directory.GetFiles(programPath);
                if (program.Contains("chrome.exe"))
                {
                    return programPath;
                }
            }


            var programX86Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), chromeAppend);

            var programX86 = Directory.GetFiles(programX86Path);
            return programX86.Any(x => x.Contains("chrome.exe")) ? programX86Path : string.Empty;
        }

        public static string GetChromeBrowserVersion(OSPlatform osPlatform)
        {
            if (osPlatform == OSPlatform.Windows)
            {
                var chromePath = Path.Combine(TryGetChromePathOnWindows(), "chrome.exe");
                return TreatVersionString(FileVersionInfo.GetVersionInfo(chromePath).FileVersion);
            }
            else if (osPlatform == OSPlatform.OSX)
            {
                var version = ShellHelper.Bash(@"/Applications/Google\ Chrome.app/Contents/MacOS/Google\ Chrome --version");
                return TreatVersionString(version);
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
}
