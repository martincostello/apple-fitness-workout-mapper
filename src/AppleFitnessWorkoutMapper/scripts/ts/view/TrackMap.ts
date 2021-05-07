// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { TrackPath } from './TrackPath';

export class TrackMap {

    private readonly map: google.maps.Map;
    private paths: TrackPath[];

    constructor(element: HTMLElement) {

        const london = { lat: 51.50809, lng: -0.1285907 };

        const options: google.maps.MapOptions = {
            center: london,
            clickableIcons: false,
            disableDoubleClickZoom: false,
            draggable: true,
            fullscreenControl: true,
            gestureHandling: 'greedy',
            mapTypeControl: true,
            maxZoom: 18,
            rotateControl: false,
            scaleControl: true,
            streetViewControl: true,
            styles: [
                {
                    featureType: 'poi',
                    elementType: 'labels',
                    stylers: [
                        { visibility: 'off' }
                    ]
                }
            ],
            zoom: 16,
            zoomControl: true
        };

        this.map = new google.maps.Map(element, options);
        this.paths = [];
    }

    addPath(path: TrackPath) {
        this.paths.push(path);
    }

    clearPaths() {
        this.paths.forEach((path) => {
            path.removeFromMap();
        });
        this.paths = [];
    }

    fitBounds() {

        const bounds = new google.maps.LatLngBounds();

        this.paths.forEach((path) => {
            path.getPoints().forEach((point) => {
                bounds.extend(point);
            });
        });

        this.map.fitBounds(bounds, 25);
    }

    hidePaths() {
        this.paths.forEach((path) => {
            path.hidePath();
        });
    }

    getMap(): google.maps.Map {
        return this.map;
    }

    showPaths() {
        this.paths.forEach((path) => {
            path.showPath();
        });
    }
}
