import { useEffect, useRef } from 'react';

interface VisualizerProps {
  getAnalyser: () => AnalyserNode | null;
  active: boolean;
}

/** Live frequency-bar visualizer driven by the engine's AnalyserNode. */
export function Visualizer({ getAnalyser, active }: VisualizerProps) {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    let raf = 0;
    let buffer: Uint8Array<ArrayBuffer> | null = null;

    const render = () => {
      raf = requestAnimationFrame(render);
      const { width, height } = canvas;
      ctx.clearRect(0, 0, width, height);

      const analyser = getAnalyser();
      if (!analyser) return;

      const bins = analyser.frequencyBinCount;
      if (!buffer || buffer.length !== bins) {
        buffer = new Uint8Array(bins);
      }
      analyser.getByteFrequencyData(buffer);

      const barWidth = width / bins;
      for (let i = 0; i < bins; i += 1) {
        const v = (buffer[i] ?? 0) / 255;
        const barHeight = v * height;
        const hue = 175 + v * 120;
        ctx.fillStyle = `hsl(${hue} 95% ${45 + v * 25}%)`;
        ctx.fillRect(i * barWidth, height - barHeight, barWidth + 0.6, barHeight);
      }
    };

    render();
    return () => cancelAnimationFrame(raf);
  }, [getAnalyser]);

  return (
    <canvas
      ref={canvasRef}
      className={active ? 'viz viz--active' : 'viz'}
      width={520}
      height={130}
      aria-hidden="true"
    />
  );
}
