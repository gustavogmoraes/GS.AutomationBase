using System;
using OpenQA.Selenium;

namespace AutomationBase.BetterSelenium.Objects
{
    public class NeverStaleWebElement : WebElement
    {
        private readonly WebDriver _driver;
        
        private IWebElement _element;

        private readonly By _foundBy;
        
        public NeverStaleWebElement(WebDriver parentDriver, string id, IWebElement element, By foundBy) 
            : base(parentDriver, id)
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
            
            base.Click();
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
            
            base.Clear();
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
            
            base.SendKeys(text);
        }
    }
}