using System;
using OpenQA.Selenium;

namespace AutomationBase.BetterSelenium.Objects
{
    public class NeverStaleWebElement : WebElement
    {
        private readonly IWebDriver _driver;
        
        private IWebElement _element;

        private readonly By _foundBy;
        
        public NeverStaleWebElement(IWebDriver parentDriver, string id, IWebElement element, By foundBy) 
            : base((WebDriver)parentDriver, id)
        {
            _element = element;
            _driver = parentDriver;
            _foundBy = foundBy;
        }

        public override void Click()
        {
            try
            {
                _element.Click();
            }
            catch (StaleElementReferenceException)
            {
                _element = _driver.FindElement(_foundBy);
            }
        }

        public override void Clear()
        {
            try
            {
                _element.Clear();
            }
            catch (StaleElementReferenceException)
            {
                _element = _driver.FindElement(_foundBy);
            }
        }

        public override void SendKeys(string text)
        {
            try
            {
                _element.SendKeys(text);
            }
            catch (StaleElementReferenceException)
            {
                _element = _driver.FindElement(_foundBy);
            }
        }
    }
}