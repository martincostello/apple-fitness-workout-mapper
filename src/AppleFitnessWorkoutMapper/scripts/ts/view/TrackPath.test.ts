// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

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

describe('createInfoWindowContent', () => {
    test('should treat the track name as text content', () => {
        const trackName = '<img src=x onerror=alert("xss")>';

        const content = createInfoWindowContent(
            trackName,
            '1 hour',
            '2 miles',
            new FakeDocument() as unknown as Pick<Document, 'createElement'>
        ) as unknown as FakeElement;
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
});
