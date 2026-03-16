// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace MartinCostello.AppleFitnessWorkoutMapper.Pages;

public sealed class ApplicationPage(IPage page)
{
    public async Task<string> RouteInfoWindowAsync()
    {
        // Wait for Google Maps to render the route polylines as SVG paths.
        // Polylines are rendered with fill="none" in SVG overlay panes inside #map.
        // Note: the overlay panes are siblings of [aria-label='Map'], not children,
        // so the selector must be scoped to #map rather than [aria-label='Map'].
        await page.WaitForFunctionAsync(@"() => document.querySelectorAll('#map svg path[fill=""none""]').length > 0");

        // Get the screen coordinates of the center of the first route polyline path,
        // then move the mouse there to trigger Google Maps' real mouseover event.
        var position = await page.EvaluateAsync<double[]>(@"
            () => {
                const path = document.querySelector('#map svg path[fill=""none""]');
                if (!path) return null;
                const rect = path.getBoundingClientRect();
                if (rect.width === 0 && rect.height === 0) return null;
                return [rect.left + rect.width / 2, rect.top + rect.height / 2];
            }");

        if (position is null)
        {
            throw new InvalidOperationException("No route polyline path found in the map.");
        }

        await page.Mouse.MoveAsync((float)position[0], (float)position[1]);

        var infoWindow = page.Locator(Selectors.InfoWindow);
        await infoWindow.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        return await infoWindow.InnerTextAsync();
    }

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
