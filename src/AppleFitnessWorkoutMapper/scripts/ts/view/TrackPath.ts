// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import moment from 'moment';
import { Track } from '../models/Track';
import { TrackMap } from './TrackMap';

export class TrackPath {
    private static blue: string = '#0000FF';
    private static red: string = '#FF0000';

    private readonly button: Element;
    private readonly container: Element;
    private readonly map: TrackMap;
    private readonly panel: Element;
    private readonly route: google.maps.Polyline;
    private readonly track: Track;

    private readonly displayDistance: string;
    private readonly displayDuration: string;

    private expanded: boolean;
    private highlighted: boolean;
    private visible: boolean;

    constructor(container: Element, track: Track, map: TrackMap, useMiles: boolean) {
        this.map = map;
        this.track = track;
        this.route = this.createRoute();
        this.visible = true;

        this.container = container;
        this.panel = this.container.querySelector('.track-item-panel');

        this.button = this.panel.querySelector('[data-js-visible]');
        this.button.addEventListener('click', () => {
            if (this.visible) {
                this.hidePath();
            } else {
                this.showPath();
            }
        });

        const startMoment = moment(track.timestamp);

        const startElement = this.panel.querySelector('[data-js-start]');
        startElement.textContent = startMoment.format('lll');
        startElement.setAttribute('title', startMoment.toISOString());

        const endMoment = moment(track.points[track.points.length - 1].timestamp);

        const endElement = this.panel.querySelector('[data-js-end]');
        endElement.textContent = endMoment.format('lll');
        endElement.setAttribute('title', endMoment.toISOString());

        const duration = moment.duration(endMoment.diff(startMoment));

        this.displayDuration = duration.humanize();

        const durationElement = this.panel.querySelector('[data-js-duration]');
        durationElement.textContent = this.displayDuration;
        durationElement.setAttribute('title', duration.toISOString());

        const distanceInMeters = this.getDistanceInMeters();
        const distance = this.getDistanceInPreferredUnits(useMiles);
        const units = useMiles ? 'miles' : 'km';

        const numberToText = (value: number, units: string) => {
            const options = { maximumFractionDigits: 2, minimumFractionDigits: 2 };
            return `${value.toLocaleString(undefined, options)} ${units}`;
        };

        this.displayDistance = numberToText(distance, units);

        const distanceElement = this.panel.querySelector('[data-js-distance]');
        distanceElement.textContent = this.displayDistance;
        distanceElement.setAttribute('title', numberToText(distanceInMeters, 'meters'));

        const pace = duration.asMinutes() / distance;
        const paceUnit = useMiles ? 'mile' : 'km';

        const paceMinutes = Math.floor(pace);
        const paceSeconds = ((pace % 1) * 60).toFixed(0);

        const paceElement = this.panel.querySelector('[data-js-pace]');
        paceElement.textContent = `${paceMinutes}'${paceSeconds}"/${paceUnit}`;

        this.container.addEventListener('click', () => {
            this.expanded = !this.expanded;
        });
        this.container.addEventListener('mouseover', () => {
            this.highlightIfNotAlready();
        });
        this.container.addEventListener('mouseout', () => {
            this.highlightIfNotAlready();
        });
    }

    getDistanceInMeters(): number {
        return google.maps.geometry.spherical.computeLength(this.route.getPath());
    }

    getDistanceInPreferredUnits(useMiles: boolean): number {
        const distanceInMeters = this.getDistanceInMeters();
        if (useMiles) {
            return distanceInMeters * 0.000621371;
        } else {
            return distanceInMeters / 1000.0;
        }
    }

    getPoints(): google.maps.LatLng[] {
        const result: google.maps.LatLng[] = [];

        this.track.points.forEach((point) => {
            result.push(new google.maps.LatLng(point.latitude, point.longitude));
        });

        return result;
    }

    hidePath() {
        this.visible = false;
        this.route.setMap(null);
        this.button.textContent = 'Show';
    }

    removeFromMap() {
        this.route.setMap(null);
    }

    showPath() {
        this.visible = true;
        this.route.setMap(this.map.getMap());
        this.button.textContent = 'Hide';
    }

    toggleHighlighting() {
        this.highlighted = !this.highlighted;
        this.route.setOptions({
            strokeColor: this.highlighted ? TrackPath.blue : TrackPath.red,
            zIndex: this.highlighted ? 1 : 0,
        });
    }

    private createRoute(): google.maps.Polyline {
        const route = new google.maps.Polyline({
            geodesic: true,
            icons: ['10%', '20%', '30%', '40%', '50%', '60%', '70%', '80%', '90%'].map((percent) => ({
                icon: { path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW },
                offset: percent,
            })),
            path: [],
            strokeColor: TrackPath.red,
            strokeOpacity: 1.0,
            strokeWeight: 1,
            zIndex: 0,
        });

        const path = route.getPath();

        this.track.points.forEach((point) => {
            path.push(new google.maps.LatLng(point.latitude, point.longitude));
        });

        const googleMap = this.map.getMap();
        const infoWindow = new google.maps.InfoWindow();

        google.maps.event.addListener(route, 'mouseout', () => {
            this.highlightIfNotAlready();
            infoWindow.close();
        });
        google.maps.event.addListener(route, 'mouseover', (e: any) => {
            this.highlightIfNotAlready();

            const content = `
<div class="card border-secondary mb-3">
  <div class="card-header">${this.track.name}</div>
  <div class="card-body text-secondary">
    <p class="card-text">
        Duration: <span>${this.displayDuration}</span>
    </p>
    <p class="card-text">
        Distance: <span>${this.displayDistance}</span>
    </p>
  </div>
</div>`;

            infoWindow.setContent(content);
            infoWindow.setPosition(e.latLng);
            infoWindow.open(googleMap);
        });

        route.setMap(googleMap);

        return route;
    }

    private highlightIfNotAlready() {
        if (!this.expanded) {
            this.toggleHighlighting();
        }
    }
}
