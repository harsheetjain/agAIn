import { useEffect, useState } from 'react';
import type { Actor, DjAction } from '../domain/types';

/** Maps an action to the DOM control id it should visually "press". */
export function pressIdFor(action: DjAction): string | null {
  switch (action.type) {
    case 'SetCrossfader':
      return 'xfader';
    case 'SetEq':
      return action.deck && action.band ? `eq-${action.deck}-${action.band}` : null;
    case 'SetVolume':
      return action.deck ? `vol-${action.deck}` : null;
    case 'SetTempo':
    case 'Nudge':
      return action.deck ? `tempo-${action.deck}` : null;
    case 'Sync':
      return action.deck ? `sync-${action.deck}` : null;
    case 'Play':
    case 'Pause':
      return action.deck ? `play-${action.deck}` : null;
    case 'Cue':
      return action.deck ? `cue-${action.deck}` : null;
    case 'LoadTrack':
      return action.deck ? `deck-${action.deck}` : null;
    case 'TriggerSample':
      return action.padId != null ? `pad-${action.padId}` : null;
    default:
      return null;
  }
}

/**
 * Tracks which controls are currently "pressed" (by the AI or the human) so the
 * UI can flash them — this is what makes the autonomous set feel alive.
 */
export function usePressFx(lastAction: DjAction | null, holdMs = 320): Map<string, Actor> {
  const [pressed, setPressed] = useState<Map<string, Actor>>(new Map());

  useEffect(() => {
    if (!lastAction) {
      return;
    }
    const id = pressIdFor(lastAction);
    if (!id) {
      return;
    }

    setPressed((prev) => new Map(prev).set(id, lastAction.actor));
    const timer = window.setTimeout(() => {
      setPressed((prev) => {
        const next = new Map(prev);
        next.delete(id);
        return next;
      });
    }, holdMs);

    return () => window.clearTimeout(timer);
  }, [lastAction, holdMs]);

  return pressed;
}

/** Adds pressed/actor classes to a base className when a control is active. */
export function pressClass(base: string, actor: Actor | undefined): string {
  if (!actor) {
    return base;
  }
  return `${base} pressed pressed--${actor === 'Ai' ? 'ai' : 'human'}`;
}
