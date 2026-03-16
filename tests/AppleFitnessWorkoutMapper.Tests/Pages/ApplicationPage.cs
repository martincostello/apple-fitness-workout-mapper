// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace MartinCostello.AppleFitnessWorkoutMapper.Pages;

public sealed class ApplicationPage(IPage page)
{
    public async Task FilterAsync()
    {
        string selector = Selectors.Filter;

        await page.ClickAsync(selector);
        await page.WaitUntilEnabledAsync(selector);

        await WaitForLoaderToBeHiddenAsync();
    }

    public async Task ImportDataAsync()
    {
        // Start the import
        await page.ClickAsync(Selectors.Import);

        // Wait for the import to complete
        await page.WaitUntilEnabledAsync(Selectors.Filter);
        await WaitForLoaderToBeHiddenAsync();
    }

    public async Task<bool> IsMapDisplayedAsync()
        => await page.IsVisibleAsync(Selectors.Map);

    public async Task<string> RouteInfoWindowAsync()
    {
        // Wait for Google Maps to render route polylines as SVG paths in the DOM.
        // Routes are rendered as SVG path elements with no fill and a colored stroke.
        await page.WaitForFunctionAsync(@"() => document.querySelectorAll('path[fill=""none""]').length > 0");

        // Find the center of the first rendered route SVG path using its bounding rect.
        // Using the actual rendered position avoids browser-specific coordinate differences.
        var center = await page.EvaluateAsync<float[]>(@"() => {
            const paths = [...document.querySelectorAll('path[fill=""none""]')]
                .filter(p => {
                    const r = p.getBoundingClientRect();
                    // Ignore zero-size or near-zero paths that aren't visible route polylines
                    return r.width > 1 && r.height > 1;
                });
            if (paths.length > 0) {
                const r = paths[0].getBoundingClientRect();
                return [r.left + r.width / 2, r.top + r.height / 2];
            }
            return null;
        }");

        center.ShouldNotBeNull();

        // Approach from outside the path's center to reliably trigger Google Maps' hit testing
        await page.Mouse.MoveAsync(center[0] - 5, center[1] - 5);
        await page.Mouse.MoveAsync(center[0], center[1], new() { Steps = 10 });

        var infoWindow = page.Locator(Selectors.InfoWindow);
        await infoWindow.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        return await infoWindow.InnerTextAsync();
    }

    public async Task<ApplicationPage> NotBeforeAsync(DateOnly value)
    {
        await EnterDateAsync(Selectors.NotBefore, value);
        return this;
    }

    public async Task<ApplicationPage> NotAfterAsync(DateOnly value)
    {
        await EnterDateAsync(Selectors.NotAfter, value);
        return this;
    }

    public async Task<string> EmissionsAsync()
        => await page.InnerTextTrimmedAsync("[data-js-emissions]");

    public async Task<string> TotalDistanceAsync()
        => await page.InnerTextTrimmedAsync("[data-js-total-distance]");

    public async Task<IReadOnlyList<TrackItem>> TracksAsync()
    {
        var children = await page.QuerySelectorAllAsync(Selectors.TrackItem);
        return [.. children.Skip(1).Select((p) => new TrackItem(p))];
    }

    public async Task TogglePolygonAsync()
        => await page.ClickAsync(Selectors.PolygonToggle);

    public async Task ToggleUnitsAsync()
    {
        string selector = Selectors.UnitOfDistanceToggle;

        await page.ClickAsync(selector);
        await page.WaitUntilEnabledAsync(selector);

        await WaitForLoaderToBeHiddenAsync();
    }

    public async Task<ApplicationPage> EnterDateAsync(string selector, DateOnly value)
    {
        var locator = page.Locator(selector);

        var culture = CultureInfo.CurrentUICulture;
        string input = value.ToString("d", culture);

        await locator.ClearAsync();
        await locator.PressSequentiallyAsync(input);
        await locator.PressAsync("Escape");

        return this;
    }

    public async Task WaitForMapAsync()
        => await page.WaitUntilVisibleAsync(Selectors.Map);

    public async Task WaitForTracksAsync()
        => await page.WaitUntilVisibleAsync(Selectors.TrackItem);

    private async Task WaitForLoaderToBeHiddenAsync()
        => await page.WaitUntilHiddenAsync(Selectors.Loader);

    private static class Selectors
    {
        public const string Filter = "id=filter";
        public const string Import = "id=import";
        public const string InfoWindow = ".gm-style-iw-d";
        public const string Loader = "id=tracks-loader";
        public const string Map = "[aria-label='Map']";
        public const string NotBefore = "id=not-before";
        public const string NotAfter = "id=not-after";
        public const string PolygonToggle = "id=show-polygon";
        public const string TrackItem = "css=.track-item";
        public const string UnitOfDistanceToggle = "id=unit-of-distance";
    }
}
