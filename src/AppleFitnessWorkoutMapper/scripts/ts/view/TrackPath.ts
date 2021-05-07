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
            if (this.visible) {
                this.hidePath();
            } else {
                this.showPath();
            }
            this.visible = !this.visible;
        });
    }

    getPoints(): google.maps.LatLng[] {

        const result: google.maps.LatLng[] = [];

        this.track.segments.forEach((segment) => {
            segment.forEach((point) => {
                result.push(new google.maps.LatLng(point.latitude, point.longitude));
            });
        });

        return result;
    }

    hidePath() {
        this.route.setMap(null);
    }

    removeFromMap() {
        this.route.setMap(null);
    }

    showPath() {
        this.route.setMap(this.map.getMap());
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
