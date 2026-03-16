// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace MartinCostello.AppleFitnessWorkoutMapper.Pages;

public sealed class ApplicationPage(IPage page)
{
    /// <summary>
    /// Gets an init script that hooks into the Google Maps API to capture mouseover event
    /// handlers and info window content for use in integration tests.
    /// </summary>
    public static string MapsTestHooksScript { get; } = """
        (function () {
            window.__playwrightTest = { mouseoverHandlers: [], lastInfoWindowContent: null };

            var test = window.__playwrightTest;

            function hookMaps(maps) {
                if (!maps || maps.__hooked) return;
                if (!maps.InfoWindow || !maps.InfoWindow.prototype) return;
                if (!maps.event || !maps.event.addListener) return;

                maps.__hooked = true;

                var origSetContent = maps.InfoWindow.prototype.setContent;
                maps.InfoWindow.prototype.setContent = function (content) {
                    test.lastInfoWindowContent = content;
                    return origSetContent.call(this, content);
                };

                var origAddListener = maps.event.addListener;
                maps.event.addListener = function (instance, eventName, handler) {
                    if (eventName === 'mouseover') {
                        test.mouseoverHandlers.push(handler);
                    }
                    return origAddListener.call(this, instance, eventName, handler);
                };
            }

            function watchProperty(obj, propName, callback) {
                var value = obj[propName];
                if (value) {
                    callback(value);
                    return;
                }
                Object.defineProperty(obj, propName, {
                    configurable: true,
                    enumerable: true,
                    get: function () { return value; },
                    set: function (v) {
                        value = v;
                        if (v) { callback(v); }
                    }
                });
            }

            watchProperty(window, 'google', function (google) {
                if (google.maps) {
                    hookMaps(google.maps);
                } else {
                    watchProperty(google, 'maps', function (maps) {
                        hookMaps(maps);
                    });
                }
            });
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
