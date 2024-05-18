// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { Component } from 'react';
import { DatePicker } from './DatePicker';
import { NoGoogleMapsKey } from './NoGoogleMapsKey';
import { Tracker } from '../view/Tracker';

interface AppProperties {
    startDate: string;
    endDate: string;
    googleMapsApiKeyName: string;
    googleMapsApiKeyValue: string;
};

interface AppState {
};

export class App extends Component<AppProperties, AppState> {
    constructor(props: AppProperties) {
        super(props);
        this.state = {};
    }

    override componentDidMount() {
        const tracker = new Tracker();
        tracker.initialize();
        const calendar: any = $('.calendar');
        calendar.datepicker({
            autoclose: true,
            clearBtn: true,
            endDate: '0d',
            format: 'yyyy-mm-dd',
            todayBtn: 'linked',
            todayHighlight: true,
            weekStart: 1
        });
    }

    override render() {
        return (
            <div className="container-fluid">
                <div className="row">
                    <nav className="col-md-2 d-none d-md-block bg-light sidebar">
                        <div className="sidebar-sticky">
                            <h6 className="sidebar-heading d-flex justify-content-between align-items-center px-3 mt-4 mb-1">
                                <span>Your Workouts <span id="track-list-count"></span></span>
                                <span className="center-block loader" aria-label="Loading..." id="tracks-loader"></span>
                            </h6>
                            <div className="d-none" id="import-container">
                                <hr />
                                <p className="font-weight-bold ml-2">
                                    No workout data has been imported.
                                </p>
                                <div className="btn-group ml-2 mr-2" role="group">
                                    <button type="button" className="btn btn-primary" id="import" disabled>
                                        Import workouts
                                        <i className="bi-upload" aria-hidden="true"></i>
                                    </button>
                                </div>
                                <div className="d-none m-2 progress" id="import-loader">
                                    <div className="progress-bar progress-bar-striped active" role="progressbar" aria-valuenow={100} aria-valuemin={0} aria-valuemax={100}>
                                        <span>Importing - this make take a few minutes<span className="ellipsis" aria-hidden="true"></span></span>
                                    </div>
                                </div>
                            </div>
                            <hr />
                            <div className="sidebar-controls ml-2 mr-2">
                                <div className="input-group mb-3">
                                    <DatePicker
                                        id="not-before"
                                        label="Start date"
                                        value={this.props.startDate}
                                    />
                                </div>
                                <div className="input-group mb-3">
                                    <DatePicker
                                        id="not-after"
                                        label="End date"
                                        value={this.props.endDate}
                                    />
                                </div>
                                <div className="input-group mb-3">
                                    <div className="btn-toolbar" role="toolbar">
                                        <div className="btn-group mr-2" role="group">
                                            <button type="button" className="btn btn-primary" id="filter" disabled>
                                                Filter
                                                <i className="bi-filter" aria-hidden="true"></i>
                                            </button>
                                        </div>
                                        <div className="btn-group mr-2" role="group">
                                            <button type="button" className="btn btn-secondary" id="show-all" disabled>Show all</button>
                                        </div>
                                        <div className="btn-group" role="group">
                                            <button type="button" className="btn btn-secondary" id="hide-all" disabled>Hide all</button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <ul className="list-group flex-column mb-2" id="track-list">
                                <li className="list-group-item d-none track-item" id="track-item-template">
                                    <a className="ml-1 nav-link" href="#" role="button" aria-expanded="false">
                                        <i className="bi-geo" aria-hidden="true"></i>
                                    </a>
                                    <div className="collapse" id="">
                                        <div className="card card-body">
                                            <ul className="track-item-metadata">
                                                <li>Started: <span data-js-start></span></li>
                                                <li>Ended: <span data-js-end></span></li>
                                                <li>Duration: <span data-js-duration></span></li>
                                                <li>Distance: <span data-js-distance></span></li>
                                                <li>Average Pace: <span data-js-pace></span></li>
                                            </ul>
                                            <div className="mt-2">
                                                <button type="button" className="btn btn-secondary btn-sm float-right track-item-visibility" data-js-visible>Hide</button>
                                            </div>
                                        </div>
                                    </div>
                                </li>
                            </ul>
                        </div>
                    </nav>
                    <main role="main" className="col-md-9 ml-sm-auto col-lg-10 px-4">
                        <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
                            <h1 className="h2">
                                Map
                                <i className="bi-map" aria-hidden="true"></i>
                            </h1>
                        </div>
                        {
                            !this.props.googleMapsApiKeyValue &&
                            <NoGoogleMapsKey
                                googleMapsApiKeyName={this.props.googleMapsApiKeyName}
                                googleMapsApiKeyValue={this.props.googleMapsApiKeyValue}
                            />
                        }
                        <div className="alert alert-primary d-none" role="alert" id="total-distance-container">
                            You have travelled <span data-js-total-distance></span> <i className="bi bi-stars text-warning" aria-hidden="true"></i> &mdash;
                            that&#39;s a saving of <span data-js-emissions></span>kg of CO<sub>2</sub> compared to driving. <i className="bi bi-tree-fill text-success"></i>
                        </div>
                        <div id="map" data-google-api-key={this.props.googleMapsApiKeyValue}></div>
                        <footer className="mt-2 text-muted">
                            Designed with <i className="bi bi-heart-fill" aria-label="love" role="img"></i> by <a href="https://martincostello.com/home/about/" rel="nofollow" target="_blank">Martin Costello</a>, &copy; {new Date().getFullYear()} |
                            &nbsp;<i className="bi bi-github" aria-hidden="true"></i> <a href="https://github.com/martincostello" rel="nofollow" target="_blank">GitHub</a> |
                            &nbsp;<i className="bi bi-twitter" aria-hidden="true"></i> <a href="https://twitter.com/martin_costello" rel="nofollow" target="_blank">Twitter</a>
                        </footer>
                    </main>
                </div>
            </div>
        );
    }
}
