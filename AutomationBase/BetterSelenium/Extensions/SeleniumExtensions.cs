using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using AutomationBase.BetterSelenium.Objects;
using AutomationBase.General.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace AutomationBase.BetterSelenium.Extensions
{
    public static class SeleniumExtensions
    {
        public static NeverStaleWebElement FindNeverStaleElement(this IWebDriver webDriver, By by)
        {
            var element = webDriver.FindElement(by);
            var fieldName = "elementId";
            
            var idField = typeof(WebElement).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            var id = (string) idField?.GetValue(element);

            var nsElement = new NeverStaleWebElement(webDriver, id, element, by);

            return nsElement;
        }
        
        public static void SelectByText(this NeverStaleWebElement element, string text)
        {
            var selectElement = new SelectElement(element);
            selectElement.SelectByText(text);
        }

        public static void SendKeysWithDelay(this NeverStaleWebElement element, string text, int? delayBetweenKeysInMs = null)
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