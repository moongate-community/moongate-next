import { HubConnectionBuilder, type HubConnection } from "@microsoft/signalr";

export type LiveConsoleEntryKind = "Log" | "CommandEcho" | "CommandOutput";

export type LiveConsoleEntry = {
  kind: LiveConsoleEntryKind;
  level?: string;
  timestamp: number;
  message: string;
};

/**
 * Builds the console hub connection. The access token is captured here and returned by the
 * SignalR accessTokenFactory on every (re)connect. If the token is rotated, pass the new token
 * (the ConsolePanel effect is keyed on accessToken and rebuilds the connection when it changes);
 * an already-open socket keeps its original token until the next reconnect.
 */
export function createConsoleConnection(accessToken: string): HubConnection {
  return new HubConnectionBuilder()
    .withUrl("/hubs/console", { accessTokenFactory: () => accessToken })
    .withAutomaticReconnect()
    .build();
}
