# agAIn 🎧 — live AI DJ

**agAIn** is an interactive **AI DJ / mixer** in your browser. It runs a live set
on a two-deck console — and when you step away it **takes over autonomously**,
beat-matching and harmonically blending track to track while you *watch it press
the buttons*. Grab any control and you're back in charge; leave it idle and
agAIn resumes. The name hides the **AI** inside "ag·**AI**·n".

It also **learns your style**: a listening loop samples the live output (derived
features only) and, together with your own console moves, trains an
interpretable style model (energy, tempo, transition pacing, harmonic/mood
preference) so the autonomous set drifts toward how *you* mix.

> **Legal & safe by design:** agAIn learns *how to mix* from features and your
> actions — it never scrapes YouTube or clones copyrighted recordings. Only
> derived features leave the browser, and the demo crate ships zero audio
> (grooves are synthesized). Add only your own or licensed audio.

## Architecture

| Tier | Stack | Role |
| --- | --- | --- |
| `web/` | React 19 + TypeScript (Vite) | Console UI, Web Audio mix engine, listening loop |
| `server/` | .NET 10 (ASP.NET Core, SignalR) | DJ brain, autonomous loop, online style trainer |

The **server is the authoritative brain**; the **web app renders its state,
plays the mix, and streams features/actions back over SignalR**. See
**[AGENTS.md](./AGENTS.md)** for the full architecture and how to extend it.

## Quickstart

Two processes (Vite proxies `/api` and `/hub` to the API — one origin):

```bash
# 1) API  → http://localhost:5215
cd server
dotnet run --project AgainDj.Api --no-launch-profile

# 2) Web  → http://localhost:5173
cd web
npm install
npm run dev
```

Open **http://localhost:5173**, hit **Enable sound**, and either watch agAIn play
or take the decks yourself.

### Expose it with a dev tunnel

```bash
devtunnel user login
devtunnel host -p 5173 --allow-anonymous   # share the printed https URL
```

## Build · test · lint

```bash
cd server && dotnet build && dotnet test
cd web    && npm run lint && npm run build
```

## For AI agents & contributors

- **[AGENTS.md](./AGENTS.md)** — architecture, interfaces, commands, guardrails.
- **[.github/copilot-instructions.md](./.github/copilot-instructions.md)** — Copilot custom instructions.
- **[.github/workflows/copilot-setup-steps.yml](./.github/workflows/copilot-setup-steps.yml)** — Copilot coding-agent environment.

## License

[MIT](./LICENSE) © Harsheet Jain
