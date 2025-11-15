// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { defineConfig } from 'vitest/config';

export default defineConfig({
    test: {
        clearMocks: true,
        coverage: {
            enabled: true,
            provider: 'v8',
            reporter: ['html', 'json', 'text'],
        },
        reporters: process.env.GITHUB_ACTIONS ? ['default', 'github-actions'] : ['default'],
    },
});
