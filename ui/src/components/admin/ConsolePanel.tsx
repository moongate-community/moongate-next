import { useEffect, useRef, useState, type FormEvent, type KeyboardEvent } from "react";
import { SendHorizontal } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { ScrollArea } from "@/components/ui/scroll-area";
import { createConsoleConnection, type LiveConsoleEntry } from "../../lib/consoleClient";

const MAX_LINES = 1000;

type ConsolePanelProps = {
  accessToken: string;
};

function levelClass(entry: LiveConsoleEntry): string {
  if (entry.kind === "CommandEcho") {
    return "text-accent font-semibold";
  }

  if (entry.kind === "CommandOutput") {
    return "text-fg";
  }

  switch (entry.level) {
    case "Error":
    case "Fatal":
      return "text-danger";
    case "Warning":
      return "text-warning";
    default:
      return "text-fg-muted";
  }
}

function formatTime(timestamp: number): string {
  return new Date(timestamp).toLocaleTimeString();
}

export function ConsolePanel({ accessToken }: ConsolePanelProps) {
  const [lines, setLines] = useState<LiveConsoleEntry[]>([]);
  const [input, setInput] = useState("");
  const [connected, setConnected] = useState(false);
  const history = useRef<string[]>([]);
  const historyIndex = useRef<number>(-1);
  const scrollRef = useRef<HTMLDivElement | null>(null);
  const connectionRef = useRef<ReturnType<typeof createConsoleConnection> | null>(null);

  useEffect(() => {
    let mounted = true;
    const connection = createConsoleConnection(accessToken);
    connectionRef.current = connection;

    connection.on("backlog", (backlog: LiveConsoleEntry[]) => {
      setLines(backlog.slice(-MAX_LINES));
    });

    connection.on("line", (entry: LiveConsoleEntry) => {
      setLines((current) => [...current, entry].slice(-MAX_LINES));
    });

    connection.onreconnected(() => {
      if (mounted) {
        setConnected(true);
      }
    });

    connection.onreconnecting(() => {
      if (mounted) {
        setConnected(false);
      }
    });

    connection.onclose(() => {
      if (mounted) {
        setConnected(false);
      }
    });

    void connection
      .start()
      .then(() => {
        if (mounted) {
          setConnected(true);
        }
      })
      .catch(() => {
        if (mounted) {
          setConnected(false);
        }
      });

    return () => {
      mounted = false;
      void connection.stop();
      connectionRef.current = null;
    };
  }, [accessToken]);

  useEffect(() => {
    const node = scrollRef.current?.querySelector<HTMLElement>("[data-slot='scroll-area-viewport']");

    if (node) {
      node.scrollTop = node.scrollHeight;
    }
  }, [lines]);

  async function submit(event: FormEvent) {
    event.preventDefault();

    const command = input.trim();

    if (command.length === 0 || !connectionRef.current) {
      return;
    }

    history.current = [...history.current, command];
    historyIndex.current = -1;
    setInput("");

    try {
      await connectionRef.current.invoke("ExecuteCommand", command);
    } catch (error) {
      console.error("Failed to execute console command", error);
    }
  }

  function onKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (history.current.length === 0) {
      return;
    }

    if (event.key === "ArrowUp") {
      event.preventDefault();
      const next = historyIndex.current < 0 ? history.current.length - 1 : Math.max(0, historyIndex.current - 1);
      historyIndex.current = next;
      setInput(history.current[next]);
    }

    if (event.key === "ArrowDown") {
      event.preventDefault();

      if (historyIndex.current < 0) {
        return;
      }

      const next = historyIndex.current + 1;

      if (next >= history.current.length) {
        historyIndex.current = -1;
        setInput("");

        return;
      }

      historyIndex.current = next;
      setInput(history.current[next]);
    }
  }

  return (
    <section className="grid gap-3 px-5 py-6 md:px-7">
      <Card className="gap-0 rounded-md border-border bg-surface py-0 shadow-card">
        <CardHeader className="flex flex-row items-center justify-between gap-3 border-b border-border px-4 py-3">
          <CardTitle className="text-lg tracking-tight text-fg">Live Console</CardTitle>
          <Badge
            variant="outline"
            className={`gap-1.5 rounded-md px-2 text-xs font-semibold ${connected ? "border-success/20 bg-success/10 text-success" : "border-border bg-muted text-fg-subtle"}`}
          >
            <span className={`h-2 w-2 rounded-full ${connected ? "bg-success" : "bg-fg-subtle"}`} aria-hidden />
            {connected ? "Connected" : "Disconnected"}
          </Badge>
        </CardHeader>
        <CardContent className="grid gap-3 p-3">
          <ScrollArea
            ref={scrollRef}
            className="h-[60vh] min-h-[200px] rounded-md border border-border bg-bg font-mono text-[13px] leading-relaxed"
          >
            <div className="grid gap-0.5 p-3">
              {lines.map((entry, index) => (
                <div key={index} className={`whitespace-pre-wrap break-words ${levelClass(entry)}`}>
                  <span className="mr-2 text-fg-subtle">{formatTime(entry.timestamp)}</span>
                  {entry.kind === "Log" && entry.level ? <span className="mr-2">[{entry.level}]</span> : null}
                  {entry.message}
                </div>
              ))}
            </div>
          </ScrollArea>

          <form onSubmit={submit} className="flex gap-2">
            <span className="flex items-center font-mono text-accent">&gt;</span>
            <Input
              type="text"
              aria-label="Command input"
              value={input}
              onChange={(event) => setInput(event.target.value)}
              onKeyDown={onKeyDown}
              placeholder="Command"
              autoComplete="off"
              spellCheck={false}
              className="min-w-0 flex-1 bg-bg font-mono text-sm text-fg"
            />
            <Button type="submit" size="icon-sm" aria-label="Send command">
              <SendHorizontal size={16} aria-hidden />
            </Button>
          </form>
        </CardContent>
      </Card>
    </section>
  );
}
