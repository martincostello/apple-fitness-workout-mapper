// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { Component } from 'react';
import { Track } from '../models/Track';
import { TrackItem } from './TrackItem';

interface TrackListProperties {
    tracks: Track[];
};

export class TrackList extends Component<TrackListProperties, {}> {
    override render() {
        const tracks = this.props.tracks.map((track) =>
            <TrackItem track={track} />
        );
        return (
            <ul className="list-group flex-column mb-2" id="track-list">
                {tracks}
            </ul>
        );
    }
}
