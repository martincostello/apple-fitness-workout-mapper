// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { ApiClient } from '../client/ApiClient';
import { TrackerUI } from './TrackerUI';
import { TrackMap } from './TrackMap';
import { TrackPath } from './TrackPath';

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
            this.ui.hide(this.ui.tracksLoader);
        } else {
            await this.loadTracks();
        }
    }

    private async importTracks() {

        this.ui.showImportInProgress();

        const count = await this.client.importTracks();

        if (count < 1) {
            this.ui.resetImport();
        } else {
            this.ui.hideImport();
            await this.loadTracks();
        }
    }

    private async loadTracks(): Promise<number> {

        this.ui.disableFilters();

        // Clear any existing map paths and the tracks in the sidebar
        this.map.clearPaths();
        this.ui.clearSidebar();

        let notBefore = this.ui.getNotBefore();
        let notAfter = this.ui.getNotAfter();

        this.ui.updateSidebarCount(0);

        const tracks = await this.client.getTracks(notBefore, notAfter);

        // TODO Apply labels to the tracks on the map
        // TODO Duration and total distance in miles/km for tracks
        // TODO Draw a bounding box with the NEWS extents and the area/total path length etc?
        tracks.forEach((track) => {
            const trackLink = this.ui.createTrackElement(track);
            this.map.addPath(new TrackPath(trackLink, track, this.map));
        });

        this.ui.updateSidebarCount(tracks.length);

        if (tracks.length > 0) {
            this.map.fitBounds();
        }

        this.ui.enableFilters();

        return tracks.length;
    }
}
