import type { DjAction, SessionMode } from '../domain/types';

interface ModeBarProps {
  mode: SessionMode | undefined;
  connected: boolean;
  audioReady: boolean;
  lastAction: DjAction | null;
  onEnableAudio: () => void;
  onReleaseToAi: () => void;
}

export function ModeBar({ mode, connected, audioReady, lastAction, onEnableAudio, onReleaseToAi }: ModeBarProps) {
  const autonomous = mode === 'Autonomous';
  return (
    <div className="modebar">
      <span className={`dot ${connected ? 'dot--on' : 'dot--off'}`} title={connected ? 'Connected' : 'Offline'} />
      <span className={`mode ${autonomous ? 'mode--ai' : 'mode--you'}`}>
        {autonomous ? '🤖 agAIn is playing' : '🎛 You’re in control'}
      </span>
      <span className="modebar__note">{lastAction?.note ?? 'reading the room…'}</span>
      <span className="modebar__spacer" />
      {!audioReady && (
        <button type="button" className="btn btn--primary" onClick={onEnableAudio}>
          ▶ Enable sound
        </button>
      )}
      {mode === 'Human' && (
        <button type="button" className="btn" onClick={onReleaseToAi}>
          Let agAIn take over
        </button>
      )}
    </div>
  );
}
