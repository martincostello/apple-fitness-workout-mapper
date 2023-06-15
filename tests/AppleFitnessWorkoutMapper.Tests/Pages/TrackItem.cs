// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace MartinCostello.AppleFitnessWorkoutMapper.Pages;

public sealed class TrackItem
{
    private readonly IElementHandle _element;

    public TrackItem(IElementHandle element)
    {
        _element = element;
    }

    public async Task ExpandAsync()
    {
        await _element.ClickAsync();

        IElementHandle? child = await _element.QuerySelectorAsync("[data-js-visible]");

        child.ShouldNotBeNull();
        await child.WaitForElementStateAsync(ElementState.Visible);
    }

    public async Task CollapseAsync()
        => await _element.ClickAsync();

    public async Task<string> LinkTextAsync()
        => await InnerTextTrimmedAsync("a");

    public async Task<string> NameAsync()
    {
        IElementHandle? child = await _element.QuerySelectorAsync("[data-track-name]");
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

    internal async Task WaitUntilVisibleAsync()
        => await _element.WaitForElementStateAsync(ElementState.Visible);

    private async Task<string> InnerTextTrimmedAsync(string selector)
    {
        IElementHandle? child = await _element.QuerySelectorAsync(selector);

        child.ShouldNotBeNull();
        string text = await child.InnerTextAsync();

        return text.Trim();
    }
}
