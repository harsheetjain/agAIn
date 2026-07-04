import type { AudioFeatureFrame } from '../domain/types';

/**
 * The "listen" half of the training loop: it samples the live master output a
 * few times a second and derives features (loudness, brightness, chroma) plus
 * the current tempo, then hands each frame to a callback (which streams it to
 * the server). Only derived features leave the browser — never raw audio.
 */
export class FeatureSampler {
  private readonly ctx: AudioContext;
  private readonly analyser: AnalyserNode;
  private readonly onFrame: (frame: AudioFeatureFrame) => void;
  private readonly tempoProvider: () => number;
  private readonly intervalMs: number;
  private readonly freq: Uint8Array<ArrayBuffer>;
  private readonly time: Float32Array<ArrayBuffer>;
  private timer: number | null = null;

  constructor(
    ctx: AudioContext,
    analyser: AnalyserNode,
    onFrame: (frame: AudioFeatureFrame) => void,
    tempoProvider: () => number,
    intervalMs = 250,
  ) {
    this.ctx = ctx;
    this.analyser = analyser;
    this.onFrame = onFrame;
    this.tempoProvider = tempoProvider;
    this.intervalMs = intervalMs;
    this.freq = new Uint8Array(analyser.frequencyBinCount);
    this.time = new Float32Array(analyser.fftSize);
  }

  get isRunning(): boolean {
    return this.timer !== null;
  }

  start(): void {
    if (this.timer !== null) {
      return;
    }
    this.timer = window.setInterval(() => this.sample(), this.intervalMs);
  }

  stop(): void {
    if (this.timer !== null) {
      window.clearInterval(this.timer);
      this.timer = null;
    }
  }

  private sample(): void {
    this.analyser.getByteFrequencyData(this.freq);
    this.analyser.getFloatTimeDomainData(this.time);

    let sumSquares = 0;
    for (let i = 0; i < this.time.length; i += 1) {
      const sample = this.time[i] ?? 0;
      sumSquares += sample * sample;
    }
    const rms = Math.min(1, Math.sqrt(sumSquares / this.time.length) * 1.6);

    const nyquist = this.ctx.sampleRate / 2;
    const binHz = nyquist / this.freq.length;
    const chroma = new Array<number>(12).fill(0);
    let magnitudeSum = 0;
    let weightedFreq = 0;
    for (let i = 1; i < this.freq.length; i += 1) {
      const magnitude = (this.freq[i] ?? 0) / 255;
      if (magnitude <= 0) {
        continue;
      }
      const frequency = i * binHz;
      magnitudeSum += magnitude;
      weightedFreq += frequency * magnitude;
      if (frequency >= 20) {
        const midi = 69 + (12 * Math.log2(frequency / 440));
        const pitchClass = ((Math.round(midi) % 12) + 12) % 12;
        chroma[pitchClass] += magnitude;
      }
    }

    const centroid = magnitudeSum > 0 ? weightedFreq / magnitudeSum / nyquist : 0;
    const chromaSum = chroma.reduce((a, b) => a + b, 0);
    const normalizedChroma = chromaSum > 0 ? chroma.map((c) => c / chromaSum) : chroma;

    this.onFrame({
      timestampSeconds: this.ctx.currentTime,
      rms,
      spectralCentroid: Math.min(1, Math.max(0, centroid)),
      tempo: this.tempoProvider(),
      chroma: normalizedChroma,
    });
  }
}
