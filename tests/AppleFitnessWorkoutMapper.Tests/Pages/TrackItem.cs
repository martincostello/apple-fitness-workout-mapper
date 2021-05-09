// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using OpenQA.Selenium;

namespace MartinCostello.AppleFitnessWorkoutMapper.Pages
{
    public sealed class TrackItem
    {
        private readonly IWebDriver _driver;
        private readonly IWebElement _element;

        public TrackItem(IWebDriver driver, IWebElement element)
        {
            _driver = driver;
            _element = element;
        }

        public void Expand()
        {
            _element.Click();
            _driver.WaitForInteractable(_element.FindElement(By.CssSelector("[data-js-visible]")));
        }

        public void Collapse()
        {
            _element.Click();
        }

        public string LinkText()
            => _element.FindElement(By.TagName("a")).Text;

        public string Name()
            => _element.FindElement(By.CssSelector("[data-track-name]")).GetAttribute("data-track-name");

        public string StartedAt()
            => _element.FindElement(By.CssSelector("[data-js-start]")).Text;

        public string EndedAt()
            => _element.FindElement(By.CssSelector("[data-js-end]")).Text;

        public string Duration()
            => _element.FindElement(By.CssSelector("[data-js-duration]")).Text;

        public string Distance()
            => _element.FindElement(By.CssSelector("[data-js-distance]")).Text;

        public string AveragePace()
            => _element.FindElement(By.CssSelector("[data-js-pace]")).Text;
    }
}
