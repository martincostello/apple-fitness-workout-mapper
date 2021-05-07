// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import * as moment from '../../../node_modules/moment/moment';
import { ApiClient } from '../client/ApiClient';
import { Moment } from '../../../node_modules/moment/moment';
import { Track } from '../models/Track';
import { TrackMap } from './TrackMap';
import { TrackPath } from './TrackPath';

export class Tracker {

    private readonly filterElement: HTMLElement;
    private readonly hideAllElement: HTMLElement;
    private readonly loader: HTMLElement;
    private readonly mapElement: HTMLElement;
    private readonly notAfterElement: HTMLInputElement;
    private readonly notBeforeElement: HTMLInputElement;
    private readonly showAllElement: HTMLElement;
    private readonly trackTemplate: HTMLElement;
    private readonly tracksCountElement: HTMLElement;
    private readonly tracksElement: HTMLElement;

    private client: ApiClient;
    private map: TrackMap;

    constructor() {
        this.filterElement = document.getElementById('filter');
        this.hideAllElement = document.getElementById('hide-all');
        this.loader = document.getElementById('loader');
        this.mapElement = document.getElementById('map');
        this.notAfterElement = <HTMLInputElement>document.getElementById('not-after');
        this.notBeforeElement = <HTMLInputElement>document.getElementById('not-before');
        this.showAllElement = document.getElementById('show-all');
        this.trackTemplate = document.getElementById('track-item-template');
        this.tracksCountElement = document.getElementById('track-list-count');
        this.tracksElement = document.getElementById('track-list');
    }

    async initialize() {

        this.map = new TrackMap(this.mapElement);
        this.client = new ApiClient();

        this.filterElement.addEventListener('click', () => {
            this.reloadTracks();
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

        await this.reloadTracks();
    }

    private createTrackElement(track: Track): Element {

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

        return trackLink;
    }

    private static disableElement(element: HTMLElement) {
        element.setAttribute('disabled', '');
    }

    private static enableElement(element: HTMLElement) {
        element.removeAttribute('disabled');
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

    private async reloadTracks() {

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

        const tracks = await this.client.getTracks(notBefore, notAfter);

        // TODO Allow the user to highlight a specific track (and label it)
        // TODO Show/hide all tracks
        // TODO More styling and timestamp to the routes (more metadata, like duration and total distance in miles/km)?
        tracks.forEach((track) => {
            const trackLink = this.createTrackElement(track);
            this.map.addPath(new TrackPath(trackLink, track, this.map));
        });

        this.updateWorkoutCount(tracks.length);

        this.map.fitBounds();

        this.enableFilters();
        this.hideLoader();
    }

    private showLoader() {
        this.loader.classList.remove('d-none');
    }

    private hideLoader() {
        this.loader.classList.add('d-none');
    }

    private updateWorkoutCount(count: number) {
        this.tracksCountElement.innerText = `(${count})`;
    }
}
