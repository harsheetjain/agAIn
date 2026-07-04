import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { AudioEngine } from './audio/AudioEngine';
import { RuleDjBrain } from './dj/djBrain';
import { CRATE } from './dj/tracks';
import type { NudgeMessage, NudgeRole, Track } from './dj/types';
import { Deck } from './components/Deck';
import { NudgeConsole } from './components/NudgeConsole';
import './App.css';

let messageSeq = 0;
function makeMessage(role: NudgeRole, text: string): NudgeMessage {
  messageSeq += 1;
  return { id: `m${messageSeq}`, role, text, at: Date.now() };
}

export default function App() {
  const engineRef = useRef<AudioEngine | null>(null);
  const brain = useMemo(() => new RuleDjBrain(), []);
  const tracks = CRATE;

  const [current, setCurrent] = useState<Track | null>(null);
  const [isPlaying, setIsPlaying] = useState(false);
  const [volume, setVolume] = useState(0.8);
  const [messages, setMessages] = useState<NudgeMessage[]>([]);

  const getEngine = useCallback(() => {
    if (!engineRef.current) {
      engineRef.current = new AudioEngine();
    }
    return engineRef.current;
  }, []);

  useEffect(() => () => engineRef.current?.dispose(), []);

  const getAnalyser = useCallback(() => engineRef.current?.getAnalyser() ?? null, []);

  const say = useCallback((role: NudgeRole, text: string) => {
    setMessages((prev) => [...prev, makeMessage(role, text)]);
  }, []);

  const playTrack = useCallback(
    async (track: Track) => {
      const engine = getEngine();
      engine.setTrack(track.bpm, track.key);
      engine.setVolume(volume);
      setCurrent(track);
      await engine.start();
      setIsPlaying(true);
    },
    [getEngine, volume],
  );

  const handleNudge = useCallback(
    async (text: string) => {
      say('you', text);
      const intent = brain.nudge(text, {
        state: { currentTrackId: current?.id ?? null, isPlaying, volume },
        tracks,
      });
      say('dj', intent.say);

      const engine = getEngine();
      switch (intent.action) {
        case 'pause':
          engine.stop();
          setIsPlaying(false);
          break;
        case 'resume':
          if (current) {
            await engine.start();
            setIsPlaying(true);
          } else {
            const next = brain.pickTrack(intent.mood ?? [], tracks, null);
            if (next) await playTrack(next);
          }
          break;
        case 'volume':
          if (typeof intent.volume === 'number') {
            setVolume(intent.volume);
            engine.setVolume(intent.volume);
          }
          break;
        case 'skip': {
          const next = brain.pickTrack(intent.mood ?? [], tracks, current?.id ?? null);
          if (next) await playTrack(next);
          break;
        }
        case 'play':
        case 'tempo':
        case 'mood': {
          const exclude = intent.action === 'play' ? null : (current?.id ?? null);
          const next = brain.pickTrack(intent.mood ?? [], tracks, exclude);
          if (next) await playTrack(next);
          break;
        }
        default:
          break;
      }
    },
    [brain, current, getEngine, isPlaying, playTrack, say, tracks, volume],
  );

  const handlePlayPause = useCallback(async () => {
    const engine = getEngine();
    if (isPlaying) {
      engine.stop();
      setIsPlaying(false);
      return;
    }
    if (current) {
      await engine.start();
      setIsPlaying(true);
    } else {
      const next = brain.pickTrack([], tracks, null);
      if (next) await playTrack(next);
    }
  }, [brain, current, getEngine, isPlaying, playTrack, tracks]);

  const handleSkip = useCallback(async () => {
    const next = brain.pickTrack([], tracks, current?.id ?? null);
    if (next) {
      await playTrack(next);
      say('dj', `Mixing in ${next.title}.`);
    }
  }, [brain, current, playTrack, say, tracks]);

  const handleVolume = useCallback(
    (v: number) => {
      setVolume(v);
      getEngine().setVolume(v);
    },
    [getEngine],
  );

  return (
    <div className="app">
      <header className="topbar">
        <div className="brand">
          <span className="brand__mark">
            ag<span className="brand__ai">AI</span>n
          </span>
          <span className="brand__tag">your interactive AI DJ</span>
        </div>
        <a
          className="brand__repo"
          href="https://github.com/harsheetjain/agAIn"
          target="_blank"
          rel="noreferrer"
        >
          ★ GitHub
        </a>
      </header>

      <main className="stage">
        <Deck
          track={current}
          isPlaying={isPlaying}
          volume={volume}
          getAnalyser={getAnalyser}
          onPlayPause={handlePlayPause}
          onSkip={handleSkip}
          onVolume={handleVolume}
        />
        <NudgeConsole messages={messages} onNudge={handleNudge} />
      </main>

      <footer className="footer">
        <span>Rule-based DJ brain — swap in an LLM to make it conversational (see AGENTS.md).</span>
      </footer>
    </div>
  );
}
