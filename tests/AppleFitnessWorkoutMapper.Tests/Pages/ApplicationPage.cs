// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace MartinCostello.AppleFitnessWorkoutMapper.Pages;

public sealed class ApplicationPage(IPage page)
{
    private const string FilterSelector = "id=filter";

    public async Task FilterAsync()
    {
        await page.ClickAsync(FilterSelector);
        await page.WaitUntilEnabledAsync(FilterSelector);

        await WaitForLoaderToBeHiddenAsync();
    }

    public async Task ImportDataAsync()
    {
        // Start the import
        await page.ClickAsync("id=import");

        // Wait for the import to complete
        await page.WaitUntilEnabledAsync(FilterSelector);
        await WaitForLoaderToBeHiddenAsync();
    }

    public async Task<bool> IsMapDisplayedAsync()
        => await page.IsVisibleAsync("[aria-label='Map']");

    public async Task<ApplicationPage> NotBeforeAsync(string value)
    {
        await EnterDateAsync("id=not-before", value);
        return this;
    }

    public async Task<ApplicationPage> NotAfterAsync(string value)
    {
        await EnterDateAsync("id=not-after", value);
        return this;
    }

    public async Task<string> EmissionsAsync()
        => await page.InnerTextTrimmedAsync("[data-js-emissions]");

    public async Task<string> TotalDistanceAsync()
        => await page.InnerTextTrimmedAsync("[data-js-total-distance]");

    public async Task<IReadOnlyList<TrackItem>> TracksAsync()
    {
        var children = await page.QuerySelectorAllAsync("css=.track-item");

        return [.. children.Skip(1).Select((p) => new TrackItem(p))];
    }

    public async Task HidePolygonAsync()
    {
        await page.ClickAsync("[for='show-polygon'][class~='toggle-on']");
    }

    public async Task ShowPolygonAsync()
    {
        await page.ClickAsync("[for='show-polygon'][class~='toggle-off']");
    }

    public async Task UseKilometersAsync()
    {
        string selector = "[for='unit-of-distance'][class~='toggle-on']";

        await page.ClickAsync(selector);
        await page.WaitUntilEnabledAsync(selector);

        await WaitForLoaderToBeHiddenAsync();
    }

    public async Task UseMilesAsync()
    {
        string selector = "[for='unit-of-distance'][class~='toggle-off']";

        await page.ClickAsync(selector);
        await page.WaitUntilEnabledAsync(selector);

        await WaitForLoaderToBeHiddenAsync();
    }

    public async Task<ApplicationPage> EnterDateAsync(string selector, string value)
    {
        var locator = page.Locator(selector);

        await locator.ClearAsync();
        await locator.PressSequentiallyAsync(value);
        await locator.PressAsync("Escape");

        return this;
    }

    public async Task WaitForTracksAsync()
        => await page.WaitUntilVisibleAsync("css=.track-item");

    private async Task WaitForLoaderToBeHiddenAsync()
        => await page.WaitUntilHiddenAsync("id=tracks-loader");
}
