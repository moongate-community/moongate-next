import { Sparkles, UserRound } from "lucide-react";
import type { AuthUser } from "../types/auth";

type PlayerDashboardProps = {
  user: AuthUser;
};

const characterSlots = [
  {
    name: "No character selected",
    meta: "Create or link a character when the shard roster API is ready."
  },
  {
    name: "Shard access",
    meta: "Use your account to enter Moongate from the UO client."
  }
];

export function PlayerDashboard({ user }: PlayerDashboardProps) {
  return (
    <section className="workspace player-dashboard">
      <header className="player-hero">
        <div className="player-avatar" aria-hidden>
          <UserRound size={34} />
        </div>
        <div>
          <h2>Player dashboard</h2>
          <p>Account status, character access, and personal shard entry points for {user.username}.</p>
        </div>
      </header>

      <div className="player-summary">
        <article>
          <span>Account</span>
          <strong>{user.isActive ? "Ready" : "Inactive"}</strong>
        </article>
        <article>
          <span>Access level</span>
          <strong>{user.level}</strong>
        </article>
      </div>

      <div className="player-grid">
        <section className="character-panel">
          <header>
            <h3>Characters</h3>
            <span>0 linked</span>
          </header>
          <div className="character-list">
            {characterSlots.map((slot) => (
              <article key={slot.name}>
                <Sparkles size={18} aria-hidden />
                <div>
                  <strong>{slot.name}</strong>
                  <p>{slot.meta}</p>
                </div>
              </article>
            ))}
          </div>
        </section>

        <aside className="entry-panel">
          <span>Next step</span>
          <strong>Connect a game client</strong>
          <p>Player-specific tools will live here: characters, account settings, and in-game session state.</p>
        </aside>
      </div>
    </section>
  );
}
