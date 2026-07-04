import * as signalR from '@microsoft/signalr';
import type {
  AudioFeatureFrame,
  DjAction,
  MixerState,
  StyleSnapshot,
} from '../domain/types';

export interface DjClientHandlers {
  onState?: (state: MixerState) => void;
  onAction?: (action: DjAction) => void;
  onStyle?: (style: StyleSnapshot) => void;
  onConnectionChange?: (connected: boolean) => void;
}

/**
 * Thin, typed wrapper over the SignalR hub. The server is the authoritative DJ
 * brain; this client receives state/actions/style and sends human actions and
 * live audio feature frames back for training.
 */
export class DjClient {
  private readonly connection: signalR.HubConnection;
  private readonly handlers: DjClientHandlers;

  constructor(handlers: DjClientHandlers, url = '/hub/dj') {
    this.handlers = handlers;
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(url)
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.connection.on('OnState', (state: MixerState) => this.handlers.onState?.(state));
    this.connection.on('OnAction', (action: DjAction) => this.handlers.onAction?.(action));
    this.connection.on('OnStyle', (style: StyleSnapshot) => this.handlers.onStyle?.(style));

    this.connection.onreconnected(() => this.handlers.onConnectionChange?.(true));
    this.connection.onreconnecting(() => this.handlers.onConnectionChange?.(false));
    this.connection.onclose(() => this.handlers.onConnectionChange?.(false));
  }

  async start(): Promise<void> {
    await this.connection.start();
    this.handlers.onConnectionChange?.(true);
  }

  async stop(): Promise<void> {
    await this.connection.stop();
  }

  sendAction(action: DjAction): Promise<void> {
    return this.connection.send('SendAction', action);
  }

  sendFeatureFrame(frame: AudioFeatureFrame): Promise<void> {
    return this.connection.send('SendFeatureFrame', frame);
  }

  releaseToAi(): Promise<void> {
    return this.connection.send('ReleaseToAi');
  }

  resetStyle(): Promise<void> {
    return this.connection.send('ResetStyle');
  }
}
