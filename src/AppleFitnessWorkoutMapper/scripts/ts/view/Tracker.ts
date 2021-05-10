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

        const milesKey = 'use-miles';
        const polygonKey = 'show-polygon';

        try {
            this.ui.distanceUnits.checked = localStorage.getItem(milesKey) === 'true';
            this.ui.showPolygon.checked = localStorage.getItem(polygonKey) === 'true';
        } catch {}

        // HACK addEventListener() doesn't fire the event, so go via jQuery
        $(this.ui.distanceUnits).on('change', () => {
            try {
                localStorage.setItem(milesKey, this.ui.distanceUnits.checked.toString());
            } catch {}
            this.loadTracks();
        });

        // HACK addEventListener() doesn't fire the event, so go via jQuery
        $(this.ui.showPolygon).on('change', () => {
            const showPolygon = this.ui.showPolygon.checked;
            try {
                localStorage.setItem(polygonKey, showPolygon.toString());
            } catch { }
            this.map.fitBounds(showPolygon);
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
        this.ui.updateTotalDistanceAndEmissions(null, null);

        const tracks = await this.client.getTracks(notBefore, notAfter);

        const useMiles = this.ui.useMiles();

        let totalDistance = 0;

        tracks.forEach((track) => {
            const trackLink = this.ui.createTrackElement(track);

            const path = new TrackPath(trackLink, track, this.map, useMiles);
            totalDistance += path.getDistanceInPreferredUnits(useMiles);

            this.map.addPath(path);
        });

        if (tracks.length > 0) {

            totalDistance = Math.ceil(totalDistance);

            const emissionsPerMile = (280 + 310 + 410) / 3; // Taken from https://www.carbonindependent.org/17.html for gCO2/mile
            let distanceInMiles = totalDistance;

            if (!useMiles) {
                distanceInMiles = totalDistance * 0.621371;
            }

            const totalCO2EmissionsKilos = (distanceInMiles * emissionsPerMile) / 1000;

            const totalDistanceUnits = useMiles ? 'miles' : 'km';
            const totalDistanceString = totalDistance.toLocaleString(undefined, { maximumFractionDigits: 0 });
            const emissionsString = totalCO2EmissionsKilos.toLocaleString(undefined, { maximumFractionDigits: 0 });

            this.ui.updateSidebarCount(tracks.length);
            this.ui.updateTotalDistanceAndEmissions(`${totalDistanceString} ${totalDistanceUnits}`, emissionsString);

            this.map.fitBounds(this.ui.showPolygon.checked);
        }

        this.ui.enableFilters();

        return tracks.length;
    }
}
