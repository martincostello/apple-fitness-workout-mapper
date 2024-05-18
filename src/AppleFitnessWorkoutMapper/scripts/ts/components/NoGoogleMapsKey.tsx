// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { Component } from 'react';

interface NoGoogleMapsKeyProperties {
    googleMapsApiKeyName: string;
    googleMapsApiKeyValue: string;
};

export class NoGoogleMapsKey extends Component<NoGoogleMapsKeyProperties, {}> {
    override render() {
        return (
            <div className="alert alert-warning" role="alert">
                <p className="lead">
                    No Google Maps API key is configured <i className="bi bi-exclamation-triangle" aria-hidden="true"></i>
                </p>
                <p>
                    To generate an API key follow the instructions here:
                    <em>
                        <a href="https://developers.google.com/maps/get-started#quickstart" rel="nofollow" target="_blank" aria-description="A link to the Google Maps getting started documentation">
                            Getting started with Google Maps Platform
                        </a>
                    </em>.
                    <i className="bi bi-box-arrow-up-right" aria-hidden="true"></i>
                </p>
                <p>
                    Once you have generated an API key, add it to the <code>{this.props.googleMapsApiKeyName}</code>
                    setting in the <code>appsettings.json</code> file in the directory containing this application&#39;s
                    files, restart the application and then reload this page.
                </p>
            </div>
        );
    }
}
