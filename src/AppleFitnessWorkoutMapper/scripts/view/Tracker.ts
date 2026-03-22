// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { importLibrary, setOptions } from '@googlemaps/js-api-loader';
import { ApiClient } from '../client/ApiClient';
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
        this.map = await this.createMap();
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

        let loadTracksOnUnitsChange = false;

        this.ui.distanceUnits.addEventListener('change', () => {
            const label = this.ui.distanceUnits.parentElement.querySelector('label');
            const attribute = this.ui.distanceUnits.checked ? 'data-text-checked' : 'data-text-unchecked';
            label.textContent = label.getAttribute(attribute);
            try {
                localStorage.setItem(milesKey, this.ui.distanceUnits.checked.toString());
            } catch {}
            if (loadTracksOnUnitsChange) {
                this.loadTracks();
            }
        });

        this.ui.showPolygon.addEventListener('change', () => {
            const showPolygon = this.ui.showPolygon.checked;
            try {
                localStorage.setItem(polygonKey, showPolygon.toString());
            } catch {}
            this.map.fitBounds(showPolygon);
        });

        try {
            if (localStorage.getItem(milesKey) === 'true') {
                this.ui.distanceUnits.checked = true;
                this.ui.distanceUnits.dispatchEvent(new Event('change'));
            }
            if (localStorage.getItem(polygonKey) === 'true') {
                this.ui.showPolygon.checked = true;
                this.ui.showPolygon.dispatchEvent(new Event('change'));
            }
        } catch {}

        loadTracksOnUnitsChange = true;

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
            const trackElement = this.ui.createTrackElement(track);

            const path = new TrackPath(trackElement, track, this.map, useMiles);
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

    private async createMap(): Promise<TrackMap> {
        const apiKey = this.ui.map.getAttribute('data-google-api-key');
        setOptions({
            key: apiKey,
            v: 'quarterly',
            libraries: ['geometry'],
        });
        await importLibrary('maps');
        return new TrackMap(this.ui.map);
    }
}
