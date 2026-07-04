export type TrackId = string;

export interface Track {
  id: TrackId;
  title: string;
  artist: string;
  /** Beats per minute — drives tempo-aware nudges and the synth groove. */
  bpm: number;
  /** Musical key, e.g. "A minor". Used to pitch the synthesized demo loop. */
  key: string;
  /** Vibe tags the DJ brain matches free-text nudges against. */
  moods: string[];
  /**
   * Optional URL to an audio file. When omitted the AudioEngine synthesizes a
   * demo loop, so the app runs with zero bundled (and zero copyrighted) audio.
   */
  src?: string;
}

export type NudgeRole = 'you' | 'dj';

export interface NudgeMessage {
  id: string;
  role: NudgeRole;
  text: string;
  /** Epoch milliseconds. */
  at: number;
}

export type DjAction =
  | 'play'
  | 'pause'
  | 'resume'
  | 'skip'
  | 'tempo'
  | 'mood'
  | 'volume'
  | 'unknown';

/** A structured intent parsed from a free-text nudge. */
export interface DjIntent {
  action: DjAction;
  /** Matched vibe tags used to pick the next track. */
  mood?: string[];
  /** For `tempo` nudges. */
  tempo?: 'up' | 'down';
  /** For `volume` nudges, in the range 0..1. */
  volume?: number;
  /** Human-readable line the DJ "speaks" back to the user. */
  say: string;
}

export interface DjState {
  currentTrackId: TrackId | null;
  isPlaying: boolean;
  volume: number;
}

/**
 * Turns a free-text "nudge" into a structured {@link DjIntent}, and picks
 * tracks from the crate. Swap the rule-based implementation for an LLM-backed
 * one without touching the UI — see AGENTS.md.
 */
export interface DjBrain {
  nudge(text: string, ctx: { state: DjState; tracks: Track[] }): DjIntent;
  pickTrack(moods: string[], tracks: Track[], excludeId?: TrackId | null): Track | null;
}
