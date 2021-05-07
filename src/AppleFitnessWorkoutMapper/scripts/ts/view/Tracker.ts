// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { ApiClient } from '../client/ApiClient';
import { TrackModel } from '../models/TrackModel';

export class Tracker {

    private readonly loader: HTMLElement;
    private readonly mapElement: HTMLElement;
    private readonly tracksElement: HTMLElement;
    private readonly trackTemplate: HTMLElement;
    private readonly tracksCountElement: HTMLElement;

    constructor() {
        this.loader = document.getElementById('loader');
        this.mapElement = document.getElementById('map');
        this.tracksElement = document.getElementById('track-list');
        this.trackTemplate = document.getElementById('track-item-template');
        this.tracksCountElement = document.getElementById('track-list-count');
    }

    async initialize() {

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

        const map = new google.maps.Map(this.mapElement, options);

        google.maps.event.addDomListener(map, 'tilesloaded', () => {
        });

        // TODO Allow user to select a date range for the tracks
        const client = new ApiClient();
        const tracks = await client.getTracks();

        const models: TrackModel[] = [];

        // TODO Allow the user to highlight a specific track (and label it)
        // TODO Show/hide all tracks
        // TODO More styling and timestamp to the routes (more metadata, like duration and total distance in miles/km)?
        tracks.forEach((track) => {

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

            track.segments.forEach((segment) => {
                segment.forEach((point) => {
                    path.push(new google.maps.LatLng(point.latitude, point.longitude));
                });
            });

            route.setMap(map);

            // Clone the template
            const newNode = this.trackTemplate.cloneNode(true);
            this.tracksElement.appendChild(newNode);

            // Clear the duplicated Id from the new node
            const trackElement = this.tracksElement.lastElementChild;
            trackElement.setAttribute('id', '');

            // Set the name onto the templated node
            const trackLink = trackElement.firstElementChild;
            trackLink.setAttribute('data-track-name', track.name);
            trackLink.textContent = track.name;

            // Unhide once populated
            trackElement.classList.remove('d-none');

            const model = {
                element: trackLink,
                route: route,
                track: track
            };

            let visible = true;

            model.element.addEventListener('click', () => {
                model.route.setMap(visible ? null : map);
                visible = !visible;
            });

            models.push(model);
        });

        this.tracksCountElement.innerText = `(${models.length})`;

        this.loader.classList.add('d-none');

        const bounds = new google.maps.LatLngBounds();

        tracks.forEach((track) => {
            track.segments.forEach((segment) => {
                segment.forEach((point) => {
                    bounds.extend(new google.maps.LatLng(point.latitude, point.longitude));
                });
            });
        });

        map.fitBounds(bounds, 25);
    }
}
