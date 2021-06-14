using System;
using System.Linq;
using System.Threading;
using AutomationBase.BetterSelenium.Objects;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AutomationBase.BetterSelenium.Extensions
{
    public static class SeleniumExtensions
    {
        public static IWebElement FindElement(this IWebElement webElement, By by)
        {
            return (NeverStaleWebElement)webElement.FindElement(by);
        }
        
        public static void SelectByText(this IWebElement element, string text)
        {
            var selectElement = new SelectElement(element);
            selectElement.SelectByText(text);
        }

        public static void SendKeysWithDelay(this IWebElement element, string text, int? delayBetweenKeysInMs = null)
        {
            var random = new Random();
            
            text.ToList().ForEach(c =>
            {
                element.SendKeys(c.ToString());

                Thread.Sleep(delayBetweenKeysInMs ?? random.Next(0, 80));
            });
        }
    }
}