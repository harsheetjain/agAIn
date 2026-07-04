# Copilot instructions for agAIn

agAIn is an interactive **AI DJ / mixer**: a **React 19 + TypeScript** console
(`web/`) driven by a **.NET 10 (ASP.NET Core + SignalR)** brain (`server/`). It
plays an autonomous set when idle and learns the user's style from a live
listening loop. **[AGENTS.md](../AGENTS.md) is the source of truth** — read it
first.

## Ground rules

- The **server is the authoritative brain and state**; the web app renders
  server-driven state, plays audio, and streams features/actions back over
  SignalR. Don't move brain logic into the client.
- Keep builds green before finishing:
  - `server`: `dotnet build && dotnet test`
  - `web`: `npm run lint && npm run build`
- **Guardrails:** learn style/behaviour from *features + actions*, never clone
  copyrighted audio; no scraping; only derived features leave the browser; no
  bundled/copyrighted audio; no secrets in the client (use config/server).

## .NET conventions

- Clean architecture: interfaces in `AgainDj.Domain.Abstractions`, implementations
  in `Infrastructure`, use-cases in `Application`, DI only in the `Api` composition
  root. Keep `MixerReducer` and policies pure and unit-tested.
- Immutable `record`s; nullable enabled; one public type per file.
- Add features behind existing ports (e.g. a new `IMixingPolicy`) and swap them in
  `Program.cs` — the UI stays untouched.

## TypeScript conventions

- Strict with `verbatimModuleSyntax` (use `import type`) and `erasableSyntaxOnly`
  (**no** `enum`/parameter-properties — use string-literal unions + explicit fields).
- Function components + hooks only; no unused locals/params.
- The TS types in `web/src/domain/types.ts` mirror the .NET contracts — keep them
  in sync when you change a contract.

## Architecture in one line

`AutonomousLoopService → MixSession.TickAsync → IMixingPolicy.Decide → MixerReducer
→ IConsoleGateway (SignalR) → React console (renders + flashes the pressed control)`.
