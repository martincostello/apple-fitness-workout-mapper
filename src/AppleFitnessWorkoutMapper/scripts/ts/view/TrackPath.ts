// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { Track } from '../models/Track';
import { TrackMap } from './TrackMap';

export class TrackPath {

    private readonly element: Element;
    private readonly map: TrackMap;
    private readonly route: google.maps.Polyline;
    private readonly track: Track;

    private visible: boolean;

    constructor(element: Element, track: Track, map: TrackMap) {
        this.element = element;
        this.map = map;
        this.track = track;
        this.route = this.createRoute();
        this.visible = this.route.getMap() !== null;

        this.element.addEventListener('click', () => {
            this.route.setMap(this.visible ? null : this.map.getMap());
            this.visible = !this.visible;
        });
    }

    private createRoute(): google.maps.Polyline {

        const route = new google.maps.Polyline({
            geodesic: true,
            icons: ['10%', '20%', '30%', '40%', '50%', '60%', '70%', '80%', '90%'].map((percent) => (
                {
                    icon: { path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW },
                    offset: percent
                }
            )),
            path: [],
            strokeColor: '#FF0000',
            strokeOpacity: 1.0,
            strokeWeight: 1
        });

        const path = route.getPath();

        this.track.segments.forEach((segment) => {
            segment.forEach((point) => {
                path.push(new google.maps.LatLng(point.latitude, point.longitude));
            });
        });

        route.setMap(this.map.getMap());

        return route;
    }
}
