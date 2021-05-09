// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;

namespace MartinCostello.AppleFitnessWorkoutMapper.Pages
{
    public sealed class ApplicationPage
    {
        private readonly IWebDriver _driver;

        public ApplicationPage(IWebDriver driver)
        {
            _driver = driver;
        }

        public void ImportData()
        {
            // Start the import
            var import = _driver.FindElementWithWait(By.Id("import"));
            _driver.WaitForInteractable(import).Click();

            // Wait for the import to complete
            var filter = _driver.FindElementWithWait(By.Id("filter"));
            _driver.WaitForInteractable(filter);
        }

        public bool IsMapDisplayed()
            => _driver.FindElement(By.CssSelector("[aria-label='Map']")).Displayed;

        public string TotalDistance()
            => _driver.FindElement(By.CssSelector("[js-data-total-distance]")).Text;

        public IReadOnlyList<TrackItem> Tracks()
        {
            return _driver.FindElementsWithWait(By.ClassName("track-item"))
                .Skip(1)
                .Select((p) => new TrackItem(_driver, p))
                .ToList();
        }

        public void HidePolygon()
        {
            ToggleOption(By.CssSelector("[for='show-polygon'][class~='toggle-on']"));
        }

        public void ShowPolygon()
        {
            ToggleOption(By.CssSelector("[for='show-polygon'][class~='toggle-off']"));
        }

        public void UseKilometers()
        {
            ToggleOption(By.CssSelector("[for='unit-of-distance'][class~='toggle-on']"));
        }

        public void UseMiles()
        {
            ToggleOption(By.CssSelector("[for='unit-of-distance'][class~='toggle-off']"));
        }

        public void WaitForReload()
        {
            _driver.WaitForInteractable(_driver.FindElementWithWait(By.Id("filter")));
        }

        private void ToggleOption(By by)
        {
            var option = _driver.FindElementWithWait(by);
            _driver.WaitForInteractable(option).Click();
        }
    }
}
