# AGENTS.md

Guidance for AI coding agents (GitHub Copilot and others) working in **agAIn**.
Read this before making changes — it is the source of truth for architecture,
commands, conventions, and guardrails.

## What this project is

**agAIn** is an interactive **AI DJ / mixer** in the browser. It runs a live set
on a two-deck console (decks, crossfader, 3-band EQ, sampler pads). When you
step away it **takes over autonomously** — you watch it "press the buttons",
beat-match and harmonically blend between tracks. Touch any control and you take
over; after a short idle it resumes.

It also runs a **"listen → sample → train" loop**: the browser samples the live
master output (derived audio features only), streams those frames to the server,
and an online learner folds them — plus your own console actions — into an
interpretable **style** (energy target, tempo centre, transition pacing,
harmonic and mood preference). Over time the autonomous set drifts toward how
*you* mix.

> **Guardrails (non-negotiable):** learn *style/behaviour from features*, never
> clone copyrighted recordings. No YouTube/audio scraping. Only derived features
> leave the browser. Any real audio you add must be your own or licensed. Titles
> in the demo crate are original; no audio is bundled (grooves are synthesized).

## Monorepo layout

```
web/       React 19 + TypeScript (Vite) — the console UI + Web Audio + listen loop
server/    .NET 10 (ASP.NET Core) — the DJ brain, autonomous loop, SignalR, trainer
.github/   CI + Copilot config
```

The **server is the authoritative brain and state**; the **web app renders
server-driven state, plays the mix, and streams features/actions back.**

### server/ — clean/onion architecture

```
AgainDj.Domain          # pure core: entities + interfaces (no external deps)
  Model/                #   Track, MixerState, DeckState, DjAction, StyleSnapshot, ...
  Abstractions/         #   IMixingPolicy, IStyleTrainer, ITrackLibrary, IConsoleGateway, ...
  Mixing/MixerReducer   #   pure (state, action) -> state and time advance
AgainDj.Application     # use-cases: MixSession (runtime), SessionCoordinator (hand-off)
AgainDj.Infrastructure  # implementations: RuleMixingPolicy, OnlineStyleTrainer,
                        #   JsonStyleStore, InMemoryTrackLibrary, RunningAudioAnalyzer
AgainDj.Api             # ASP.NET Core: DjHub (SignalR), controllers, DI composition
                        #   root, SignalRConsoleGateway, AutonomousLoopService
AgainDj.Tests           # xUnit tests for the reducer, policy and harmonic mixing
```

Dependencies point inward: `Application`/`Infrastructure` → `Domain`; `Api` →
`Application` + `Infrastructure`. The API is the only composition root. SignalR
lives behind the `IConsoleGateway` port so the core stays transport-agnostic.

### web/ — layered client

```
src/domain/types.ts     # TS mirror of the server contracts
src/realtime/           # DjClient (SignalR), useDj hook, usePressFx (control flashes)
src/audio/              # MixEngine (2 synth decks + EQ + crossfader + analyser),
                        #   DeckVoice (per-deck synth), FeatureSampler (listen loop)
src/components/         # DeckPanel, Crossfader, SamplerPads, StylePanel, ModeBar,
                        #   ActionFeed, Visualizer
src/App.tsx             # composition
```

## Run it

Two processes (the Vite dev server proxies `/api` and `/hub` to the API, so the
browser uses one origin — which also means a dev tunnel on `:5173` exposes
everything).

```bash
# 1) API  (http://localhost:5215)
cd server && dotnet run --project AgainDj.Api --no-launch-profile

# 2) Web  (http://localhost:5173)
cd web && npm install && npm run dev
```

Open http://localhost:5173, click **Enable sound**, then leave it alone to watch
agAIn play — or grab the controls yourself.

### Build / test / lint

```bash
cd server && dotnet build && dotnet test      # .NET
cd web    && npm run lint && npm run build     # web (tsc -b + vite)
```

Always run these before a PR.

## Data flow

```
Browser (MixEngine plays; FeatureSampler @4Hz)
  --SignalR SendFeatureFrame-->  MixSession.IngestFeatureFrame -> OnlineStyleTrainer
Human moves a control
  --SignalR SendAction-->        MixSession.ApplyHumanActionAsync (Actor=Human, trains)
AutonomousLoopService (~180ms)  -> MixSession.TickAsync -> IMixingPolicy.Decide
  -> MixerReducer.Apply          -> IConsoleGateway.Broadcast{Action,State,Style}
  --SignalR OnAction/OnState/OnStyle-->  Browser (renders + flashes pressed control)
```

## Extending it

- **Smarter brain:** implement `IMixingPolicy` (e.g. an LLM/learned policy) and
  register it in `Program.cs` instead of `RuleMixingPolicy`. The UI is untouched.
- **Deploy a model to Azure:** stand up a Hugging Face model (embeddings /
  MusicGen) behind an Azure ML managed online endpoint and call it from a new
  `IMixingPolicy`/analyzer. Keep secrets in config, never in the client.
- **Real audio:** give a `Track` a `src` URL and teach `MixEngine`/`DeckVoice` to
  prefer a buffer/`<audio>` source. **Do not commit copyrighted audio.**

## Conventions

**C#** — nullable enabled; prefer immutable `record`s; interfaces in
`Domain.Abstractions`; keep the reducer/policy pure and unit-tested; DI only in
the API composition root; one public type per file.

**TypeScript** — strict with `verbatimModuleSyntax` (use `import type`) and
`erasableSyntaxOnly` (**no** `enum`/parameter-properties — use string-literal
unions and explicit fields); function components + hooks; no unused
locals/params.

## Roadmap

- LLM-backed `IMixingPolicy`; Azure ML endpoint for embeddings/generation.
- Real audio playback + crossfade/beat-grid alignment; stem sampler (Demucs).
- Expose the UI via a dev tunnel (`devtunnel host -p 5173`).
- Persisted set history; richer training signals.
