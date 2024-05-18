// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { Component } from 'react';
import { Track } from '../models/Track';

interface TrackItemProperties {
    track: Track;
};

interface TrackItemState {
    buttonText: string;
    visible: boolean;
};

export class TrackItem extends Component<TrackItemProperties, TrackItemState> {
    constructor(props: TrackItemProperties) {
        super(props);
        this.state = {
            buttonText: 'Show',
            visible: false
        };
    }

    override render() {
        const track = this.props.track;
        const collapseId = `details-${track.name}`;
        return (
            <li className="list-group-item track-item">
                <a className="ml-1 nav-link"
                    href="#"
                    role="button"
                    aria-expanded="false"
                    aria-controls={collapseId}
                    data-track-name={track.name}
                    onClick={(e) => {
                        this.toggleVisibility(collapseId);
                    }}>
                    <i className="bi-geo" aria-hidden="true"></i>
                    <span>{track.name}</span>
                </a>
                <div className="collapse" id={collapseId}>
                    <div className="card card-body">
                        <ul className="track-item-metadata">
                            <li>Started: <span data-js-start></span></li>
                            <li>Ended: <span data-js-end></span></li>
                            <li>Duration: <span data-js-duration></span></li>
                            <li>Distance: <span data-js-distance></span></li>
                            <li>Average Pace: <span data-js-pace></span></li>
                        </ul>
                        <div className="mt-2">
                            <button
                                type="button"
                                className="btn btn-secondary btn-sm float-right track-item-visibility"
                                onClick={this.togglePath}
                            >
                                {this.state.buttonText}
                            </button>
                        </div>
                    </div>
                </div>
            </li>
        );
    }

    private toggleVisibility(id: string): void {
        const card: any = $(document.getElementById(id));
        card.collapse('toggle');
    }

    private togglePath(_: React.MouseEvent<HTMLButtonElement>): void {
        let state: TrackItemState;
        if (this.state.visible) {
            state = {
                buttonText: 'Show',
                visible: false
            };
            //this.route.setMap(null);
        } else {
            state = {
                buttonText: 'Hide',
                visible: true
            };
            //this.route.setMap(this.map.getMap());
        }
        this.setState(state);
    }
}
