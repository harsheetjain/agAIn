# AGENTS.md

Guidance for AI coding agents (GitHub Copilot and others) working in **agAIn**.
Read this before making changes — it is the source of truth for architecture,
commands, and conventions.

## What this project is

**agAIn** is an interactive **AI DJ** in the browser. You "nudge" the DJ with
plain-language messages — *"more energy"*, *"something emotional"*, *"go
garage"* — and it responds by picking and playing tracks and adjusting
tempo / mood / volume, chatting back as it goes. It's inspired by live,
improvised, sample-driven sets — built so you can *talk* to the decks.

The current build is a **dependency-free, offline demo**:

- a **rule-based DJ brain** (keyword → intent), and
- a **Web Audio synth** that generates an audible groove from each track's
  BPM/key.

So it runs with **no API keys** and **no bundled/copyrighted audio**. The
architecture is deliberately layered so an LLM brain and real audio can be
dropped in **without touching the UI**.

## Tech stack

- **React 19 + TypeScript** (strict), bundled with **Vite**.
- **Web Audio API** for synthesis and analysis (no audio assets).
- **oxlint** for linting.

## Repo layout

```
src/
  audio/AudioEngine.ts    # Web Audio synth + AnalyserNode; start/stop/volume/setTrack
  dj/
    types.ts              # Track, DjIntent, DjState, and the DjBrain interface
    tracks.ts             # CRATE: demo tracks (original titles, no `src`)
    djBrain.ts            # RuleDjBrain: free-text nudge -> DjIntent (keyword rules)
  components/
    Deck.tsx              # now-playing + transport controls + visualizer
    NudgeConsole.tsx      # chat log + text input to nudge the DJ
    Visualizer.tsx        # <canvas> frequency bars from the analyser
  App.tsx                 # wires brain + engine + UI state together
  main.tsx                # React entry point
```

## Commands

```bash
npm install        # install dependencies
npm run dev        # dev server at http://localhost:5173
npm run build      # type-check (tsc -b) + production build
npm run typecheck  # type-check only
npm run lint       # oxlint
npm run preview    # serve the production build
```

**Always run `npm run build` and `npm run lint` before opening a PR.** `build`
type-checks the whole project, so a green build means types are sound.

## How a nudge flows

```
NudgeConsole (text)
  -> App.handleNudge(text)
     -> brain.nudge(text, { state, tracks })  => DjIntent
     -> App applies the intent to AudioEngine + React state
     -> App appends the DJ's `intent.say` line to the chat
```

The `DjIntent.action` is one of `play | pause | resume | skip | tempo | mood |
volume | unknown`. `App` is the only place that mutates the engine and state;
the brain is pure (text + state in, intent out).

## Extending the DJ

### Swap in an LLM brain

`RuleDjBrain` implements the **`DjBrain`** interface in `src/dj/types.ts`. To go
conversational, add an alternative (e.g. `LlmDjBrain`) implementing the same
interface and construct it in `App.tsx` instead of `RuleDjBrain`. Have the model
return a `DjIntent`. Keep the interface stable so the UI stays untouched.

- **Never hardcode keys.** Read config from `import.meta.env` (see
  `.env.example`). Anything secret must go through a small server/proxy — do not
  ship secret keys in client code.

### Add real music

Give a `Track` a `src` URL in `src/dj/tracks.ts`, then teach `AudioEngine` to
prefer an audio source (`<audio>` / `AudioBufferSourceNode`) over the synth when
`src` is present. **Do not commit copyrighted audio** — use royalty-free /
licensed sources or user-provided URLs.

## Conventions (enforced by tsconfig — the build fails otherwise)

- **`verbatimModuleSyntax`** → import types with `import type { ... }`.
- **`noUnusedLocals` / `noUnusedParameters`** → no unused variables or params.
- **`erasableSyntaxOnly`** → **no** TS `enum`, `namespace`, or constructor
  parameter properties. Use string-literal unions and explicit field
  declarations instead.
- **Function components + hooks only** — no class components.
- Keep modules small and single-purpose; put shared types in `src/dj/types.ts`.

## Guardrails

- **No secrets in the repo.** Use `.env` (gitignored); only `.env.example` is
  tracked.
- **No copyrighted content** — audio, artwork, or real track metadata. Titles in
  `CRATE` are original.
- **Keep the offline demo working** — `npm run dev` must run with no network and
  no keys.

## Roadmap ideas

- LLM-backed conversational brain (`LlmDjBrain`).
- Real audio playback with crossfade / beatmatching between tracks.
- Persisted crates and listening history.
- Vitest tests for `djBrain` intent parsing.
