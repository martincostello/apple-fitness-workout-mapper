// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace MartinCostello.AppleFitnessWorkoutMapper.Pages;

public sealed class ApplicationPage
{
    private const string FilterSelector = "id=filter";

    private readonly IPage _page;

    public ApplicationPage(IPage page)
    {
        _page = page;
    }

    public async Task FilterAsync()
    {
        await _page.ClickAsync(FilterSelector);
        await _page.WaitUntilEnabledAsync(FilterSelector);

        await WaitForLoaderToBeHiddenAsync();
    }

    public async Task ImportDataAsync()
    {
        // Start the import
        await _page.ClickAsync("id=import");

        // Wait for the import to complete
        await _page.WaitUntilEnabledAsync(FilterSelector);
        await WaitForLoaderToBeHiddenAsync();
    }

    public async Task<bool> IsMapDisplayedAsync()
        => await _page.IsVisibleAsync("[aria-label='Map']");

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
        => await _page.InnerTextTrimmedAsync("[js-data-emissions]");

    public async Task<string> TotalDistanceAsync()
        => await _page.InnerTextTrimmedAsync("[js-data-total-distance]");

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

        await WaitForLoaderToBeHiddenAsync();
    }

    public async Task UseMilesAsync()
    {
        string selector = "[for='unit-of-distance'][class~='toggle-off']";

        await _page.ClickAsync(selector);
        await _page.WaitUntilEnabledAsync(selector);

        await WaitForLoaderToBeHiddenAsync();
    }

    public async Task<ApplicationPage> EnterDateAsync(string selector, string value)
    {
        await _page.ClearTextAsync(selector);
        await _page.FillAsync(selector, value);
        await _page.Keyboard.PressAsync("Escape");

        return this;
    }

    public async Task WaitForTracksAsync()
        => await _page.WaitUntilVisibleAsync("css=.track-item");

    private async Task WaitForLoaderToBeHiddenAsync()
        => await _page.WaitUntilHiddenAsync("id=tracks-loader");
}
