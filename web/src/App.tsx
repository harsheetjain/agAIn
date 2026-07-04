import { ActionFeed } from './components/ActionFeed';
import { Crossfader } from './components/Crossfader';
import { DeckPanel } from './components/DeckPanel';
import { ModeBar } from './components/ModeBar';
import { SamplerPads } from './components/SamplerPads';
import { StylePanel } from './components/StylePanel';
import { Visualizer } from './components/Visualizer';
import type { DjAction } from './domain/types';
import { useDj } from './realtime/useDj';
import { usePressFx } from './realtime/usePressFx';
import './App.css';

export default function App() {
  const dj = useDj();
  const pressed = usePressFx(dj.lastAction);
  const { mixer } = dj;

  // Every human action also ensures the audio context is running so you hear it.
  const send = (action: Omit<DjAction, 'actor'>) => {
    void dj.enableAudio();
    dj.sendAction({ ...action, actor: 'Human' });
  };

  return (
    <div className="app">
      <header className="topbar">
        <div className="brand">
          ag<span className="brand__ai">AI</span>n
          <span className="brand__tag">live AI DJ · it plays itself when you step away</span>
        </div>
      </header>

      <ModeBar
        mode={mixer?.mode}
        connected={dj.connected}
        audioReady={dj.audioReady}
        lastAction={dj.lastAction}
        onEnableAudio={() => void dj.enableAudio()}
        onReleaseToAi={dj.releaseToAi}
      />

      {!mixer ? (
        <div className="loading">Connecting to the decks…</div>
      ) : (
        <main className="stage">
          <section className="decks">
            <DeckPanel
              deck={mixer.deckA}
              tracks={dj.tracks}
              pressed={pressed}
              onEq={(band, value) => send({ type: 'SetEq', deck: 'A', band, value })}
              onVolume={(value) => send({ type: 'SetVolume', deck: 'A', value })}
              onPlayPause={() => send({ type: mixer.deckA.isPlaying ? 'Pause' : 'Play', deck: 'A' })}
              onCue={() => send({ type: 'Cue', deck: 'A', value: 0 })}
              onSync={() => send({ type: 'Sync', deck: 'A' })}
              onLoad={(trackId) => send({ type: 'LoadTrack', deck: 'A', trackId })}
            />

            <Crossfader
              value={mixer.crossfader}
              pressed={pressed}
              onChange={(value) => send({ type: 'SetCrossfader', value })}
            />

            <DeckPanel
              deck={mixer.deckB}
              tracks={dj.tracks}
              pressed={pressed}
              onEq={(band, value) => send({ type: 'SetEq', deck: 'B', band, value })}
              onVolume={(value) => send({ type: 'SetVolume', deck: 'B', value })}
              onPlayPause={() => send({ type: mixer.deckB.isPlaying ? 'Pause' : 'Play', deck: 'B' })}
              onCue={() => send({ type: 'Cue', deck: 'B', value: 0 })}
              onSync={() => send({ type: 'Sync', deck: 'B' })}
              onLoad={(trackId) => send({ type: 'LoadTrack', deck: 'B', trackId })}
            />
          </section>

          <aside className="side">
            <Visualizer getAnalyser={dj.getAnalyser} />
            <SamplerPads
              pads={mixer.pads}
              pressed={pressed}
              onTrigger={(padId) => send({ type: 'TriggerSample', padId })}
            />
            <StylePanel style={dj.style} onReset={dj.resetStyle} />
            <ActionFeed feed={dj.feed} />
          </aside>
        </main>
      )}

      <footer className="footer">
        Idle for a few seconds and agAIn takes over — watch it press the buttons. It learns your style from what you
        play (derived features only; bring your own or licensed audio).
      </footer>
    </div>
  );
}
