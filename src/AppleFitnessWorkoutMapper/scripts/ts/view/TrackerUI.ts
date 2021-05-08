// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { Track } from '../models/Track';

export class TrackerUI {

    readonly filterButton: Element;
    readonly hideAllButton: Element;
    readonly importButton: Element;
    readonly importContainer: Element;
    readonly importLoader: Element;
    readonly map: Element;
    readonly notAfterDate: HTMLInputElement;
    readonly notBeforeDate: HTMLInputElement;
    readonly showAllButton: Element;
    readonly trackItemTemplate: Element;
    readonly tracksCount: HTMLElement;
    readonly tracksList: Element;
    readonly tracksLoader: Element;

    constructor() {
        this.filterButton = document.getElementById('filter');
        this.hideAllButton = document.getElementById('hide-all');
        this.importButton = document.getElementById('import');
        this.importContainer = document.getElementById('import-container');
        this.importLoader = document.getElementById('import-loader');
        this.map = document.getElementById('map');
        this.notAfterDate = <HTMLInputElement>document.getElementById('not-after');
        this.notBeforeDate = <HTMLInputElement>document.getElementById('not-before');
        this.showAllButton = document.getElementById('show-all');
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
    }

    enableFilters() {
        this.enable(this.filterButton);
        this.enable(this.hideAllButton);
        this.enable(this.notAfterDate);
        this.enable(this.notBeforeDate);
        this.enable(this.showAllButton);
    }

    createTrackElement(track: Track): Element {

        // Clone the template
        const newNode = this.trackItemTemplate.cloneNode(true);
        this.tracksList.appendChild(newNode);

        // Clear the duplicated Id from the new node
        const trackElement = this.tracksList.lastElementChild;
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
        this.show(trackElement);

        return trackLink;
    }
}
