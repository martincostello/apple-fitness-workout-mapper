// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { TrackPath } from './TrackPath';

export class TrackMap {

    private readonly map: google.maps.Map;
    private paths: TrackPath[];
    private polygon: google.maps.Polygon;

    constructor(element: HTMLElement) {

        const options: google.maps.MapOptions = {
            center: { lat: 0.0, lng: 0.0 },
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
            zoom: 2,
            zoomControl: true
        };

        this.map = new google.maps.Map(element, options);
        this.paths = [];
        this.polygon = null;
    }

    addPath(path: TrackPath) {
        this.paths.push(path);
    }

    clearPaths() {

        this.paths.forEach((path) => {
            path.removeFromMap();
        });
        this.paths = [];

        if (this.polygon !== null) {
            this.polygon.setMap(null);
            this.polygon = null;
        }
    }

    fitBounds(showPolygon: boolean = false) {

        if (this.polygon !== null) {
            this.hidePolygon();
            this.polygon = null;
        }

        const bounds = new google.maps.LatLngBounds();

        const polyPaths: google.maps.LatLng[] = [];

        this.paths.forEach((path) => {
            path.getPoints().forEach((point) => {
                bounds.extend(point);
                polyPaths.push(point);
            });
        });

        this.map.fitBounds(bounds, 25);

        if (showPolygon) {
            const red: string = '#FF0000';
            this.polygon = new google.maps.Polygon({
                fillColor: red,
                fillOpacity: 0.35,
                paths: polyPaths,
                strokeColor: red,
                strokeOpacity: 0.8,
                strokeWeight: 3,
                zIndex: -1
            });
            this.polygon.setMap(this.map);
        }
    }

    hidePaths() {
        this.paths.forEach((path) => {
            path.hidePath();
        });
        this.hidePolygon();
    }

    hidePolygon() {
        if (this.polygon !== null) {
            this.polygon.setMap(null);
        }
    }

    getMap(): google.maps.Map {
        return this.map;
    }

    showPaths() {
        this.paths.forEach((path) => {
            path.showPath();
        });
        this.showPolygon();
    }

    showPolygon() {
        if (this.polygon !== null) {
            this.polygon.setMap(this.map);
        }
    }
}
