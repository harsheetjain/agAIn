import type { Track } from './types';

/**
 * A small demo "crate". Titles are original — no copyrighted tracks are
 * referenced or bundled. Each entry has no `src`, so the AudioEngine
 * synthesizes an audible groove from its bpm/key.
 *
 * Add real tracks by supplying a `src` URL (see AGENTS.md → "Add real music").
 */
export const CRATE: Track[] = [
  {
    id: 'dawn-chorus',
    title: 'Dawn Chorus',
    artist: 'agAIn',
    bpm: 120,
    key: 'A minor',
    moods: ['emotional', 'uplifting', 'vocal', 'house'],
  },
  {
    id: 'ravehold',
    title: 'Ravehold',
    artist: 'agAIn',
    bpm: 126,
    key: 'F minor',
    moods: ['euphoric', 'peak', 'dancing', 'build'],
  },
  {
    id: 'concrete',
    title: 'Concrete',
    artist: 'agAIn',
    bpm: 140,
    key: 'E minor',
    moods: ['hard', 'peak', 'energy', 'bass'],
  },
  {
    id: 'petrichor',
    title: 'Petrichor',
    artist: 'agAIn',
    bpm: 110,
    key: 'C minor',
    moods: ['chill', 'emotional', 'downtempo', 'ambient'],
  },
  {
    id: 'streetlight',
    title: 'Streetlight',
    artist: 'agAIn',
    bpm: 132,
    key: 'G minor',
    moods: ['garage', 'dancing', 'bouncy', 'night'],
  },
  {
    id: 'afterglow',
    title: 'Afterglow',
    artist: 'agAIn',
    bpm: 100,
    key: 'D minor',
    moods: ['ambient', 'calm', 'reflective', 'warmup'],
  },
];
