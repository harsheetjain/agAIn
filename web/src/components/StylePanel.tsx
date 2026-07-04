import type { StyleSnapshot } from '../domain/types';

interface StylePanelProps {
  style: StyleSnapshot | null;
  onReset: () => void;
}

function barStyle(value: number): { width: string } {
  return { width: `${Math.round(Math.min(1, Math.max(0, value)) * 100)}%` };
}

function Metric({ label, value, display }: { label: string; value: number; display: string }) {
  return (
    <div className="metric">
      <span>{label}</span>
      <div className="meter">
        <i style={barStyle(value)} />
      </div>
      <b>{display}</b>
    </div>
  );
}

export function StylePanel({ style, onReset }: StylePanelProps) {
  if (!style) {
    return (
      <section className="panel">
        <h3>Learned style</h3>
        <p className="muted">Connecting…</p>
      </section>
    );
  }

  const moods = Object.entries(style.moodWeights)
    .sort((a, b) => b[1] - a[1])
    .slice(0, 5);

  return (
    <section className="panel">
      <header className="panel__head">
        <h3>Learned style</h3>
        <span className="chip">{style.samples} samples</span>
      </header>
      <Metric label="Energy target" value={style.energyTarget} display={`${Math.round(style.energyTarget * 100)}%`} />
      <Metric
        label="Harmonic affinity"
        value={style.harmonicAffinity}
        display={`${Math.round(style.harmonicAffinity * 100)}%`}
      />
      <div className="metric metric--plain">
        <span>Tempo centre</span>
        <b>{Math.round(style.tempoCenter)} BPM</b>
      </div>
      <div className="metric metric--plain">
        <span>Transition every</span>
        <b>{Math.round(style.meanTransitionSeconds)}s</b>
      </div>
      {moods.length > 0 && (
        <div className="moods">
          {moods.map(([mood]) => (
            <span key={mood} className="chip">
              {mood}
            </span>
          ))}
        </div>
      )}
      <button type="button" className="btn btn--ghost" onClick={onReset}>
        Reset learning
      </button>
    </section>
  );
}
