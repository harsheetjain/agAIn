import type { Track } from '../dj/types';
import { Visualizer } from './Visualizer';

interface DeckProps {
  track: Track | null;
  isPlaying: boolean;
  volume: number;
  getAnalyser: () => AnalyserNode | null;
  onPlayPause: () => void;
  onSkip: () => void;
  onVolume: (v: number) => void;
}

/** The turntable: now-playing info, visualizer and transport controls. */
export function Deck({
  track,
  isPlaying,
  volume,
  getAnalyser,
  onPlayPause,
  onSkip,
  onVolume,
}: DeckProps) {
  return (
    <section className="deck" aria-label="DJ deck">
      <div className={isPlaying ? 'platter platter--spin' : 'platter'} aria-hidden="true">
        <div className="platter__ring" />
        <div className="platter__label">agAIn</div>
      </div>

      <div className="deck__body">
        <div className="now-playing">
          <span className="now-playing__eyebrow">{isPlaying ? 'NOW PLAYING' : 'CUED'}</span>
          <h2 className="now-playing__title">{track ? track.title : 'Nothing cued yet'}</h2>
          <p className="now-playing__meta">
            {track
              ? `${track.artist} · ${track.bpm} BPM · ${track.key}`
              : 'Nudge me to start the set'}
          </p>
          {track ? (
            <ul className="tags">
              {track.moods.map((m) => (
                <li key={m} className="tag">
                  {m}
                </li>
              ))}
            </ul>
          ) : null}
        </div>

        <Visualizer getAnalyser={getAnalyser} active={isPlaying} />

        <div className="transport">
          <button type="button" className="btn btn--primary" onClick={onPlayPause}>
            {isPlaying ? '⏸ Pause' : '▶ Play'}
          </button>
          <button type="button" className="btn" onClick={onSkip}>
            ⏭ Skip
          </button>
          <label className="volume">
            <span aria-hidden="true">🔊</span>
            <input
              type="range"
              min={0}
              max={1}
              step={0.01}
              value={volume}
              onChange={(e) => onVolume(Number(e.target.value))}
              aria-label="Volume"
            />
          </label>
        </div>
      </div>
    </section>
  );
}
