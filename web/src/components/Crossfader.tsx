import type { Actor } from '../domain/types';
import { pressClass } from '../realtime/usePressFx';

interface CrossfaderProps {
  value: number;
  pressed: Map<string, Actor>;
  onChange: (value: number) => void;
}

export function Crossfader({ value, pressed, onChange }: CrossfaderProps) {
  return (
    <div className={pressClass('crossfader', pressed.get('xfader'))}>
      <span className="crossfader__label">A</span>
      <input
        type="range"
        min={0}
        max={1}
        step={0.01}
        value={value}
        onChange={(e) => onChange(Number(e.target.value))}
        aria-label="Crossfader"
      />
      <span className="crossfader__label">B</span>
    </div>
  );
}
