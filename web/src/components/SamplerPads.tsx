import type { Actor, SamplerPad } from '../domain/types';
import { pressClass } from '../realtime/usePressFx';

interface SamplerPadsProps {
  pads: SamplerPad[];
  pressed: Map<string, Actor>;
  onTrigger: (padId: number) => void;
}

export function SamplerPads({ pads, pressed, onTrigger }: SamplerPadsProps) {
  return (
    <section className="panel">
      <h3>Sampler</h3>
      <div className="pads">
        {pads.map((pad) => {
          const actor = pressed.get(`pad-${pad.id}`) ?? (pad.active ? ('Ai' as Actor) : undefined);
          return (
            <button
              key={pad.id}
              type="button"
              className={pressClass('pad', actor)}
              onClick={() => onTrigger(pad.id)}
            >
              <span className="pad__num">{pad.id}</span>
              <span className="pad__label">{pad.label}</span>
            </button>
          );
        })}
      </div>
    </section>
  );
}
