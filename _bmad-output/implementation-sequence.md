---
project: 'BounceReaper'
date: '2026-03-13'
status: 'active'
approach: 'quick-dev / quick-spec (no formal stories)'
---

# Implementation Sequence — Bounce Reaper MVP

**Approche :** Pas de stories formelles. L'architecture (`game-architecture.md`) + le project context (`project-context.md`) + `CLAUDE.md` sont les specs. Quick dev direct quand l'archi suffit, quick spec rapide quand des détails gameplay manquent.

**Documents de référence :**
- `_bmad-output/game-architecture.md` — architecture complète (8 ADR, patterns, structure)
- `_bmad-output/project-context.md` — 95 règles critiques pour AI agents
- `CLAUDE.md` — conventions projet

---

## Phase 1 — Fondations (Quick Dev Direct)

L'architecture a tous les exemples de code. Aucune spec supplémentaire nécessaire.

| # | Système | Fichiers | Source archi |
| --- | --- | --- | --- |
| 1 | Core | `GameEvents.cs`, `Singleton.cs`, `GameConstants.cs`, `GameConfig.cs` (SO class) | ADR-001 + Implementation Patterns |
| 2 | Data | `SaveData.cs`, `GameState.cs` (enum), `CurrencyType.cs` (enum) | ADR-005 + Cross-cutting |
| 3 | SaveManager | `SaveManager.cs` | ADR-005 (Json.NET, persistentDataPath, OnApplicationPause) |

**Status :** [ ] Non commencé

---

## Phase 2 — Gameplay (Quick Spec → Quick Dev)

Un quick spec rapide (5-10 min) avant chaque pour régler les détails d'implémentation gameplay manquants.

| # | Système | Fichiers | Détails manquants à spécifier |
| --- | --- | --- | --- |
| 4 | Ball | `BallController.cs`, `BallStats.cs` (SO), `BallManager.cs` | Vitesse initiale, angle rebond, min/max speed, comportement coins |
| 5 | Enemy | `EnemyController.cs`, `EnemyStats.cs` (SO), `EnemyHealth.cs` | Types MVP (3-4), patterns mouvement, death flow |
| 6 | Wave | `WaveManager.cs`, `WaveConfig.cs` (SO) | Algorithme spawn, ramp difficulty, boss trigger (wave 10) |
| 7 | Currency | `CurrencyManager.cs` | Formules rewards, anti-exploit, shards/souls par ennemi |
| 8 | Upgrade | `UpgradeManager.cs`, `UpgradeConfig.cs` (SO) | Upgrades MVP, formules coût, effets |

**Status :** [ ] Non commencé

---

## Phase 3 — Polish & Systems (Quick Spec → Quick Dev)

| # | Système | Fichiers | Détails manquants à spécifier |
| --- | --- | --- | --- |
| 9 | VFX | `VFXManager.cs` | Orchestration hitstop + shake + damage numbers + combo |
| 10 | UI | `UIManager.cs`, `HUDController.cs`, `UpgradePanel.cs`, `BossTimerUI.cs` | Layout HUD, éléments, connexion events |
| 11 | Input | `InputManager.cs` | Zones touch, InputAction assets, gestes |
| 12 | Audio | `AudioManager.cs` | ADR-006 suffit probablement (quick dev direct possible) |

**Status :** [ ] Non commencé

---

## Workflow par système

```
Si Phase 1 (fondations) :
  → /bmad-gds-quick-dev directement (l'archi a tout)

Si Phase 2-3 (gameplay/polish) :
  → /bmad-gds-quick-spec (5 min, résoudre les détails manquants)
  → /bmad-gds-quick-dev (implémenter le spec)
```

## Ordre des dépendances

```
Core (GameEvents, Singleton) ← TOUT dépend de ça
  → Data (enums, SaveData)
    → SaveManager
      → CurrencyManager, UpgradeManager
        → Ball, Enemy
          → Wave
            → VFX, UI, Input, Audio
```
