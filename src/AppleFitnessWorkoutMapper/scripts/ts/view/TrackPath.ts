// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import * as moment from '../../../node_modules/moment/moment';
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

    private expanded: boolean;
    private highlighted: boolean;
    private visible: boolean;

    constructor(container: Element, track: Track, map: TrackMap) {

        this.map = map;
        this.track = track;
        this.route = this.createRoute();
        this.visible = true;

        this.container = container;
        this.panel = this.container.nextElementSibling;

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

        const durationElement = this.panel.querySelector('[data-js-duration]');
        durationElement.textContent = duration.humanize();
        durationElement.setAttribute('title', duration.toISOString());

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
            zIndex: this.highlighted ? 1 : 0
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
            strokeColor: TrackPath.red,
            strokeOpacity: 1.0,
            strokeWeight: 1,
            zIndex: 0
        });

        const path = route.getPath();

        this.track.points.forEach((point) => {
            path.push(new google.maps.LatLng(point.latitude, point.longitude));
        });

        google.maps.event.addListener(route, 'mouseout', () => {
            this.highlightIfNotAlready();
        });
        google.maps.event.addListener(route, 'mouseover', () => {
            this.highlightIfNotAlready();
        });

        route.setMap(this.map.getMap());

        return route;
    }

    private highlightIfNotAlready() {
        if (!this.expanded) {
            this.toggleHighlighting();
        }
    }
}
