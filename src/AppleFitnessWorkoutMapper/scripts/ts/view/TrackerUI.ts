// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import moment from 'moment';
import { Moment } from 'moment';
import { Track } from '../models/Track';

export class TrackerUI {
    readonly distanceUnits: HTMLInputElement;
    readonly filterButton: Element;
    readonly hideAllButton: Element;
    readonly importButton: Element;
    readonly importContainer: Element;
    readonly map: HTMLElement;
    readonly showAllButton: Element;
    readonly showPolygon: HTMLInputElement;
    readonly tracksLoader: Element;

    private readonly importLoader: Element;
    private readonly notAfterDate: HTMLInputElement;
    private readonly notBeforeDate: HTMLInputElement;
    private readonly totalDistance: Element;
    private readonly trackItemTemplate: Element;
    private readonly tracksCount: Element;
    private readonly tracksList: Element;

    constructor() {
        this.distanceUnits = <HTMLInputElement>document.getElementById('unit-of-distance');
        this.filterButton = document.getElementById('filter');
        this.hideAllButton = document.getElementById('hide-all');
        this.importButton = document.getElementById('import');
        this.importContainer = document.getElementById('import-container');
        this.importLoader = document.getElementById('import-loader');
        this.map = <HTMLElement>document.getElementById('map');
        this.notAfterDate = <HTMLInputElement>document.getElementById('not-after');
        this.notBeforeDate = <HTMLInputElement>document.getElementById('not-before');
        this.showAllButton = document.getElementById('show-all');
        this.showPolygon = <HTMLInputElement>document.getElementById('show-polygon');
        this.totalDistance = document.getElementById('total-distance-container');
        this.trackItemTemplate = document.getElementById('track-item-template');
        this.tracksCount = document.getElementById('track-list-count');
        this.tracksList = document.getElementById('track-list');
        this.tracksLoader = document.getElementById('tracks-loader');
    }

    disable(element: Element) {
        element.setAttribute('disabled', '');
    }

    enable(element: Element) {
        element.removeAttribute('disabled');
    }

    hide(element: Element) {
        element.classList.add('d-none');
    }

    show(element: Element) {
        element.classList.remove('d-none');
    }

    disableFilters() {
        this.disable(this.filterButton);
        this.disable(this.hideAllButton);
        this.disable(this.notAfterDate);
        this.disable(this.notBeforeDate);
        this.disable(this.showAllButton);
        this.show(this.tracksLoader);
    }

    enableFilters() {
        this.enable(this.filterButton);
        this.enable(this.hideAllButton);
        this.enable(this.notAfterDate);
        this.enable(this.notBeforeDate);
        this.enable(this.showAllButton);
        this.hide(this.tracksLoader);
    }

    clearSidebar() {
        let sibling = this.trackItemTemplate.nextElementSibling;

        while (sibling !== null) {
            let previous = sibling;
            sibling = previous.nextElementSibling;
            previous.remove();
        }
    }

    createTrackElement(track: Track): Element {
        // Clone the template
        const newNode = this.trackItemTemplate.cloneNode(true);
        this.tracksList.appendChild(newNode);

        // Clear the duplicated Id from the new node
        const trackElement = this.tracksList.lastElementChild;
        trackElement.setAttribute('id', '');

        const collapseId = `details-${track.name.replace(' ', '_')}`;
        const collapseParentId = `${collapseId}-parent`;

        // Set up the collapse for the element containing the track details
        const details = trackElement.querySelector('.track-item-panel');
        details.setAttribute('id', collapseId);
        details.setAttribute('aria-labelledby', collapseParentId);

        // Set the name onto the templated node
        const trackTitle = trackElement.querySelector('.track-item-title');
        trackTitle.setAttribute('aria-controls', collapseId);
        trackTitle.setAttribute('data-bs-target', `#${collapseId}`);
        trackTitle.setAttribute('data-track-name', track.name);
        trackTitle.parentElement.setAttribute('id', collapseParentId);

        const trackName = document.createElement('span');
        trackName.textContent = track.name;
        trackTitle.appendChild(trackName);

        // Unhide once populated
        this.show(trackElement);

        return trackElement;
    }

    hideImport() {
        this.hide(this.importContainer);
    }

    resetImport() {
        this.hide(this.importLoader);
        this.enable(this.importButton);
        this.show(this.importButton.parentElement);
    }

    showImportInProgress() {
        this.disable(this.importButton);
        this.hide(this.importButton.parentElement);
        this.show(this.importLoader);
    }

    updateSidebarCount(count: number) {
        this.tracksCount.textContent = `(${count})`;
    }

    updateTotalDistanceAndEmissions(distance: string, emissions: string) {
        if (distance && emissions) {
            this.totalDistance.querySelector('[data-js-total-distance]').textContent = distance;
            this.totalDistance.querySelector('[data-js-emissions]').textContent = emissions;
            this.show(this.totalDistance);
        } else {
            this.hide(this.totalDistance);
        }
    }

    useMiles(): boolean {
        return (<HTMLInputElement>document.getElementById('unit-of-distance')).checked;
    }

    getNotAfter(): Moment {
        let result = this.getMoment(this.notAfterDate);

        if (result !== null) {
            result = result.add(1, 'days');
        }

        return result;
    }

    getNotBefore(): Moment {
        return this.getMoment(this.notBeforeDate);
    }

    private getMoment(element: HTMLInputElement): Moment {
        let result: Moment = null;

        if (element.value) {
            const parsed = moment(element.value);

            if (parsed.isValid()) {
                result = parsed;
            }
        }

        return result;
    }
}
