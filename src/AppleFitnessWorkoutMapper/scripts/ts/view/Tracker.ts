// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import * as moment from '../../../node_modules/moment/moment';
import { Moment } from '../../../node_modules/moment/moment';
import { ApiClient } from '../client/ApiClient';
import { Track } from '../models/Track';
import { TrackMap } from './TrackMap';
import { TrackPath } from './TrackPath';

export class Tracker {

    private readonly filterElement: HTMLElement;
    private readonly hideAllElement: HTMLElement;
    private readonly importContainer: HTMLElement;
    private readonly importElement: HTMLElement;
    private readonly importLoader: HTMLElement;
    private readonly mapElement: HTMLElement;
    private readonly notAfterElement: HTMLInputElement;
    private readonly notBeforeElement: HTMLInputElement;
    private readonly showAllElement: HTMLElement;
    private readonly trackTemplate: HTMLElement;
    private readonly tracksCountElement: HTMLElement;
    private readonly tracksElement: HTMLElement;
    private readonly tracksLoader: HTMLElement;

    private client: ApiClient;
    private map: TrackMap;

    constructor() {
        this.filterElement = document.getElementById('filter');
        this.hideAllElement = document.getElementById('hide-all');
        this.importContainer = document.getElementById('import-container');
        this.importElement = document.getElementById('import');
        this.importLoader = document.getElementById('import-loader');
        this.mapElement = document.getElementById('map');
        this.notAfterElement = <HTMLInputElement>document.getElementById('not-after');
        this.notBeforeElement = <HTMLInputElement>document.getElementById('not-before');
        this.showAllElement = document.getElementById('show-all');
        this.trackTemplate = document.getElementById('track-item-template');
        this.tracksCountElement = document.getElementById('track-list-count');
        this.tracksElement = document.getElementById('track-list');
        this.tracksLoader = document.getElementById('tracks-loader');
    }

    async initialize() {

        this.map = new TrackMap(this.mapElement);
        this.client = new ApiClient();

        this.filterElement.addEventListener('click', () => {
            this.loadTracks();
        });

        this.hideAllElement.addEventListener('click', () => {
            Tracker.disableElement(this.hideAllElement);
            this.map.hidePaths();
            Tracker.enableElement(this.hideAllElement);
        });

        this.showAllElement.addEventListener('click', () => {
            Tracker.disableElement(this.showAllElement);
            this.map.showPaths();
            Tracker.enableElement(this.showAllElement);
        });

        const count = await this.client.getCount();

        if (count < 1) {

            this.disableFilters();

            Tracker.showElement(this.importContainer);

            this.importElement.addEventListener('click', () => {
                this.importTracks();
            });

            Tracker.enableElement(this.importElement);
            this.hideLoader();
        } else {
            await this.loadTracks();
        }
    }

    private createTrackElement(track: Track): Element {

        // Clone the template
        const newNode = this.trackTemplate.cloneNode(true);
        this.tracksElement.appendChild(newNode);

        // Clear the duplicated Id from the new node
        const trackElement = this.tracksElement.lastElementChild;
        trackElement.setAttribute('id', '');

        const collapseId = `details-${track.name}`;

        // Set the name onto the templated node
        const trackLink = trackElement.firstElementChild;
        trackLink.setAttribute('aria-controls', collapseId);
        trackLink.setAttribute('data-track-name', track.name);
        trackLink.textContent = track.name;

        // Set up the collapse for the element containing the track details
        const details = trackLink.nextElementSibling;

        details.setAttribute('id', collapseId);

        trackLink.addEventListener('click', () => {
            ($(details) as any).collapse('toggle');
        });

        // Unhide once populated
        Tracker.showElement(trackElement);

        return trackLink;
    }

    private static disableElement(element: Element) {
        element.setAttribute('disabled', '');
    }

    private static enableElement(element: Element) {
        element.removeAttribute('disabled');
    }

    private static hideElement(element: Element) {
        element.classList.add('d-none');
    }

    private static showElement(element: Element) {
        element.classList.remove('d-none');
    }

    private disableFilters() {
        Tracker.disableElement(this.filterElement);
        Tracker.disableElement(this.hideAllElement);
        Tracker.disableElement(this.notAfterElement);
        Tracker.disableElement(this.notBeforeElement);
        Tracker.disableElement(this.showAllElement);
    }

    private enableFilters() {
        Tracker.enableElement(this.filterElement);
        Tracker.enableElement(this.hideAllElement);
        Tracker.enableElement(this.notAfterElement);
        Tracker.enableElement(this.notBeforeElement);
        Tracker.enableElement(this.showAllElement);
    }

    private async importTracks() {

        Tracker.disableElement(this.importElement);
        Tracker.hideElement(this.importElement.parentElement);
        Tracker.showElement(this.importLoader);

        const count = await this.client.importTracks();

        Tracker.hideElement(this.importLoader);

        if (count < 1) {
            Tracker.enableElement(this.importElement);
            Tracker.showElement(this.importElement.parentElement);
        } else {
            Tracker.hideElement(this.importContainer);
            await this.loadTracks();
        }
    }

    private async loadTracks(): Promise<number> {

        this.disableFilters();
        this.showLoader();

        // Clear any existing map paths and the tracks in the sidebar
        this.map.clearPaths();

        let sibling = this.trackTemplate.nextElementSibling;

        while (sibling !== null) {
            let previous = sibling;
            sibling = previous.nextElementSibling;
            previous.remove();
        }

        let notBefore: Moment = null;
        let notAfter: Moment = null;

        if (this.notBeforeElement.value) {
            notBefore = moment(this.notBeforeElement.value);
        }

        if (this.notAfterElement.value) {
            notAfter = moment(this.notAfterElement.value).add(1, 'days');
        }

        this.updateWorkoutCount(0);

        const tracks = await this.client.getTracks(notBefore, notAfter);

        // TODO Add help icon that links to repo
        // TODO Add Apple icon to brand header
        // TODO Convert timestamps to local browser time zone
        // TODO Include empty App_Data folder in dotnet publish
        // TODO Refactor elements in this class to their own class
        // TODO Reset individual show/hide buttons when using the show/hide all buttons
        // TODO Apply labels to the tracks on the map
        // TODO More styling to tracks and more metadata, like duration and total distance in miles/km?
        // TODO Draw a bounding box with the NEWS extents and the area/total path length etc?
        tracks.forEach((track) => {
            const trackLink = this.createTrackElement(track);
            this.map.addPath(new TrackPath(trackLink, track, this.map));
        });

        this.updateWorkoutCount(tracks.length);

        if (tracks.length > 0) {
            this.map.fitBounds();
        }

        this.enableFilters();
        this.hideLoader();

        return tracks.length;
    }

    private showLoader() {
        Tracker.showElement(this.tracksLoader);
    }

    private hideLoader() {
        Tracker.hideElement(this.tracksLoader);
    }

    private updateWorkoutCount(count: number) {
        this.tracksCountElement.innerText = `(${count})`;
    }
}
