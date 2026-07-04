import type { Actor, DeckState, EqBand, Track } from '../domain/types';
import { pressClass } from '../realtime/usePressFx';

interface DeckPanelProps {
  deck: DeckState;
  tracks: Track[];
  pressed: Map<string, Actor>;
  onEq: (band: EqBand, value: number) => void;
  onVolume: (value: number) => void;
  onPlayPause: () => void;
  onCue: () => void;
  onSync: () => void;
  onLoad: (trackId: string) => void;
}

const BANDS: EqBand[] = ['High', 'Mid', 'Low'];

function bandValue(deck: DeckState, band: EqBand): number {
  switch (band) {
    case 'Low':
      return deck.eq.low;
    case 'Mid':
      return deck.eq.mid;
    case 'High':
      return deck.eq.high;
    default:
      return 0.5;
  }
}

export function DeckPanel({ deck, tracks, pressed, onEq, onVolume, onPlayPause, onCue, onSync, onLoad }: DeckPanelProps) {
  const id = deck.id;
  const track = deck.track;

  return (
    <section className={pressClass(`deck deck--${id.toLowerCase()}`, pressed.get(`deck-${id}`))}>
      <div className="deck__top">
        <div className={deck.isPlaying ? 'platter platter--spin' : 'platter'} aria-hidden="true">
          <span>{id}</span>
        </div>
        <div className="deck__info">
          <span className="eyebrow">
            DECK {id} · {deck.isPlaying ? 'PLAYING' : 'CUED'}
          </span>
          <h2>{track ? track.title : '—'}</h2>
          <p className="muted">
            {track ? `${track.artist} · ${Math.round(deck.tempo)} BPM · ${track.key}` : 'no track loaded'}
          </p>
          <div className="progress">
            <i style={{ width: `${Math.round(deck.progressFraction * 100)}%` }} />
          </div>
        </div>
      </div>

      <div className="eq">
        {BANDS.map((band) => (
          <label key={band} className={pressClass('slider', pressed.get(`eq-${id}-${band}`))}>
            <span>{band[0]}</span>
            <input
              type="range"
              min={0}
              max={1}
              step={0.01}
              value={bandValue(deck, band)}
              onChange={(e) => onEq(band, Number(e.target.value))}
              aria-label={`Deck ${id} ${band} EQ`}
            />
          </label>
        ))}
        <label className={pressClass('slider', pressed.get(`vol-${id}`))}>
          <span>V</span>
          <input
            type="range"
            min={0}
            max={1}
            step={0.01}
            value={deck.volume}
            onChange={(e) => onVolume(Number(e.target.value))}
            aria-label={`Deck ${id} volume`}
          />
        </label>
      </div>

      <div className="deck__buttons">
        <button
          type="button"
          className={pressClass('btn btn--primary', pressed.get(`play-${id}`))}
          onClick={onPlayPause}
        >
          {deck.isPlaying ? '⏸ Pause' : '▶ Play'}
        </button>
        <button type="button" className={pressClass('btn', pressed.get(`cue-${id}`))} onClick={onCue}>
          Cue
        </button>
        <button type="button" className={pressClass('btn', pressed.get(`sync-${id}`))} onClick={onSync}>
          Sync
        </button>
        <select
          className="deck__load"
          value={track?.id ?? ''}
          onChange={(e) => onLoad(e.target.value)}
          aria-label={`Load a track on deck ${id}`}
        >
          <option value="" disabled>
            load…
          </option>
          {tracks.map((option) => (
            <option key={option.id} value={option.id}>
              {option.title} · {option.key}
            </option>
          ))}
        </select>
      </div>
    </section>
  );
}
