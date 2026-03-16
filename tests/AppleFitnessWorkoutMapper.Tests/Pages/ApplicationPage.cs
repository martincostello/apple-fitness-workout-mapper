// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace MartinCostello.AppleFitnessWorkoutMapper.Pages;

public sealed class ApplicationPage(IPage page)
{
    /// <summary>
    /// A minimal init script that must be injected via <see cref="IPage.AddInitScriptAsync"/> before
    /// page navigation. It polls until <c>google.maps.Polyline</c> is available and then patches
    /// <c>google.maps.Polyline.prototype.setMap</c> to store every polyline that is added to the map
    /// in <c>window.__routes</c>, so that <see cref="RouteInfoWindowAsync"/> can trigger the
    /// Google Maps mouseover event on a route without needing the SVG overlay to render.
    /// </summary>
    internal const string RouteCapturingScript = """
        (function patchMaps() {
            if (window.__routes_patched) { return; }
            if (window.google && window.google.maps && window.google.maps.Polyline) {
                window.__routes_patched = true;
                const orig = window.google.maps.Polyline.prototype.setMap;
                window.google.maps.Polyline.prototype.setMap = function(map) {
                    if (map) { window.__routes = window.__routes || []; window.__routes.push(this); }
                    return orig.apply(this, arguments);
                };
            } else {
                setTimeout(patchMaps, 50);
            }
        }());
        """;

    public async Task<string> RouteInfoWindowAsync()
    {
        // Wait until at least one polyline has been added to the map.
        // The RouteCapturingScript init script stores polyline references in window.__routes.
        await page.WaitForFunctionAsync("() => (window.__routes && window.__routes.length > 0)");

        // Trigger the Google Maps mouseover event on the first route polyline.
        // This fires the handler in TrackPath that calls createInfoWindowContent and opens the
        // real Google Maps InfoWindow, appending the .gm-style-iw-d element to the DOM.
        await page.EvaluateAsync(@"
            () => {
                const route = window.__routes[0];
                google.maps.event.trigger(route, 'mouseover', { latLng: new google.maps.LatLng(0, 0) });
            }");

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
