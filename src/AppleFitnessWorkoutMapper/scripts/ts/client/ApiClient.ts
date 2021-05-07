// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { Moment } from '../../../node_modules/moment/moment';
import { Track } from '../models/Track';

export class ApiClient {

    async getTracks(notBefore: Moment = null, notAfter: Moment = null): Promise<Track[]> {

        let requestUri = '/api/tracks';
        const query: {
            key: string;
            value: string;
        }[] = [];

        if (notBefore !== null) {
            query.push({
                key: 'notBefore',
                value: notBefore.toISOString()
            });
        }

        if (notAfter !== null) {
            query.push({
                key: 'notAfter',
                value: notAfter.toISOString()
            });
        }

        if (query.length > 0) {
            requestUri += '?';
            query.forEach((parameter) => {
                requestUri += `${encodeURIComponent(parameter.key)}=${encodeURIComponent(parameter.value)}&`;
            });
            requestUri = requestUri.substring(0, requestUri.length - 1);
        }

        const response = await fetch(requestUri);

        const tracks: Track[] = await response.json();

        return tracks;
    }
}
