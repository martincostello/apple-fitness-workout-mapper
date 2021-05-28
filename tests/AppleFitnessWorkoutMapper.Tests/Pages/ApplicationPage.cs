// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace MartinCostello.AppleFitnessWorkoutMapper.Pages
{
    public sealed class ApplicationPage
    {
        private const string FilterSelector = "id=filter";
        private const string LoaderSelector = "id=tracks-loader";

        private readonly IPage _page;

        public ApplicationPage(IPage page)
        {
            _page = page;
        }

        public async Task FilterAsync()
        {
            await _page.ClickAsync(FilterSelector);
            await _page.WaitUntilEnabledAsync(FilterSelector);
            await _page.WaitUntilHiddenAsync(LoaderSelector);
        }

        public async Task ImportDataAsync()
        {
            // Start the import
            await _page.ClickAsync("id=import");

            // Wait for the import to complete
            await _page.WaitUntilEnabledAsync(FilterSelector);
            await _page.WaitUntilHiddenAsync(LoaderSelector);
        }

        public async Task<bool> IsMapDisplayedAsync()
            => await _page.IsVisibleAsync("[aria-label='Map']");

        public async Task<ApplicationPage> NotBeforeAsync(string value)
        {
            await _page.EnterTextAsync("id=not-before", value);
            await _page.Keyboard.PressAsync("Escape");

            return this;
        }

        public async Task<ApplicationPage> NotAfterAsync(string value)
        {
            await _page.EnterTextAsync("id=not-after", value);
            await _page.Keyboard.PressAsync("Escape");

            return this;
        }

        public async Task<string> EmissionsAsync()
            => (await _page.InnerTextAsync("[js-data-emissions]")).Trim();

        public async Task<string> TotalDistanceAsync()
            => (await _page.InnerTextAsync("[js-data-total-distance]")).Trim();

        public async Task<IReadOnlyList<TrackItem>> TracksAsync()
        {
            IReadOnlyList<IElementHandle> children = await _page.QuerySelectorAllAsync("css=.track-item");

            return children
                .Skip(1)
                .Select((p) => new TrackItem(p))
                .ToList();
        }

        public async Task HidePolygonAsync()
        {
            await _page.ClickAsync("[for='show-polygon'][class~='toggle-on']");
        }

        public async Task ShowPolygonAsync()
        {
            await _page.ClickAsync("[for='show-polygon'][class~='toggle-off']");
        }

        public async Task UseKilometersAsync()
        {
            string selector = "[for='unit-of-distance'][class~='toggle-on']";

            await _page.ClickAsync(selector);
            await _page.WaitUntilEnabledAsync(selector);
        }

        public async Task UseMilesAsync()
        {
            string selector = "[for='unit-of-distance'][class~='toggle-off']";

            await _page.ClickAsync(selector);
            await _page.WaitUntilEnabledAsync(selector);
        }
    }
}
