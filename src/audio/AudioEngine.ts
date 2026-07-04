/**
 * A tiny Web Audio "instrument". Because the demo crate ships no audio files,
 * this engine synthesizes an audible four-on-the-floor groove (kick, hats,
 * bass arpeggio, pad) pitched to the track's key and locked to its BPM.
 *
 * It exposes an AnalyserNode so the UI can draw a live visualizer. When you add
 * real tracks (a `src` URL), replace the synth in {@link AudioEngine.scheduleStep}
 * with a buffer/`<audio>` source — see AGENTS.md.
 */
const LOOKAHEAD_MS = 25;
const SCHEDULE_AHEAD_S = 0.12;
const STEPS_PER_BAR = 16;

/** Semitone offsets of a natural minor scale. */
const MINOR_SCALE = [0, 2, 3, 5, 7, 8, 10];

const NOTE_INDEX: Record<string, number> = {
  c: 0, 'c#': 1, db: 1, d: 2, 'd#': 3, eb: 3, e: 4, f: 5,
  'f#': 6, gb: 6, g: 7, 'g#': 8, ab: 8, a: 9, 'a#': 10, bb: 10, b: 11,
};

function rootMidiFromKey(key: string): number {
  const token = key.trim().toLowerCase().split(/\s+/)[0] ?? 'a';
  const semitone = NOTE_INDEX[token] ?? 9; // default to A
  return 33 + semitone; // ~A1 region for a fat bass
}

function midiToFreq(midi: number): number {
  return 440 * Math.pow(2, (midi - 69) / 12);
}

export class AudioEngine {
  private ctx: AudioContext | null = null;
  private master: GainNode | null = null;
  private analyser: AnalyserNode | null = null;
  private noise: AudioBuffer | null = null;
  private timer: number | null = null;
  private step = 0;
  private nextNoteTime = 0;
  private bpm = 122;
  private rootNote = 42;
  private volume = 0.8;

  get isRunning(): boolean {
    return this.timer !== null;
  }

  getAnalyser(): AnalyserNode | null {
    return this.analyser;
  }

  setTrack(bpm: number, key: string): void {
    this.bpm = bpm;
    this.rootNote = rootMidiFromKey(key);
  }

  setVolume(v: number): void {
    this.volume = Math.min(1, Math.max(0, v));
    if (this.master && this.ctx) {
      this.master.gain.setTargetAtTime(this.volume, this.ctx.currentTime, 0.02);
    }
  }

  /** Must be called from a user gesture (autoplay policy). Starts the loop. */
  async start(): Promise<void> {
    const ctx = this.ensureContext();
    if (ctx.state === 'suspended') {
      await ctx.resume();
    }
    if (this.timer !== null) return;
    this.step = 0;
    this.nextNoteTime = ctx.currentTime + 0.05;
    this.timer = window.setInterval(() => this.scheduler(), LOOKAHEAD_MS);
  }

  stop(): void {
    if (this.timer !== null) {
      window.clearInterval(this.timer);
      this.timer = null;
    }
  }

  dispose(): void {
    this.stop();
    if (this.ctx) {
      void this.ctx.close();
      this.ctx = null;
      this.master = null;
      this.analyser = null;
      this.noise = null;
    }
  }

  private ensureContext(): AudioContext {
    if (this.ctx) return this.ctx;

    const ctx = new AudioContext();
    const master = ctx.createGain();
    master.gain.value = this.volume;

    const analyser = ctx.createAnalyser();
    analyser.fftSize = 256;
    analyser.smoothingTimeConstant = 0.8;

    master.connect(analyser);
    analyser.connect(ctx.destination);

    // Pre-baked white-noise buffer, reused for every hi-hat.
    const noise = ctx.createBuffer(1, Math.floor(ctx.sampleRate * 0.4), ctx.sampleRate);
    const data = noise.getChannelData(0);
    for (let i = 0; i < data.length; i += 1) {
      data[i] = Math.random() * 2 - 1;
    }

    this.ctx = ctx;
    this.master = master;
    this.analyser = analyser;
    this.noise = noise;
    return ctx;
  }

  private scheduler(): void {
    const ctx = this.ctx;
    if (!ctx) return;
    const secondsPerStep = 60 / this.bpm / 4; // sixteenth notes
    while (this.nextNoteTime < ctx.currentTime + SCHEDULE_AHEAD_S) {
      this.scheduleStep(this.step, this.nextNoteTime);
      this.nextNoteTime += secondsPerStep;
      this.step = (this.step + 1) % STEPS_PER_BAR;
    }
  }

  private scheduleStep(step: number, time: number): void {
    if (step % 4 === 0) this.kick(time);
    if (step % 2 === 1) this.hat(time, 0.1);
    if (step % 4 === 2) this.hat(time, 0.16);
    if (step % 2 === 0) {
      const degree = MINOR_SCALE[(step / 2) % MINOR_SCALE.length] ?? 0;
      this.bass(this.rootNote + degree, time);
    }
    if (step === 0) this.pad(time);
  }

  private kick(time: number): void {
    const ctx = this.ctx;
    const master = this.master;
    if (!ctx || !master) return;
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();
    osc.frequency.setValueAtTime(140, time);
    osc.frequency.exponentialRampToValueAtTime(48, time + 0.12);
    gain.gain.setValueAtTime(1, time);
    gain.gain.exponentialRampToValueAtTime(0.001, time + 0.28);
    osc.connect(gain).connect(master);
    osc.start(time);
    osc.stop(time + 0.3);
  }

  private hat(time: number, level: number): void {
    const ctx = this.ctx;
    const master = this.master;
    if (!ctx || !master || !this.noise) return;
    const src = ctx.createBufferSource();
    src.buffer = this.noise;
    const hp = ctx.createBiquadFilter();
    hp.type = 'highpass';
    hp.frequency.value = 7000;
    const gain = ctx.createGain();
    gain.gain.setValueAtTime(level, time);
    gain.gain.exponentialRampToValueAtTime(0.001, time + 0.05);
    src.connect(hp).connect(gain).connect(master);
    src.start(time);
    src.stop(time + 0.06);
  }

  private bass(midi: number, time: number): void {
    const ctx = this.ctx;
    const master = this.master;
    if (!ctx || !master) return;
    const osc = ctx.createOscillator();
    osc.type = 'sawtooth';
    osc.frequency.value = midiToFreq(midi);
    const lp = ctx.createBiquadFilter();
    lp.type = 'lowpass';
    lp.frequency.value = 620;
    const gain = ctx.createGain();
    gain.gain.setValueAtTime(0.0001, time);
    gain.gain.linearRampToValueAtTime(0.32, time + 0.01);
    gain.gain.exponentialRampToValueAtTime(0.001, time + 0.18);
    osc.connect(lp).connect(gain).connect(master);
    osc.start(time);
    osc.stop(time + 0.2);
  }

  private pad(time: number): void {
    const ctx = this.ctx;
    const master = this.master;
    if (!ctx || !master) return;
    const gain = ctx.createGain();
    gain.gain.setValueAtTime(0.0001, time);
    gain.gain.linearRampToValueAtTime(0.1, time + 0.4);
    gain.gain.linearRampToValueAtTime(0.0001, time + 1.8);
    gain.connect(master);
    for (const semi of [0, 3, 7, 12]) {
      const osc = ctx.createOscillator();
      osc.type = 'triangle';
      osc.frequency.value = midiToFreq(this.rootNote + 24 + semi);
      osc.connect(gain);
      osc.start(time);
      osc.stop(time + 1.9);
    }
  }
}
