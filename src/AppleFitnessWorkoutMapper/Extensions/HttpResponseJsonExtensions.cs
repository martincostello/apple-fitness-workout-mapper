// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// Provides extension methods for writing a JSON serialized value to the HTTP response.
/// </summary>
/// <remarks>
/// Based on https://github.com/dotnet/aspnetcore/blob/main/src/Http/Http.Extensions/src/HttpResponseJsonExtensions.cs.
/// </remarks>
public static class HttpResponseJsonExtensions
{
    /// <summary>
    /// Write the specified value as JSON to the response body. The response content-type will be set to
    /// the specified content-type.
    /// </summary>
    /// <typeparam name="T">The type of the value to write as JSON.</typeparam>
    /// <param name="response">The response to write JSON to.</param>
    /// <param name="value">The value to write as JSON.</param>
    /// <param name="jsonTypeInfo">The JSON type metadata to use when serializing the value.</param>
    /// <param name="contentType">The content-type to set on the response.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel the operation.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static Task WriteAsJsonAsync<T>(
        this HttpResponse response,
        T value,
        JsonTypeInfo<T> jsonTypeInfo,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        response.ContentType = contentType ?? "application/json; charset=utf-8";

        return JsonSerializer.SerializeAsync(response.Body, value, jsonTypeInfo, cancellationToken);
    }
}
