// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace MartinCostello.AppleFitnessWorkoutMapper.Pages;

public sealed class ApplicationPage(IPage page)
{
    /// <summary>
    /// Gets an init script that installs a complete fake Google Maps API before any page
    /// scripts run. Because <c>google.maps.importLibrary</c> is already defined when
    /// <c>@googlemaps/js-api-loader</c> initialises, the loader uses the fake directly and
    /// makes no CDN network requests. The fake captures polyline <c>mouseover</c> handlers
    /// and <c>InfoWindow.setContent</c> calls so Playwright tests can inspect info-window
    /// content without a real Google Maps API key.
    /// </summary>
    public static string MapsTestHooksScript { get; } = """
        (function () {
            'use strict';

            // Test state accessed by GetRouteInfoWindowHtmlAsync()
            window.__playwrightTest = { mouseoverHandlers: [], lastInfoWindowContent: null };
            var test = window.__playwrightTest;

            // Haversine distance (metres) between two fake LatLng objects
            function haversineDistance(p1, p2) {
                var R = 6371000;
                var lat1 = p1.lat() * Math.PI / 180;
                var lat2 = p2.lat() * Math.PI / 180;
                var dLat = (p2.lat() - p1.lat()) * Math.PI / 180;
                var dLng = (p2.lng() - p1.lng()) * Math.PI / 180;
                var a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
                        Math.cos(lat1) * Math.cos(lat2) *
                        Math.sin(dLng / 2) * Math.sin(dLng / 2);
                return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
            }

            // Fake MVCArray used as the polyline path
            function FakeMVCArray() { this._items = []; }
            FakeMVCArray.prototype.push = function (item) { this._items.push(item); };
            FakeMVCArray.prototype.getAt = function (i) { return this._items[i]; };
            FakeMVCArray.prototype.getLength = function () { return this._items.length; };
            FakeMVCArray.prototype[Symbol.iterator] = function () { return this._items[Symbol.iterator](); };

            // Fake LatLng
            function FakeLatLng(lat, lng) { this._lat = lat; this._lng = lng; }
            FakeLatLng.prototype.lat = function () { return this._lat; };
            FakeLatLng.prototype.lng = function () { return this._lng; };

            // Fake Map — sets aria-label so WaitForMapAsync passes
            function FakeMap(element) { element.setAttribute('aria-label', 'Map'); }
            FakeMap.prototype.fitBounds = function () {};

            // Fake LatLngBounds
            function FakeLatLngBounds() {}
            FakeLatLngBounds.prototype.extend = function () {};

            // Fake Polyline
            function FakePolyline() { this._path = new FakeMVCArray(); }
            FakePolyline.prototype.getPath = function () { return this._path; };
            FakePolyline.prototype.setMap = function () {};
            FakePolyline.prototype.setOptions = function () {};

            // Fake InfoWindow — captures content for assertions
            function FakeInfoWindow() {}
            FakeInfoWindow.prototype.setContent = function (content) { test.lastInfoWindowContent = content; };
            FakeInfoWindow.prototype.setPosition = function () {};
            FakeInfoWindow.prototype.open = function () {};
            FakeInfoWindow.prototype.close = function () {};

            // Fake Polygon
            function FakePolygon() {}
            FakePolygon.prototype.setMap = function () {};

            // Fake google.maps namespace — all APIs used by TrackMap and TrackPath
            var fakeMaps = {
                // importLibrary already defined → @googlemaps/js-api-loader skips CDN fetch
                importLibrary: function () { return Promise.resolve(fakeMaps); },
                Map: FakeMap,
                LatLng: FakeLatLng,
                LatLngBounds: FakeLatLngBounds,
                Polyline: FakePolyline,
                Polygon: FakePolygon,
                InfoWindow: FakeInfoWindow,
                SymbolPath: { FORWARD_CLOSED_ARROW: 2 },
                event: {
                    addListener: function (instance, eventName, handler) {
                        if (eventName === 'mouseover') {
                            test.mouseoverHandlers.push(handler);
                        }
                        return {};
                    }
                },
                geometry: {
                    spherical: {
                        computeLength: function (path) {
                            var total = 0;
                            var items = path._items;
                            for (var i = 1; i < items.length; i++) {
                                total += haversineDistance(items[i - 1], items[i]);
                            }
                            return total;
                        }
                    }
                }
            };

            // Install the fake before any page script runs so the loader uses it directly
            window.google = { maps: fakeMaps };
        })();
        """;

    public async Task<string?> GetRouteInfoWindowHtmlAsync()
    {
        await page.WaitForFunctionAsync("() => (window.__playwrightTest?.mouseoverHandlers?.length ?? 0) > 0");

        await page.EvaluateAsync("""
            var handlers = window.__playwrightTest?.mouseoverHandlers;
            if (handlers && handlers.length > 0) {
                handlers[0]({ latLng: { lat: function () { return 51.5; }, lng: function () { return -0.1; } } });
            }
            """);

        return await page.EvaluateAsync<string?>("""
            var content = window.__playwrightTest?.lastInfoWindowContent;
            return content ? content.innerHTML : null;
            """);
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
        public const string Loader = "id=tracks-loader";
        public const string Map = "[aria-label='Map']";
        public const string NotBefore = "id=not-before";
        public const string NotAfter = "id=not-after";
        public const string PolygonToggle = "id=show-polygon";
        public const string TrackItem = "css=.track-item";
        public const string UnitOfDistanceToggle = "id=unit-of-distance";
    }
}
