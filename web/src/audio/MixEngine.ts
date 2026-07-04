import type { DeckState, MixerState } from '../domain/types';
import { DeckVoice } from './DeckVoice';

interface DeckChain {
  voice: DeckVoice;
  low: BiquadFilterNode;
  mid: BiquadFilterNode;
  high: BiquadFilterNode;
  channel: GainNode;
  xfade: GainNode;
}

const EQ_RANGE_DB = 24;

function eqDb(value: number): number {
  return (Math.min(1, Math.max(0, value)) - 0.5) * 2 * EQ_RANGE_DB;
}

/**
 * Turns the authoritative {@link MixerState} into sound: two synth decks routed
 * through per-deck 3-band EQ, channel faders and an equal-power crossfader into
 * a master bus + analyser (which the FeatureSampler listens to for training).
 */
export class MixEngine {
  private readonly ctx: AudioContext;
  private readonly master: GainNode;
  private readonly analyser: AnalyserNode;
  private readonly deckA: DeckChain;
  private readonly deckB: DeckChain;

  constructor(ctx: AudioContext) {
    this.ctx = ctx;
    this.master = ctx.createGain();
    this.master.gain.value = 0.85;
    this.analyser = ctx.createAnalyser();
    this.analyser.fftSize = 1024;
    this.analyser.smoothingTimeConstant = 0.8;
    this.master.connect(this.analyser);
    this.analyser.connect(ctx.destination);
    this.deckA = this.buildChain();
    this.deckB = this.buildChain();
  }

  get analyserNode(): AnalyserNode {
    return this.analyser;
  }

  get context(): AudioContext {
    return this.ctx;
  }

  async resume(): Promise<void> {
    if (this.ctx.state === 'suspended') {
      await this.ctx.resume();
    }
  }

  applyState(state: MixerState): void {
    this.master.gain.setTargetAtTime(state.masterVolume, this.ctx.currentTime, 0.02);
    const x = Math.min(1, Math.max(0, state.crossfader));
    this.applyDeck(this.deckA, state.deckA, Math.cos((x * Math.PI) / 2));
    this.applyDeck(this.deckB, state.deckB, Math.sin((x * Math.PI) / 2));
  }

  dispose(): void {
    this.deckA.voice.stop();
    this.deckB.voice.stop();
    void this.ctx.close();
  }

  private applyDeck(chain: DeckChain, deck: DeckState, xfade: number): void {
    chain.voice.setTrack(deck.tempo, deck.track?.key ?? '8A');
    if (deck.isPlaying && !chain.voice.isRunning) {
      chain.voice.start();
    } else if (!deck.isPlaying && chain.voice.isRunning) {
      chain.voice.stop();
    }

    const now = this.ctx.currentTime;
    chain.channel.gain.setTargetAtTime(deck.volume, now, 0.02);
    chain.xfade.gain.setTargetAtTime(xfade, now, 0.03);
    chain.low.gain.setTargetAtTime(eqDb(deck.eq.low), now, 0.03);
    chain.mid.gain.setTargetAtTime(eqDb(deck.eq.mid), now, 0.03);
    chain.high.gain.setTargetAtTime(eqDb(deck.eq.high), now, 0.03);
  }

  private buildChain(): DeckChain {
    const low = this.ctx.createBiquadFilter();
    low.type = 'lowshelf';
    low.frequency.value = 220;

    const mid = this.ctx.createBiquadFilter();
    mid.type = 'peaking';
    mid.frequency.value = 1000;
    mid.Q.value = 1;

    const high = this.ctx.createBiquadFilter();
    high.type = 'highshelf';
    high.frequency.value = 4200;

    const channel = this.ctx.createGain();
    const xfade = this.ctx.createGain();

    low.connect(mid).connect(high).connect(channel).connect(xfade).connect(this.master);
    const voice = new DeckVoice(this.ctx, low);
    return { voice, low, mid, high, channel, xfade };
  }
}
