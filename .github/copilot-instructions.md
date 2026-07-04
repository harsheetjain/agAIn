# Copilot instructions for agAIn

agAIn is an interactive **AI DJ** web app (React 19 + TypeScript strict, Vite,
Web Audio API). Users "nudge" the DJ with plain text and it plays and mixes
tracks. **[AGENTS.md](../AGENTS.md) is the source of truth** for architecture,
commands, and conventions — read it first.

## Ground rules

- Keep the build green: run `npm run build` (type-checks via `tsc -b`) and
  `npm run lint` before finishing.
- TypeScript is strict with `verbatimModuleSyntax` and `erasableSyntaxOnly`:
  - Use `import type` for type-only imports.
  - **No** `enum` / `namespace` / parameter-properties — use string-literal
    unions and explicit field declarations.
  - No unused variables or parameters.
- React: **function components + hooks only**.

## Architecture in one line

`NudgeConsole` (text) → `App.handleNudge` → `DjBrain.nudge()` → `DjIntent` →
`AudioEngine` + React state → DJ reply in the chat.

## Do

- Keep the app runnable **offline** with **no API keys** and **no bundled audio**.
- Add features behind the existing `DjBrain` interface; swap implementations in
  `App.tsx`.
- Put shared types in `src/dj/types.ts`.

## Don't

- Don't commit secrets — use `.env` (gitignored); only `.env.example` is tracked.
- Don't commit copyrighted audio or real track names/metadata.
- Don't add class components or break the strict-TypeScript rules above.
