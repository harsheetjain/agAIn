import type { DjBrain, DjIntent, DjState, Track, TrackId } from './types';

/** Keyword groups mapped to the mood tags used in the crate. */
const MOOD_WORDS: Record<string, string[]> = {
  emotional: ['emotional', 'feels', 'sad', 'heart', 'cry'],
  euphoric: ['euphoric', 'happy', 'joy', 'hands up'],
  peak: ['peak', 'drop', 'banger'],
  hard: ['hard', 'heavy', 'harder', 'aggressive'],
  chill: ['chill', 'chilled', 'mellow', 'calm', 'relax'],
  ambient: ['ambient', 'atmospheric', 'floaty', 'dreamy'],
  dancing: ['dance', 'dancing', 'groove', 'move'],
  garage: ['garage', 'ukg', 'shuffle'],
  vocal: ['vocal', 'vocals', 'voice', 'sing'],
  night: ['night', 'late', 'dark'],
};

function matchMoods(text: string): string[] {
  const found: string[] = [];
  for (const [mood, words] of Object.entries(MOOD_WORDS)) {
    if (words.some((w) => text.includes(w))) found.push(mood);
  }
  return found;
}

function pickByMood(moods: string[], tracks: Track[], excludeId?: TrackId | null): Track | null {
  const pool = tracks.filter((t) => t.id !== excludeId);
  const candidates = pool.length > 0 ? pool : tracks;
  if (candidates.length === 0) return null;

  if (moods.length === 0) {
    return candidates[Math.floor(Math.random() * candidates.length)] ?? null;
  }

  let best: Track | null = null;
  let bestScore = 0;
  for (const t of candidates) {
    const score = t.moods.filter((m) => moods.includes(m)).length;
    if (score > bestScore) {
      best = t;
      bestScore = score;
    }
  }
  return best ?? candidates[Math.floor(Math.random() * candidates.length)] ?? null;
}

/**
 * A deterministic, dependency-free DJ brain driven by keyword rules. It is the
 * default so the app runs offline with no API keys. Replace it with an
 * LLM-backed `DjBrain` (same interface) to get conversational mixing —
 * see AGENTS.md → "Swap in an LLM brain".
 */
export class RuleDjBrain implements DjBrain {
  pickTrack(moods: string[], tracks: Track[], excludeId?: TrackId | null): Track | null {
    return pickByMood(moods, tracks, excludeId);
  }

  nudge(text: string, ctx: { state: DjState; tracks: Track[] }): DjIntent {
    const t = text.toLowerCase().trim();
    const { state } = ctx;

    if (/(^|\b)(stop|pause|hold on|freeze|shush)(\b|$)/.test(t)) {
      return { action: 'pause', say: 'Pulling it back — tap me when you want it again.' };
    }
    if (/(louder|turn it up|pump it|more volume)/.test(t)) {
      return { action: 'volume', volume: clamp(state.volume + 0.15), say: 'Cranking it up.' };
    }
    if (/(quieter|softer|turn it down|lower it|less volume)/.test(t)) {
      return { action: 'volume', volume: clamp(state.volume - 0.15), say: 'Bringing the level down.' };
    }
    if (/(faster|harder|more energy|hype|turn up|go off|peak time)/.test(t)) {
      return {
        action: 'tempo',
        tempo: 'up',
        mood: ['peak', 'hard', 'euphoric'],
        say: 'Lifting the energy — here comes the peak.',
      };
    }
    if (/(slower|chill|calm|relax|come down|mellow|wind down)/.test(t)) {
      return {
        action: 'tempo',
        tempo: 'down',
        mood: ['chill', 'ambient', 'calm'],
        say: 'Cooling it right down for you.',
      };
    }
    if (/(skip|next|another|change it|different|switch)/.test(t)) {
      return { action: 'skip', mood: matchMoods(t), say: 'Say less — mixing in something new.' };
    }
    if (t.length === 0 || /(play|start|drop it|spin|resume|let'?s go|again|kick off)/.test(t)) {
      const moods = matchMoods(t);
      if (state.isPlaying && moods.length === 0) {
        return { action: 'resume', say: "We're already rolling — I got you." };
      }
      return {
        action: state.currentTrackId && moods.length === 0 ? 'resume' : 'play',
        mood: moods,
        say: moods.length
          ? `Reading the room — ${moods.join(', ')}. Dropping it now.`
          : 'Dropping something for you.',
      };
    }

    // Fallback: treat the whole nudge as a vibe request.
    const moods = matchMoods(t);
    if (moods.length > 0) {
      return { action: 'mood', mood: moods, say: `Locking into a ${moods.join(' / ')} vibe.` };
    }
    return { action: 'mood', mood: [], say: "Not sure I caught that — here's a fresh one anyway." };
  }
}

function clamp(v: number): number {
  return Math.min(1, Math.max(0, v));
}
