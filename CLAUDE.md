# Bounce Reaper — Project Instructions

## Project Overview

Bounce Reaper is a mobile idle bouncer + combat game built in Unity 6.3 LTS (2D URP).
Balls bounce around an arena, kill enemies, drop gold/XP/loot, and the player upgrades balls and buys more.
Target: iOS/Android. Scope: ~1 week for MVP.

## Tech Stack

- **Engine:** Unity 6.3 LTS (6000.3.10f1), 2D URP template
- **Language:** C#
- **Platform:** Mobile (iOS + Android)
- **MCP:** Unity Official MCP (com.unity.ai.assistant)

## Installed Assets

- DOTween Pro — tweens, UI animations, juice
- All In 1 Sprite Shader — glow, outline, dissolve (neon style)
- Text Animator for Unity — animated damage numbers, UI text
- Hot Reload — fast iteration
- vHierarchy 2.0 — hierarchy organization
- Ultimate Game Music Collection — music (to reimport)

## Project Structure

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Ball/          # Ball types, physics, upgrades
│   │   ├── Enemy/         # Enemy types, spawning, AI
│   │   ├── UI/            # HUD, menus, popups
│   │   ├── Managers/      # GameManager, WaveManager, AudioManager
│   │   ├── Progression/   # Prestige, upgrades, currency
│   │   └── Monetization/  # Ads, IAP, offers
│   ├── Prefabs/
│   ├── ScriptableObjects/
│   ├── Scenes/
│   ├── Art/
│   ├── Audio/
│   ├── Localization/
│   └── Settings/
├── Plugins/               # DOTween, AllIn1SpriteShader, Febucci
└── ThirdParty/
```

## Code Conventions

- C# with Unity conventions: PascalCase for public members, \_camelCase for private fields
- Use `[SerializeField] private` over public fields
- Prefer ScriptableObjects for data/config (ball stats, enemy stats, wave config)
- Use DOTween for all animations and tweens (not Unity Animator for simple stuff)
- Namespace: `BounceReaper.<Module>` (e.g., `BounceReaper.Ball`, `BounceReaper.Enemy`)
- One MonoBehaviour per file, filename matches class name
- Use TextMeshPro for all text (never legacy Text)

## Architecture Patterns

- Singleton pattern for managers (GameManager, AudioManager, etc.)
- Observer pattern for events (OnEnemyKilled, OnWaveComplete, OnCurrencyChanged)
- Object pooling for balls, enemies, damage numbers, particles
- ScriptableObjects for all game data (no magic numbers in code)

## Art Style

- Geometric neon on dark background
- Balls = glowing circles with bloom/trails
- Enemies = colored geometric shapes
- Use All In 1 Sprite Shader for glow effects
- Particle effects for trails and explosions

## Game Design Reference

- Brainstorming session: `_bmad-output/brainstorming-session-2026-03-08.md`
- Monetization details: `_bmad-output/monetisation-bounce-reaper.md`

## BMAD Framework

- BMAD GDS installed in `_bmad/` — use slash commands for workflows
- Output goes to `_bmad-output/`
- Key commands: `/bmad-gds-create-game-brief`, `/bmad-gds-create-gdd`, `/bmad-gds-dev-story`

## Communication

- Respond in French (user preference)
- Document output language: French
- Keep explanations concise and direct

## Do NOT

- Use Unity's legacy UI (use TextMeshPro + Canvas)
- Use Find() or GetComponent() in Update loops
- Create God classes — keep scripts focused and small
- Use string-based comparisons for tags (use enums or ScriptableObject references)
- Add features beyond current sprint scope

## Commits

Format: `<emoji> <Type>: <short description>`

| Emoji | Type     | Example                                 |
| ----- | -------- | --------------------------------------- |
| ✨    | Feat     | `✨ Feat: add double jump`              |
| 🐞    | Fix      | `🐞 Fix: player stuck in wall`          |
| 🔨    | Refactor | `🔨 Refactor: extract health component` |
| 🎨    | Style    | `🎨 Style: update main menu layout`     |
| 🔧    | Chore    | `🔧 Chore: update export settings`      |
| 📝    | Docs     | `📝 Docs: add input system docs`        |
| 🧪    | Test     | `🧪 Test: add player movement tests`    |
| 🚀    | Perf     | `🚀 Perf: optimize tilemap rendering`   |
| ❌    | Remove   | `❌ Remove: unused assets`              |

Rules:

- Message in English, imperative, lowercase after type
- Keep short and conventional
- No trailing period
- NEVER use `Co-Authored-By` or any AI attribution
- NEVER use long AI-generated descriptions
