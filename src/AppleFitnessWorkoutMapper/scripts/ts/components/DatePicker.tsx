// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { Component } from 'react';

interface DatePickerProperties {
    id: string;
    label: string;
    value: string;
};

export class DatePicker extends Component<DatePickerProperties, {}> {
    override render() {
        return (
            <>
                <div className="input-group-prepend" aria-hidden="true">
                    <span className="input-group-text">
                        <i className="bi-calendar-event" aria-hidden="true"></i>
                    </span>
                </div>
                <input
                    id={this.props.id}
                    type="text"
                    aria-label={this.props.label}
                    autoComplete="off"
                    className="form-control calendar"
                    disabled
                    defaultValue={this.props.value}
                    placeholder={this.props.label}
                    />
            </>
        );
    }
}
