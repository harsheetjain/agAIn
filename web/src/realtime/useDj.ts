import { useCallback, useEffect, useRef, useState } from 'react';
import { FeatureSampler } from '../audio/FeatureSampler';
import { MixEngine } from '../audio/MixEngine';
import type { DjAction, MixerState, StyleSnapshot, Track } from '../domain/types';
import { DjClient } from './DjClient';

const FEED_LIMIT = 60;

export interface UseDj {
  connected: boolean;
  audioReady: boolean;
  mixer: MixerState | null;
  style: StyleSnapshot | null;
  tracks: Track[];
  feed: DjAction[];
  lastAction: DjAction | null;
  getAnalyser: () => AnalyserNode | null;
  enableAudio: () => Promise<void>;
  sendAction: (action: DjAction) => void;
  releaseToAi: () => void;
  resetStyle: () => void;
}

/**
 * Owns the SignalR connection, the audio engine and the listening loop, and
 * exposes the console state to React. The server is the brain; this hook renders
 * its state, plays the resulting mix, streams audio features back for training,
 * and forwards the human's console actions.
 */
export function useDj(): UseDj {
  const clientRef = useRef<DjClient | null>(null);
  const engineRef = useRef<MixEngine | null>(null);
  const samplerRef = useRef<FeatureSampler | null>(null);
  const stateRef = useRef<MixerState | null>(null);

  const [connected, setConnected] = useState(false);
  const [audioReady, setAudioReady] = useState(false);
  const [mixer, setMixer] = useState<MixerState | null>(null);
  const [style, setStyle] = useState<StyleSnapshot | null>(null);
  const [tracks, setTracks] = useState<Track[]>([]);
  const [feed, setFeed] = useState<DjAction[]>([]);
  const [lastAction, setLastAction] = useState<DjAction | null>(null);

  useEffect(() => {
    const client = new DjClient({
      onConnectionChange: setConnected,
      onState: (state) => {
        stateRef.current = state;
        setMixer(state);
        engineRef.current?.applyState(state);
      },
      onStyle: setStyle,
      onAction: (action) => {
        setLastAction(action);
        setFeed((prev) => [action, ...prev].slice(0, FEED_LIMIT));
      },
    });
    clientRef.current = client;
    void client.start().catch((error) => console.error('DJ hub connection failed', error));

    void fetch('/api/tracks')
      .then((response) => response.json())
      .then((data: Track[]) => setTracks(data))
      .catch(() => undefined);

    return () => {
      samplerRef.current?.stop();
      engineRef.current?.dispose();
      void client.stop().catch(() => undefined);
    };
  }, []);

  const enableAudio = useCallback(async () => {
    if (engineRef.current) {
      await engineRef.current.resume();
      return;
    }

    const engine = new MixEngine(new AudioContext());
    engineRef.current = engine;
    await engine.resume();
    if (stateRef.current) {
      engine.applyState(stateRef.current);
    }

    const sampler = new FeatureSampler(
      engine.context,
      engine.analyserNode,
      (frame) => clientRef.current?.sendFeatureFrame(frame).catch(() => undefined),
      () => {
        const state = stateRef.current;
        if (!state) {
          return 0;
        }
        const live = state.crossfader <= 0.5 ? state.deckA : state.deckB;
        return live.isPlaying ? live.tempo : 0;
      },
    );
    sampler.start();
    samplerRef.current = sampler;
    setAudioReady(true);
  }, []);

  const getAnalyser = useCallback(() => engineRef.current?.analyserNode ?? null, []);

  const sendAction = useCallback((action: DjAction) => {
    clientRef.current?.sendAction(action).catch(() => undefined);
  }, []);

  const releaseToAi = useCallback(() => {
    clientRef.current?.releaseToAi().catch(() => undefined);
  }, []);

  const resetStyle = useCallback(() => {
    clientRef.current?.resetStyle().catch(() => undefined);
  }, []);

  return {
    connected,
    audioReady,
    mixer,
    style,
    tracks,
    feed,
    lastAction,
    getAnalyser,
    enableAudio,
    sendAction,
    releaseToAi,
    resetStyle,
  };
}
