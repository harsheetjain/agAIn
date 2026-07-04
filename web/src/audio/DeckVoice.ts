// A compact synth "voice" for one deck: a four-on-the-floor groove (kick, hats,
// bass arpeggio) pitched to the track's key and locked to its tempo. Output is
// routed into a caller-provided node so the MixEngine can EQ and crossfade it.

const LOOKAHEAD_MS = 25;
const SCHEDULE_AHEAD_S = 0.12;
const STEPS_PER_BAR = 16;
const MINOR_SCALE = [0, 2, 3, 5, 7, 8, 10];

// Camelot number (letter 'A') -> pitch class (C=0..B=11).
const CAMELOT_TO_PITCH_CLASS: Record<number, number> = {
  5: 0, 12: 1, 7: 2, 2: 3, 9: 4, 4: 5, 11: 6, 6: 7, 1: 8, 8: 9, 3: 10, 10: 11,
};

function keyToRootMidi(key: string): number {
  const match = /^(\d{1,2})([AB])$/.exec(key?.trim() ?? '');
  const number = match ? Number.parseInt(match[1], 10) : 8;
  const pitchClass = CAMELOT_TO_PITCH_CLASS[number] ?? 9;
  return 33 + pitchClass;
}

function midiToFreq(midi: number): number {
  return 440 * Math.pow(2, (midi - 69) / 12);
}

export class DeckVoice {
  private readonly ctx: AudioContext;
  private readonly out: GainNode;
  private readonly noise: AudioBuffer;
  private timer: number | null = null;
  private step = 0;
  private nextNoteTime = 0;
  private bpm = 124;
  private rootNote = 42;

  constructor(ctx: AudioContext, destination: AudioNode) {
    this.ctx = ctx;
    this.out = ctx.createGain();
    this.out.gain.value = 0.9;
    this.out.connect(destination);

    this.noise = ctx.createBuffer(1, Math.floor(ctx.sampleRate * 0.4), ctx.sampleRate);
    const data = this.noise.getChannelData(0);
    for (let i = 0; i < data.length; i += 1) {
      data[i] = Math.random() * 2 - 1;
    }
  }

  get output(): AudioNode {
    return this.out;
  }

  setTrack(bpm: number, key: string): void {
    if (bpm > 0) {
      this.bpm = bpm;
    }
    this.rootNote = keyToRootMidi(key);
  }

  get isRunning(): boolean {
    return this.timer !== null;
  }

  start(): void {
    if (this.timer !== null) {
      return;
    }
    this.step = 0;
    this.nextNoteTime = this.ctx.currentTime + 0.05;
    this.timer = window.setInterval(() => this.scheduler(), LOOKAHEAD_MS);
  }

  stop(): void {
    if (this.timer !== null) {
      window.clearInterval(this.timer);
      this.timer = null;
    }
  }

  private scheduler(): void {
    const secondsPerStep = 60 / this.bpm / 4;
    while (this.nextNoteTime < this.ctx.currentTime + SCHEDULE_AHEAD_S) {
      this.scheduleStep(this.step, this.nextNoteTime);
      this.nextNoteTime += secondsPerStep;
      this.step = (this.step + 1) % STEPS_PER_BAR;
    }
  }

  private scheduleStep(step: number, time: number): void {
    if (step % 4 === 0) {
      this.kick(time);
    }
    if (step % 2 === 1) {
      this.hat(time, 0.08);
    }
    if (step % 2 === 0) {
      const degree = MINOR_SCALE[(step / 2) % MINOR_SCALE.length] ?? 0;
      this.bass(this.rootNote + degree, time);
    }
  }

  private kick(time: number): void {
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();
    osc.frequency.setValueAtTime(140, time);
    osc.frequency.exponentialRampToValueAtTime(48, time + 0.12);
    gain.gain.setValueAtTime(1, time);
    gain.gain.exponentialRampToValueAtTime(0.001, time + 0.28);
    osc.connect(gain).connect(this.out);
    osc.start(time);
    osc.stop(time + 0.3);
  }

  private hat(time: number, level: number): void {
    const src = this.ctx.createBufferSource();
    src.buffer = this.noise;
    const hp = this.ctx.createBiquadFilter();
    hp.type = 'highpass';
    hp.frequency.value = 7000;
    const gain = this.ctx.createGain();
    gain.gain.setValueAtTime(level, time);
    gain.gain.exponentialRampToValueAtTime(0.001, time + 0.05);
    src.connect(hp).connect(gain).connect(this.out);
    src.start(time);
    src.stop(time + 0.06);
  }

  private bass(midi: number, time: number): void {
    const osc = this.ctx.createOscillator();
    osc.type = 'sawtooth';
    osc.frequency.value = midiToFreq(midi);
    const lp = this.ctx.createBiquadFilter();
    lp.type = 'lowpass';
    lp.frequency.value = 620;
    const gain = this.ctx.createGain();
    gain.gain.setValueAtTime(0.0001, time);
    gain.gain.linearRampToValueAtTime(0.3, time + 0.01);
    gain.gain.exponentialRampToValueAtTime(0.001, time + 0.18);
    osc.connect(lp).connect(gain).connect(this.out);
    osc.start(time);
    osc.stop(time + 0.2);
  }
}
