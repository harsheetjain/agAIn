import type { DjAction } from '../domain/types';

function pct(value: number | null | undefined): string {
  return `${Math.round((value ?? 0) * 100)}%`;
}

function describe(action: DjAction): string {
  switch (action.type) {
    case 'LoadTrack':
      return `load ${action.trackId} → deck ${action.deck}`;
    case 'Play':
      return `play deck ${action.deck}`;
    case 'Pause':
      return `pause deck ${action.deck}`;
    case 'Cue':
      return `cue deck ${action.deck}`;
    case 'Sync':
      return `sync deck ${action.deck}`;
    case 'SetTempo':
      return `deck ${action.deck} tempo → ${Math.round(action.value ?? 0)} BPM`;
    case 'Nudge':
      return `nudge deck ${action.deck}`;
    case 'SetVolume':
      return `deck ${action.deck} volume → ${pct(action.value)}`;
    case 'SetEq':
      return `deck ${action.deck} ${action.band} → ${pct(action.value)}`;
    case 'SetCrossfader':
      return `crossfader → ${pct(action.value)}`;
    case 'TriggerSample':
      return `sampler pad ${action.padId}`;
    default:
      return action.type;
  }
}

interface ActionFeedProps {
  feed: DjAction[];
}

export function ActionFeed({ feed }: ActionFeedProps) {
  return (
    <section className="panel feed">
      <h3>Console activity</h3>
      <ul className="feed__list">
        {feed.map((action, index) => (
          <li key={`${action.at ?? ''}-${index}`} className={`feed__item feed__item--${action.actor === 'Ai' ? 'ai' : 'human'}`}>
            <span className="badge">{action.actor === 'Ai' ? 'agAIn' : 'you'}</span>
            <span className="feed__text">{describe(action)}</span>
          </li>
        ))}
      </ul>
    </section>
  );
}
