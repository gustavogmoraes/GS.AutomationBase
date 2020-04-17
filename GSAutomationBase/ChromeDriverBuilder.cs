using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using OpenQA.Selenium.Chrome;

namespace GSAutomationBase
{
    public class ChromeDriverBuilder
    {
        private string ChromeDriverPath { get; set; }

        private ChromeOptions _options { get; set; }
        private ChromeOptions Options => _options ??= new ChromeOptions();

        private ChromeDriverService _service { get; set; }
        private ChromeDriverService Service =>
            _service ??= ChromeDriverService.CreateDefaultService(ChromeDriverPath ?? AppDomain.CurrentDomain.BaseDirectory);

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

        public ChromeDriverBuilder WithLanguage(ChromeDriverLanguage language)
        {
            switch (language)
            {
                case ChromeDriverLanguage.English:
                    Options.AddArgument("--lang=en");
                    break;
                case ChromeDriverLanguage.Portuguese:
                    Options.AddArgument("--lang=pt");
                    break;
            }

            return this;
        }

        public ChromeDriverBuilder SetDownloadPath(string downloadFilepath)
        {
            Options.AddUserProfilePreference("download.default_directory", downloadFilepath);
            Options.AddUserProfilePreference("download.prompt_for_download", false);
            //Options.AddUserProfilePreference("intl.accept_languages", "nl");

            DisablePopupBlocking();

            return this;
        }

        public ChromeDriverBuilder WithOptions(ChromeOptions options)
        {
            _options = options;

            return this;
        }

        public ChromeDriverBuilder OutputService(out ChromeDriverService service)
        {
            service = Service;

            return this;
        }

        public ChromeDriver Build(bool killAnotherChromeDriverProcesses = true)
        {
            if (!Options.Arguments.Contains("headless"))
            {
                Options.AddArgument("start-maximized");
            }

            if (killAnotherChromeDriverProcesses)
            {
                Process.GetProcessesByName("chromedriver.exe").ToList().ForEach(x => x.Kill());
            }

            return new ChromeDriver(Service, Options, TimeSpan.FromSeconds(180));
        }
    }

    public enum ChromeDriverLanguage
    {
        English,

        Portuguese
    }
}
