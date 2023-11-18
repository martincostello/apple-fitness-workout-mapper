// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Polly;

namespace MartinCostello.AppleFitnessWorkoutMapper.Data;

public class ResilientExecutionStrategy(
    ExecutionStrategyDependencies dependencies,
    ResiliencePipeline pipeline) : IExecutionStrategy
{
    public bool RetriesOnFailure => true;

    [ExcludeFromCodeCoverage]
    public TResult Execute<TState, TResult>(
        TState state,
        Func<DbContext, TState, TResult> operation,
        Func<DbContext, TState, ExecutionResult<TResult>>? verifySucceeded)
    {
        throw new NotImplementedException();
    }

    public async Task<TResult> ExecuteAsync<TState, TResult>(
        TState state,
        Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
        Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>>? verifySucceeded,
        CancellationToken cancellationToken = default)
    {
        var dbContext = dependencies.CurrentContext.Context;
        return await pipeline.ExecuteAsync(
            static async (state, token) => await state.operation(state.dbContext, state.state, token),
            (operation, dbContext, state),
            cancellationToken);
    }
}
