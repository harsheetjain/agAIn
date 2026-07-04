// TypeScript mirror of the .NET domain contracts (camelCase over the wire).

export type DeckId = 'A' | 'B';
export type EqBand = 'Low' | 'Mid' | 'High';
export type Actor = 'Human' | 'Ai';
export type SessionMode = 'Human' | 'Autonomous';

export type DjActionType =
  | 'LoadTrack'
  | 'Play'
  | 'Pause'
  | 'Cue'
  | 'SetTempo'
  | 'Nudge'
  | 'SetVolume'
  | 'SetEq'
  | 'SetCrossfader'
  | 'TriggerSample'
  | 'StartTransition'
  | 'Sync';

export interface Track {
  id: string;
  title: string;
  artist: string;
  bpm: number;
  key: string;
  energy: number;
  moods: string[];
  src?: string | null;
  durationSeconds: number;
}

export interface EqSettings {
  low: number;
  mid: number;
  high: number;
}

export interface DeckState {
  id: DeckId;
  track: Track | null;
  isPlaying: boolean;
  positionSeconds: number;
  tempo: number;
  volume: number;
  eq: EqSettings;
  progressFraction: number;
  secondsRemaining: number;
}

export interface SamplerPad {
  id: number;
  label: string;
  active: boolean;
}

export interface MixerState {
  deckA: DeckState;
  deckB: DeckState;
  crossfader: number;
  masterVolume: number;
  mode: SessionMode;
  pads: SamplerPad[];
}

export interface DjAction {
  type: DjActionType;
  actor: Actor;
  deck?: DeckId | null;
  band?: EqBand | null;
  value?: number | null;
  trackId?: string | null;
  padId?: number | null;
  note?: string | null;
  at?: string;
}

export interface StyleSnapshot {
  energyTarget: number;
  tempoCenter: number;
  meanTransitionSeconds: number;
  harmonicAffinity: number;
  moodWeights: Record<string, number>;
  samples: number;
}

export interface AudioFeatureFrame {
  timestampSeconds: number;
  rms: number;
  spectralCentroid: number;
  tempo: number;
  chroma: number[];
}
