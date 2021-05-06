import { TrackPoint } from './TrackPoint';

export interface Track {
    timestamp: string;
    segments: TrackPoint[][];
}
