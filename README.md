# agAIn 🎧

**agAIn** is an interactive **AI DJ** in your browser. Nudge it with plain
language — *"more energy"*, *"something emotional"*, *"go garage"* — and it
picks, plays and mixes the vibe you're after. The name hides the **AI** inside
"ag·**AI**·n", because you'll always want one more track.

> Inspired by live, improvised, sample-driven sets — built so you can *talk* to
> the decks.

## ✨ Features

- 💬 **Nudge console** — steer the set with natural language.
- 🧠 **Pluggable DJ brain** — ships with a rule-based brain; swap in an LLM
  behind the same interface.
- 🔊 **Zero-asset audio** — a Web Audio synth generates an audible groove from
  each track's BPM/key, so it runs offline with **no API keys and no
  copyrighted audio**.
- 📈 **Live visualizer** driven by the Web Audio analyser.

## 🚀 Quickstart

```bash
npm install
npm run dev      # http://localhost:5173
```

Build for production:

```bash
npm run build && npm run preview
```

## 🕹️ Try saying

`play something` · `more energy` · `bring it down` · `something emotional` ·
`go garage` · `skip this`

## 🧩 How it works

`NudgeConsole` → `App.handleNudge` → `DjBrain.nudge()` returns a `DjIntent` →
`App` drives the `AudioEngine` and updates the UI. Full architecture and how to
extend it (LLM brain, real audio) live in **[AGENTS.md](./AGENTS.md)**.

## 🤖 For AI agents & contributors

This repo is set up for agentic development:

- **[AGENTS.md](./AGENTS.md)** — architecture, commands, conventions, guardrails.
- **[.github/copilot-instructions.md](./.github/copilot-instructions.md)** —
  Copilot custom instructions.
- **[.github/workflows/copilot-setup-steps.yml](./.github/workflows/copilot-setup-steps.yml)**
  — environment setup for the Copilot coding agent.

## 📄 License

[MIT](./LICENSE) © Harsheet Jain
