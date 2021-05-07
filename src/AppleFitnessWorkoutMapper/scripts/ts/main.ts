// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { Track } from './models/Track';

const initializer = async () => {

    const london = { lat: 51.50809, lng: -0.1285907 };

    const options: google.maps.MapOptions = {
        center: london,
        clickableIcons: false,
        disableDoubleClickZoom: false,
        draggable: true,
        fullscreenControl: true,
        gestureHandling: 'greedy',
        mapTypeControl: false,
        maxZoom: 16,
        rotateControl: false,
        scaleControl: true,
        scrollwheel: false,
        streetViewControl: false,
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

    const mapElement = document.getElementById('map');
    const tracksElement = document.getElementById('track-list');
    const trackTemplate = document.getElementById('track-item-template');
    const tracksCountElement = document.getElementById('track-list-count');

    const map = new google.maps.Map(mapElement, options);

    google.maps.event.addDomListener(map, 'tilesloaded', () => {
    });

    const response = await fetch('api/tracks');

    const tracks: Track[] = await response.json();

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

        const trackElement = document.createElement('li');
        trackElement.classList.add('nav-item');

        const trackLink = document.createElement('a');
        trackLink.classList.add('nav-link');
        trackLink.setAttribute('href', '#');
        trackLink.setAttribute('data-track-name', track.name);
        trackLink.innerText = track.name;
        trackElement.appendChild(trackLink);

        tracksElement.appendChild(trackElement);
    });

    tracksCountElement.innerText = `(${tracks.length})`;

    const bounds = new google.maps.LatLngBounds();

    tracks.forEach((track) => {
        track.segments.forEach((segment) => {
            segment.forEach((point) => {
                bounds.extend(new google.maps.LatLng(point.latitude, point.longitude));
            });
        });
    });

    map.fitBounds(bounds, 25);
};

google.maps.event.addDomListener(window, 'load', initializer);
