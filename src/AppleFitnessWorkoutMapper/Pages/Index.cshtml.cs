// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace MartinCostello.AppleFitnessWorkoutMapper.Pages;

public class IndexModel(TimeProvider timeProvider, IOptions<ApplicationOptions> options) : PageModel
{
    public TimeProvider TimeProvider { get; } = timeProvider;

    public string GoogleMapsApiKey { get; } = options.Value.GoogleMapsApiKey;

    public void OnGet()
    {
    }
}
