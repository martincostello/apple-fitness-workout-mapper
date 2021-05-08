// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import * as moment from '../../../node_modules/moment/moment';
import { Moment } from '../../../node_modules/moment/moment';
import { ApiClient } from '../client/ApiClient';
import { Track } from '../models/Track';
import { TrackMap } from './TrackMap';
import { TrackPath } from './TrackPath';
import { TrackerUI } from './TrackerUI';

export class Tracker {

    private readonly ui: TrackerUI;

    private client: ApiClient;
    private map: TrackMap;

    constructor() {
        this.ui = new TrackerUI();
    }

    async initialize() {

        this.map = new TrackMap(this.ui.map);
        this.client = new ApiClient();

        this.ui.filterButton.addEventListener('click', () => {
            this.loadTracks();
        });

        this.ui.hideAllButton.addEventListener('click', () => {
            this.ui.disable(this.ui.hideAllButton);
            this.map.hidePaths();
            this.ui.enable(this.ui.hideAllButton);
        });

        this.ui.showAllButton.addEventListener('click', () => {
            this.ui.disable(this.ui.showAllButton);
            this.map.showPaths();
            this.ui.enable(this.ui.showAllButton);
        });

        const count = await this.client.getCount();

        if (count < 1) {

            this.ui.disableFilters();

            this.ui.show(this.ui.importContainer);

            this.ui.importButton.addEventListener('click', () => {
                this.importTracks();
            });

            this.ui.enable(this.ui.importButton);
            this.hideLoader();
        } else {
            await this.loadTracks();
        }
    }

    private createTrackElement(track: Track): Element {

        // Clone the template
        const newNode = this.ui.trackItemTemplate.cloneNode(true);
        this.ui.tracksList.appendChild(newNode);

        // Clear the duplicated Id from the new node
        const trackElement = this.ui.tracksList.lastElementChild;
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
        this.ui.show(trackElement);

        return trackLink;
    }

    private async importTracks() {

        this.ui.disable(this.ui.importButton);
        this.ui.hide(this.ui.importButton.parentElement);
        this.ui.show(this.ui.importLoader);

        const count = await this.client.importTracks();

        this.ui.hide(this.ui.importLoader);

        if (count < 1) {
            this.ui.enable(this.ui.importButton);
            this.ui.show(this.ui.importButton.parentElement);
        } else {
            this.ui.hide(this.ui.importContainer);
            await this.loadTracks();
        }
    }

    private async loadTracks(): Promise<number> {

        this.ui.disableFilters();
        this.showLoader();

        // Clear any existing map paths and the tracks in the sidebar
        this.map.clearPaths();

        let sibling = this.ui.trackItemTemplate.nextElementSibling;

        while (sibling !== null) {
            let previous = sibling;
            sibling = previous.nextElementSibling;
            previous.remove();
        }

        let notBefore: Moment = null;
        let notAfter: Moment = null;

        if (this.ui.notBeforeDate.value) {
            notBefore = moment(this.ui.notBeforeDate.value);
        }

        if (this.ui.notAfterDate.value) {
            notAfter = moment(this.ui.notAfterDate.value).add(1, 'days');
        }

        this.updateWorkoutCount(0);

        const tracks = await this.client.getTracks(notBefore, notAfter);

        // TODO Add help icon that links to repo
        // TODO Include empty App_Data folder in dotnet publish
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

        this.ui.enableFilters();
        this.hideLoader();

        return tracks.length;
    }

    private showLoader() {
        this.ui.show(this.ui.tracksLoader);
    }

    private hideLoader() {
        this.ui.hide(this.ui.tracksLoader);
    }

    private updateWorkoutCount(count: number) {
        this.ui.tracksCount.innerText = `(${count})`;
    }
}
