// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import moment from 'moment';
import { describe, expect, test } from 'vitest';
import { createInfoWindowContent } from './TrackPath';

class FakeElement {
    readonly tagName: string;
    readonly children: FakeElement[] = [];

    className: string = '';
    textContent: string | null = null;

    constructor(tagName: string) {
        this.tagName = tagName;
    }

    appendChild<TElement extends FakeElement>(node: TElement): TElement {
        this.children.push(node);
        return node;
    }
}

class FakeDocument {
    createElement(tagName: string): FakeElement {
        return new FakeElement(tagName);
    }
}

function buildContent(trackName: string, displayDuration: string, displayDistance: string): FakeElement {
    return createInfoWindowContent(
        trackName,
        displayDuration,
        displayDistance,
        new FakeDocument() as unknown as Pick<Document, 'createElement'>
    ) as unknown as FakeElement;
}

describe('createInfoWindowContent', () => {
    test('should treat the track name as text content', () => {
        const trackName = '<img src=x onerror=alert("xss")>';

        const content = buildContent(trackName, '1 hour', '2 miles');
        const header = content.children[0];
        const body = content.children[1];
        const duration = body.children[0];
        const distance = body.children[1];

        expect(content.className).toBe('card border-secondary mb-3');
        expect(header.className).toBe('card-header');
        expect(header.textContent).toBe(trackName);
        expect(header.children).toHaveLength(0);
        expect(duration.textContent).toBe('Duration: ');
        expect(duration.children[0].textContent).toBe('1 hour');
        expect(distance.textContent).toBe('Distance: ');
        expect(distance.children[0].textContent).toBe('2 miles');
    });

    test('should compute track duration and distance correctly', () => {
        const startMoment = moment('2021-05-04T11:25:35Z');
        const endMoment = moment('2021-05-04T11:45:12Z');
        const duration = moment.duration(endMoment.diff(startMoment));

        const displayDuration = duration.humanize();
        const displayDistance =
            (1312.37 / 1000.0).toLocaleString(undefined, { maximumFractionDigits: 2, minimumFractionDigits: 2 }) + ' km';

        expect(displayDuration).not.toContain('undefined');
        expect(displayDistance).not.toContain('undefined');

        const content = buildContent('Route 1', displayDuration, displayDistance);
        const body = content.children[1];
        const durationText = body.children[0].children[0].textContent;
        const distanceText = body.children[1].children[0].textContent;

        expect(durationText).not.toContain('undefined');
        expect(durationText).toBeTruthy();
        expect(distanceText).not.toContain('undefined');
        expect(distanceText).toBeTruthy();
    });
});
