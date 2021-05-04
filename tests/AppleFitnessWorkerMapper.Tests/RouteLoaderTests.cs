// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using Shouldly;
using Xunit;

namespace MartinCostello.AppleFitnessWorkerMapper
{
    public class RouteLoaderTests
    {
        [Fact]
        public void Can_Create_RouteLoader()
        {
            // Act
            var target = new RouteLoader();

            // Assert
            target.ShouldNotBeNull();
        }
    }
}
