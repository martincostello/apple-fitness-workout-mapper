// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { Track } from '../models/Track';

export class ApiClient {

    async getTracks(): Promise<Track[]> {

        // TODO Allow user to select a date range for the tracks
        const response = await fetch('api/tracks');

        const tracks: Track[] = await response.json();

        return tracks;
    }

}
