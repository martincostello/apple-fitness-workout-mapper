// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MartinCostello.AppleFitnessWorkoutMapper
{
    public static class IWebDriverExtensions
    {
        public static IWebElement FindElementWithWait(this IWebDriver driver, By by)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            return wait.Until((p) => p.FindElement(by));
        }

        public static IReadOnlyCollection<IWebElement> FindElementsWithWait(this IWebDriver driver, By by)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until((p) => p.FindElements(by).Count > 0);

            return driver.FindElements(by);
        }

        public static IWebElement WaitForInteractable(this IWebDriver driver, IWebElement element)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until((p) => element.Displayed && element.Enabled);

            return element;
        }
    }
}
