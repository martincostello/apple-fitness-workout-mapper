// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.AppleFitnessWorkoutMapper.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace MartinCostello.AppleFitnessWorkoutMapper.Pages;

public class IndexModel(
    TrackService service,
    TimeProvider timeProvider,
    IOptions<ApplicationOptions> options) : PageModel
{
    public string GoogleMapsApiKey { get; } = options.Value.GoogleMapsApiKey;

    public string? StartDate { get; private set; }

    public string? EndDate { get; private set; }

    public async Task OnGet(CancellationToken cancellationToken)
    {
        var today = timeProvider.GetUtcNow().UtcDateTime.Date;
        var timestamp = await service.GetLatestTrackAsync(cancellationToken);

        timestamp ??= today;

        var endDate = timestamp.GetValueOrDefault().Date;

        if (endDate < today)
        {
            endDate = endDate.AddDays(1);
        }

        StartDate = endDate.AddDays(-28).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        EndDate = endDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
