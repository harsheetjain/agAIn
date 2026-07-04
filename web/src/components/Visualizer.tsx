import { useEffect, useRef } from 'react';

interface VisualizerProps {
  getAnalyser: () => AnalyserNode | null;
}

/** Master-bus frequency bars — the visible pulse of the live mix. */
export function Visualizer({ getAnalyser }: VisualizerProps) {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) {
      return;
    }
    const ctx = canvas.getContext('2d');
    if (!ctx) {
      return;
    }

    let raf = 0;
    let buffer: Uint8Array<ArrayBuffer> | null = null;

    const render = () => {
      raf = requestAnimationFrame(render);
      const { width, height } = canvas;
      ctx.clearRect(0, 0, width, height);

      const analyser = getAnalyser();
      if (!analyser) {
        return;
      }

      const bins = analyser.frequencyBinCount;
      if (!buffer || buffer.length !== bins) {
        buffer = new Uint8Array(bins);
      }
      analyser.getByteFrequencyData(buffer);

      const barWidth = width / bins;
      for (let i = 0; i < bins; i += 1) {
        const v = (buffer[i] ?? 0) / 255;
        const barHeight = v * height;
        ctx.fillStyle = `hsl(${175 + v * 120} 95% ${45 + v * 25}%)`;
        ctx.fillRect(i * barWidth, height - barHeight, barWidth + 0.5, barHeight);
      }
    };

    render();
    return () => cancelAnimationFrame(raf);
  }, [getAnalyser]);

  return <canvas ref={canvasRef} className="viz" width={900} height={90} aria-hidden="true" />;
}
