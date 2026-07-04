import { useEffect, useRef, useState } from 'react';
import type { NudgeMessage } from '../dj/types';

const SUGGESTIONS = [
  'play something',
  'more energy',
  'bring it down',
  'something emotional',
  'go garage',
  'skip this',
];

interface NudgeConsoleProps {
  messages: NudgeMessage[];
  onNudge: (text: string) => void;
}

/** Chat-style console where the user "nudges" the DJ with free text. */
export function NudgeConsole({ messages, onNudge }: NudgeConsoleProps) {
  const [draft, setDraft] = useState('');
  const logRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    const el = logRef.current;
    if (el) el.scrollTop = el.scrollHeight;
  }, [messages]);

  const send = (text: string) => {
    const value = text.trim();
    if (!value) return;
    onNudge(value);
    setDraft('');
  };

  return (
    <section className="console" aria-label="Nudge the DJ">
      <header className="console__header">
        <h2>Nudge the DJ</h2>
        <p>Tell agAIn what you want to hear.</p>
      </header>

      <div className="console__log" ref={logRef}>
        {messages.length === 0 ? (
          <p className="console__hint">
            Try “play something emotional”, “more energy”, or “go garage”.
          </p>
        ) : (
          messages.map((m) => (
            <div key={m.id} className={`bubble bubble--${m.role}`}>
              <span className="bubble__who">{m.role === 'dj' ? 'agAIn' : 'you'}</span>
              <p>{m.text}</p>
            </div>
          ))
        )}
      </div>

      <div className="suggestions">
        {SUGGESTIONS.map((s) => (
          <button key={s} type="button" className="chip" onClick={() => send(s)}>
            {s}
          </button>
        ))}
      </div>

      <form
        className="composer"
        onSubmit={(e) => {
          e.preventDefault();
          send(draft);
        }}
      >
        <input
          type="text"
          value={draft}
          placeholder="Nudge agAIn…"
          onChange={(e) => setDraft(e.target.value)}
          aria-label="Nudge message"
        />
        <button type="submit" className="btn btn--primary">
          Send
        </button>
      </form>
    </section>
  );
}
