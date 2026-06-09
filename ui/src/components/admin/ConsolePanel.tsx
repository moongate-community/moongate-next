import { useEffect, useRef, useState, type FormEvent, type KeyboardEvent } from "react";
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
      return "text-red-400";
    case "Warning":
      return "text-amber-400";
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
    const node = scrollRef.current;

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
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-bold tracking-tight text-fg">Live Console</h2>
        <span className={`text-xs font-semibold ${connected ? "text-emerald-400" : "text-fg-subtle"}`}>
          {connected ? "● connected" : "○ disconnected"}
        </span>
      </div>

      <div
        ref={scrollRef}
        className="h-[60vh] min-h-[200px] overflow-y-auto rounded-lg border border-border bg-surface p-3 font-mono text-[13px] leading-relaxed shadow-card"
      >
        {lines.map((entry, index) => (
          <div key={index} className={`whitespace-pre-wrap break-words ${levelClass(entry)}`}>
            <span className="mr-2 text-fg-subtle">{formatTime(entry.timestamp)}</span>
            {entry.kind === "Log" && entry.level ? <span className="mr-2">[{entry.level}]</span> : null}
            {entry.message}
          </div>
        ))}
      </div>

      <form onSubmit={submit} className="flex gap-2">
        <span className="flex items-center font-mono text-accent">&gt;</span>
        <input
          type="text"
          aria-label="Command input"
          value={input}
          onChange={(event) => setInput(event.target.value)}
          onKeyDown={onKeyDown}
          placeholder="Type a command and press Enter…"
          autoComplete="off"
          spellCheck={false}
          className="min-w-0 flex-1 rounded-md border border-border bg-bg px-3 py-2 font-mono text-sm text-fg outline-none focus:border-accent"
        />
      </form>
    </section>
  );
}
