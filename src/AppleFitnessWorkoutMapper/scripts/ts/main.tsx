// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { createRoot } from 'react-dom/client';
import { Container } from './components/Container';

window.addEventListener('load', () => {
    const container = document.getElementById('container');
    if (container) {
        const startDate = container.getAttribute('data-start-date');
        const endDate = container.getAttribute('data-end-date');
        const googleMapsApiKeyName = container.getAttribute('data-google-maps-api-key-name');
        const googleMapsApiKeyValue = container.getAttribute('data-google-api-key-value');

        const root = createRoot(container);
        root.render(
            <>
                <Container
                    startDate={startDate}
                    endDate={endDate}
                    googleMapsApiKeyName={googleMapsApiKeyName}
                    googleMapsApiKeyValue={googleMapsApiKeyValue}
                />
            </>
        );
    }
});
