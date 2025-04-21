// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace MartinCostello.AppleFitnessWorkoutMapper.Pages;

public sealed class TrackItem(IElementHandle element)
{
    public async Task ExpandAsync()
    {
        await element.ClickAsync();

        var child = await element.QuerySelectorAsync("[data-js-visible]");

        child.ShouldNotBeNull();
        await child.WaitForElementStateAsync(ElementState.Visible);
    }

    public async Task CollapseAsync()
        => await element.ClickAsync();

    public async Task<string> TitleAsync()
        => await InnerTextTrimmedAsync("button");

    public async Task<string> NameAsync()
    {
        var child = await element.QuerySelectorAsync("[data-track-name]");
        child.ShouldNotBeNull();
        return await child.GetAttributeAsync("data-track-name") ?? string.Empty;
    }

    public async Task<string> StartedAtAsync()
        => await InnerTextTrimmedAsync("[data-js-start]");

    public async Task<string> EndedAtAsync()
        => await InnerTextTrimmedAsync("[data-js-end]");

    public async Task<string> DurationAsync()
        => await InnerTextTrimmedAsync("[data-js-duration]");

    public async Task<string> DistanceAsync()
        => await InnerTextTrimmedAsync("[data-js-distance]");

    public async Task<string> AveragePaceAsync()
        => await InnerTextTrimmedAsync("[data-js-pace]");

    private async Task<string> InnerTextTrimmedAsync(string selector)
    {
        var child = await element.QuerySelectorAsync(selector);

        child.ShouldNotBeNull();
        string text = await child.InnerTextAsync();

        return text.Trim();
    }
}
