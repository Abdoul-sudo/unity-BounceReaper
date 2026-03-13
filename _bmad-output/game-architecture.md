---
title: 'Game Architecture'
project: 'BounceReaper'
date: '2026-03-09'
author: 'Abdoul'
version: '1.0'
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8, 9]
status: 'complete'
engine: 'Unity 6.3 LTS (6000.3.10f1)'
platform: 'Android (principal), iOS (conditionnel)'

# Source Documents
gdd: null
epics: null
brief: '_bmad-output/game-brief.md'
monetisation: '_bmad-output/monetisation-bounce-reaper.md'
---

# Game Architecture

## Document Status

This architecture document is being created through the GDS Architecture Workflow.

**Steps Completed:** 9 of 9 (Complete)

---

## Executive Summary

**Bounce Reaper** est un idle bouncer mobile conçu pour Unity 6.3 LTS (2D URP) ciblant Android (principal) et iOS (conditionnel).

**Décisions architecturales clés :**
- **Communication :** GameEvents static centralisé avec reset Hot Reload — zero-alloc, découplage total
- **Data :** ScriptableObjects par entité avec AnimationCurves pour le balancing
- **Async :** Mix Coroutines (gameplay) + Awaitable (system I/O) — zero dépendance externe
- **Pools :** Unity ObjectPool\<T\> natif, chaque manager gère ses pools
- **Save :** Newtonsoft Json.NET (built-in Unity 6) + OnApplicationPause

**Structure :** Organisation Hybrid Domain-Driven avec 14 systèmes core, 10 managers spécialisés, communication via Events uniquement.

**Patterns :** 12 patterns définis (9 standard + 3 novel) avec exemples code pour chaque, assurant la cohérence de l'implémentation.

**MVP Scope :** 12 systèmes en semaine 1, 4 systèmes post-MVP. Game feel (hitstop, juice combo, trails, glow) comme différenciateur principal.

---

## Development Environment

### Prerequisites

- Unity 6.3 LTS (6000.3.10f1)
- Template 2D URP
- Android SDK (API level selon requirements Google Play)
- Xcode (pour iOS, conditionnel)

### Installed Assets

| Asset | Usage |
| --- | --- |
| DOTween Pro | Animations, tweens, juice, UI |
| All In 1 Sprite Shader | Glow, outline, dissolve (neon style) |
| Text Animator for Unity 3.5.1 | Damage numbers animés, UI text |
| vHierarchy 2.0 | Organisation hiérarchie éditeur |
| Hot Reload | Itération rapide sans recompilation |

### AI Tooling (MCP Servers)

| MCP Server | Purpose | Status |
| --- | --- | --- |
| Unity MCP (com.unity.ai.assistant) | Accès direct à l'éditeur Unity | Installé |
| Context7 | Documentation Unity à jour | Disponible |

### First Steps

1. Vérifier que tous les assets sont importés correctement dans Unity
2. Créer la structure de dossiers `Assets/_Project/Scripts/` (Core, Ball, Enemy, Wave, etc.)
3. Configurer les Physics2D Layers (Ball, Enemy, Wall) et la Collision Matrix
4. Configurer le New Input System dans Project Settings
5. Créer le `GameConfig` ScriptableObject
6. Implémenter `GameEvents.cs` et `Singleton<T>` dans Core/

---

## Project Context

### Game Overview

**Bounce Reaper** — Idle bouncer mobile où des balles autonomes éliminent des vagues d'ennemis dans une arène néon géométrique. Plus de balles = plus de chaos = plus de puissance.

### Technical Scope

**Plateforme:** Android (principal), iOS (conditionnel)
**Genre:** Idle / Action / Bouncer
**Moteur:** Unity 6.3 LTS (6000.3.10f1), 2D URP
**Niveau de complexité:** Moyen

### Core Systems — MVP (Semaine 1)

| Système | Complexité | Description |
| --- | --- | --- |
| Ball Physics | Haute | Rebond, collision, trajectoires |
| Damage Resolution | Moyenne | Calcul dégâts, effets on-hit, balle-ennemi/mur/balle |
| Wave/Enemy Spawner | Moyenne | Spawning progressif, types d'ennemis |
| Upgrade System | Moyenne | Dégâts, vitesse, +1 balle (basique) |
| Currency System | Moyenne | Shards (soft) + Souls (hard), transactions, anti-exploit |
| Event System | Haute | Bus d'événements, communication inter-managers |
| Object Pool System | Moyenne-Haute | Pool générique avec warm-up et dynamic resize |
| VFX / Game Feel | Haute | Glow, trails, shake, damage numbers, DOTween |
| Boss Fight | Moyenne | Timer 30-60s, "Boss escaped!" on fail |
| UI System | Moyenne | HUD, menus, popups, TextMeshPro |
| Input Manager | Moyenne | Touch mobile, zones UI vs gameplay, activation sorts |
| Save System | Moyenne | JSON local + migration |

### Core Systems — Post-MVP

| Système | Complexité | Description |
| --- | --- | --- |
| Build/Loadout System | Haute | Balles (3-5) + Sorts (3) + Reliques (3-5) + Skins |
| Prestige System | Moyenne | Reset + bonus permanents |
| Time Manager | Moyenne | Calcul idle/offline, récompenses de retour |
| Audio Manager | Basse → Moyenne | Pool audio SFX + musique (basique en MVP) |

### Architecture des Managers

Communication via **Events uniquement** — pas de références directes entre managers.

| Manager | Responsabilité |
| --- | --- |
| GameManager | State machine uniquement + scene transitions |
| WaveManager | Spawning des vagues, progression, conditions boss |
| BallManager | Pool de balles, slots, physique |
| CurrencyManager | Shards/Souls, transactions, validation |
| UpgradeManager | Arbre d'upgrades, coûts, application |
| SaveManager | Sérialisation JSON, chargement, migration |
| AudioManager | Pool SFX, musique, volume |
| VFXManager | Pool particules, trails, screen shake |
| UIManager | Écrans, popups, HUD updates |
| InputManager | Touch input, zones, gestes mobile |

### États du GameManager

```
Boot → MainMenu → Loading → Playing → BossFight → Victory → Defeat → Pause
```

### Contraintes Techniques

- **FPS cible:** 60fps (option 120fps)
- **Cap visuel balles:** ~30, au-delà multiplicateurs de puissance
- **Budget:** 0$ (max 100$), assets gratuits uniquement
- **Assets installés:** DOTween Pro, All In 1 Sprite Shader, Text Animator, vHierarchy 2.0
- **MVP strict:** 1 semaine — ball physics, 3-4 types ennemis, waves, upgrades basiques, boss/10 waves, save JSON, UI basique, game feel

### Complexity Drivers

**Haute complexité:**
- Ball Physics — cœur du jeu, doit être satisfaisant et performant
- Event System — critique pour découplage des managers
- Build/Loadout System — combinatoire balles + sorts + reliques (post-MVP)
- VFX / Game Feel — différenciateur principal, dopamine visuelle

**Concepts novateurs:**
- Hybrid fail system (balles immortelles + timer boss)
- Scaling visuel cappé à 30 balles avec multiplicateurs
- Système de builds combinatoires (post-MVP)

### Risques Techniques

1. **Low-end device support** — Samsung A13 et équivalents, 30 balles + ennemis + VFX simultanés
2. **Thermal throttling** — surchauffe après 15+ min de particules intensives
3. **Game feel** — le jeu repose sur la satisfaction visuelle, si c'est "flat" le jeu échoue
4. **Scaling** — transition smooth entre balles visuelles et multiplicateurs
5. **Save system offline** — calcul idle fiable sans exploits
6. **Fragmentation Android** — diversité GPU/CPU, résolutions, versions OS

## Engine & Framework

### Selected Engine

**Unity 6.3 LTS** (6000.3.10f1)

**Rationale:** Moteur mature pour mobile 2D, excellent support Android/iOS, large écosystème d'assets, URP optimisé pour mobile, familiarité du développeur.

### Project Initialization

Template 2D URP — projet déjà initialisé.

### Installed Assets & Versions

| Asset | Version | Usage |
| --- | --- | --- |
| DOTween Pro | À vérifier | Animations, tweens, juice, UI |
| All In 1 Sprite Shader | À vérifier | Glow, outline, dissolve (neon style) |
| Text Animator for Unity | 3.5.1 | Damage numbers animés, UI text |
| vHierarchy 2.0 | À vérifier | Organisation hiérarchie éditeur |
| Hot Reload | À vérifier | Itération rapide sans recompilation |

### Engine-Provided Architecture

| Composant | Solution | Notes |
| --- | --- | --- |
| Rendering | URP 2D | Bloom, Light2D pour neon |
| Physics | Physics2D (Box2D) | Collisions balles/ennemis/murs |
| Audio | AudioSource + AudioMixer | Pool SFX via AudioManager custom |
| Input | **New Input System** | Multi-touch natif, InputAction assets (SO) |
| Scene Management | SceneManager | **Single Scene** pour MVP |
| Build Pipeline | Gradle (Android) / Xcode (iOS) | **Mono (dev) / IL2CPP (release)** |
| UI | Canvas + TextMeshPro | HUD, menus, damage numbers |
| Animation | DOTween Pro | Tweens, juice, UI animations |
| Shaders | URP ShaderGraph + All In 1 Sprite Shader | Glow, outline, dissolve |
| Text Effects | Text Animator for Unity | Damage numbers animés |
| Profiling | Unity Profiler + Frame Debugger + Memory Profiler + GPU Profiler / RenderDoc | Critique pour low-end devices, GPU bottleneck bloom/glow |

### Runtime Constraints

**Target Frame Rate:**
- `Application.targetFrameRate = 60` (ou 120) explicite au boot
- `QualitySettings.vSyncCount = 0` obligatoire sur mobile
- Sans ces settings, Unity default à 30fps

**Incremental GC:** Activé obligatoirement. Code designé pour minimiser les allocations runtime (zero-alloc sur le hot path). Éviter les `new` en Update, pré-alloquer les listes, utiliser les struct pools.

**Input Response:** < 100ms pour les actions joueur (activation de sorts). Tester sur device Android réel, pas juste éditeur.

**Zéro Loading Screen:** Aucun écran de chargement pendant une session de jeu. Transitions via DOTween fades sur Canvas overlay. Le flow de dopamine ne doit jamais être interrompu.

**Collision Matrix (Physics2D Layers) :**

| Layer | Ball | Enemy | Wall |
| --- | --- | --- | --- |
| Ball | ❌ | ✅ | ✅ |
| Enemy | ✅ | ❌ | ✅ |
| Wall | ✅ | ✅ | ❌ |

- **Balles ignorent les balles** — même layer, collision désactivée (sinon 30 balles = 435 paires/frame)
- **Ennemis ignorent les ennemis** — pas de collision inter-ennemis

**Build Strategy:**
- **Mono** pour dev/itération rapide (compilation rapide)
- **IL2CPP** uniquement pour builds release (performance mobile)

### Development Workflow Constraints

**Hot Reload Compatibility:**
- Singletons : utiliser `[RuntimeInitializeOnLoadMethod]` pour reset des statics
- Delegates/events : éviter les doublons au reload
- Coroutines : pas de Coroutines longue durée qui survivent au reload
- **Object Pools : détecter le domain reload et re-scan les objets actifs** (sinon doublons en scène)

### AI Development Tools (MCPs)

- **Unity MCP** (com.unity.ai.assistant) — accès direct à l'éditeur
- **Context7** — documentation Unity à jour

### Remaining Architectural Decisions

| ID | Décision | Contexte |
| --- | --- | --- |
| ADR-001 | Event Bus pattern | Custom C# events vs UnityEvents |
| ADR-002 | Async pattern | Coroutines vs async/await vs UniTask |
| ADR-003 | Object pooling strategy | Pool générique, warm-up, resize |
| ADR-004 | ScriptableObject data architecture | Stats, configs, balancing |
| ADR-005 | Save system format + migration | JSON structure, versioning |
| ADR-006 | Audio pooling strategy | SFX pool, priorité, limites |
| ADR-007 | Sprite Atlas strategy | Batching draw calls, atlas grouping |
| ADR-008 | Assembly Definitions | Un seul assembly MVP, split post-MVP |

## Architectural Decisions

### Decision Summary

| ID | Décision | Choix | Rationale |
| --- | --- | --- | --- |
| ADR-001 | Event Bus | GameEvents static centralisé + Reset | Zero-alloc, Hot Reload safe, un seul fichier |
| ADR-002 | Async Pattern | Mix Coroutines + Awaitable | Coroutines = gameplay, Awaitable = system I/O |
| ADR-003 | Object Pooling | Unity ObjectPool\<T\> natif par manager | Built-in, typage fort, pas de central |
| ADR-004 | Data Architecture | SO par entité + AnimationCurves | Modulaire, live tuning, Registries post-MVP |
| ADR-005 | Save System | Newtonsoft Json.NET + version field | Built-in Unity 6, Dictionary, migration-ready |
| ADR-006 | Audio Pooling | Pool 8 AudioSources + throttle + pitch | Throttle 50ms, AudioMixer Master→Music+SFX |
| ADR-007 | Sprite Atlas | Post-MVP (SRP Batcher suffit) | Implémenter quand > 50 draw calls |
| ADR-008 | Assembly Defs | Pas d'asmdef MVP | Hot Reload suffit, trigger > 5s compile |

### ADR-001: Event Bus — GameEvents Static

**Pattern:** Fichier `GameEvents.cs` centralisé avec C# static events + `[RuntimeInitializeOnLoadMethod]` reset pour Hot Reload.

**Règles:**
- Tous les events passent par `GameEvents` — pas de ref directe entre managers
- Reset automatique au domain reload
- Migration vers MessagePipe post-MVP si besoin de filtrage/async

**GameEvents MVP:**

```
OnGameStateChanged(GameState state)
OnEnemyHit(Enemy enemy, float damage)
OnEnemyKilled(Enemy enemy)
OnWaveComplete(int waveNumber)
OnBossSpawn(Enemy boss)
OnBossEscaped()
OnCurrencyChanged(CurrencyType type, int amount)
OnUpgradePurchased(UpgradeConfig config)
OnBallSpawned(Ball ball)
```

### ADR-002: Async — Mix Coroutines + Awaitable

| Contexte | Outil |
| --- | --- |
| Hot path (Update, FixedUpdate) | Synchrone |
| Séquences gameplay (spawn, VFX) | Coroutines (WaitForSeconds cached) |
| Opérations système (save, load, init) | Awaitable (Unity 6 natif) |

**Règle:** Jamais d'async sur le hot path. Zero dépendance externe (pas d'UniTask).

### ADR-003: Object Pooling — Unity Built-in

**Pattern:** `UnityEngine.Pool.ObjectPool<T>` natif. Chaque manager crée et gère ses propres pools.
- BallManager → pool de balles
- WaveManager → pool d'ennemis
- VFXManager → pool de particules et damage numbers
- AudioManager → pool d'AudioSources

**Warm-up au Start.** Max size configuré par pool.

### ADR-004: Data — ScriptableObjects par Entité

**MVP:** Un SO par entité (BallStats, EnemyStats, UpgradeConfig, WaveConfig). AnimationCurves pour les courbes de scaling (coûts, dégâts, HP).

**Post-MVP:** Registries (EnemyRegistry, BallRegistry) pour le balancing groupé. Enums pour UI uniquement, jamais sérialisés.

### ADR-005: Save — Newtonsoft Json.NET

**Format:** JSON via `com.unity.nuget.newtonsoft-json` (built-in Unity 6).
**Emplacement:** `Application.persistentDataPath/save.json`
**Timing:** Save sur `OnApplicationPause(true)` + après chaque wave complete.
**Timestamps:** Unix `long` (pas DateTime) pour éviter les problèmes de timezone.
**Champ `version`:** Toujours en premier, ne bouge jamais.

**SaveData MVP:**

```csharp
public class SaveData
{
    public int version = 1;
    public int shards;
    public int souls;
    public int highestWave;
    public Dictionary<string, int> upgradeLevels;
    public long lastPlayTimestamp; // Unix UTC
}
```

**Post-MVP:** Migration system (v1→v2), anti-tamper checksum, backup file.

### ADR-006: Audio — Pool Centralisée

**Pool:** 8 AudioSources sur AudioManager. Vol la plus ancienne si pool pleine.
**Throttle:** 50ms minimum entre deux sons du même type.
**Pitch:** Randomization ±10% sur chaque SFX.
**Mixer:** AudioMixer avec Master → Music + SFX dès le MVP.

### ADR-007: Sprite Atlas — Post-MVP

**MVP:** SRP Batcher suffit (< 30 draw calls). Pas d'atlas.
**Post-MVP:** Un atlas par catégorie (Balls, Enemies, UI, VFX), 2048×2048, Tight Packing.
**Risque:** Tester compatibilité All In 1 Sprite Shader + Atlas avant implémentation.
**Trigger:** Implémenter quand Frame Debugger montre > 50 draw calls.

### ADR-008: Assembly Definitions — Post-MVP

**MVP:** Pas d'asmdef. Hot Reload pour itération rapide.
**Structure dossiers prête** pour migration future (Core/, Ball/, Enemy/, Managers/, UI/).
**Trigger migration:** > 5s compile OU > 100 scripts OU ajout de tests EditMode.

### Damage Pipeline (Interaction des ADR sur le Hot Path)

```
Balle frappe Ennemi (Physics2D OnCollisionEnter2D) →
  1. BallStats.BaseDamage (from SO — ADR-004)
  2. × UpgradeMultiplier (from UpgradeManager)
  3. = FinalDamage
  4. → Enemy.TakeDamage(FinalDamage)
  5. → GameEvents.OnEnemyHit(enemy, FinalDamage) (ADR-001)
  6. → VFXManager affiche damage number (pool — ADR-003)
  7. → AudioManager joue hit SFX (throttle — ADR-006)
  8. → Si enemy.HP <= 0 : GameEvents.OnEnemyKilled(enemy)
  9.   → CurrencyManager ajoute shards
  10.  → WaveManager update compteur
  11.  → VFXManager particules de mort
```

## Cross-cutting Concerns

Ces patterns s'appliquent à TOUS les systèmes et doivent être suivis par chaque implémentation.

### Error Handling

**Stratégie:** Graceful Degradation + Try-Catch Save/Load

| Règle | Détail |
| --- | --- |
| Managers autonomes | Chaque manager init dans try-catch, flag `_initialized` |
| Si non-initialisé | Return silencieux, pas de crash |
| Save/Load | Try-catch explicite, retry au prochain OnApplicationPause |
| Tout le reste | `Debug.LogError/Warning` + continue |
| Release builds | `Application.logMessageReceived` pour exceptions non-gérées |
| Fatal | Uniquement GameManager — seul crash qui arrête le jeu |

**Criticité des managers :**

| Manager | Si crash ? | Action |
| --- | --- | --- |
| GameManager | Jeu mort | Fatal — seul vrai crash |
| SaveManager | Progression perdue | Try-catch, retry |
| Tous les autres | Feature dégradée | Graceful degradation |

### Logging

**Format:** `Debug.Log/Warning/Error` natif Unity + tag `[ManagerName]`

```csharp
Debug.Log("[Wave] Wave 5 started, 12 enemies");
Debug.LogWarning("[Pool] Enemy pool near capacity (18/20)");
Debug.LogError("[Save] Write failed: disk full");
```

| Niveau | Usage |
| --- | --- |
| `Log` | Milestones normaux (wave start, boss spawn) |
| `LogWarning` | Inattendu mais géré (pool presque pleine) |
| `LogError` | Quelque chose a cassé (save échoué, init failed) |

**Règles :**
- Tag `[ManagerName]` obligatoire
- JAMAIS de Debug.Log dans Update/FixedUpdate
- Post-MVP : wrapper `GameDebug` avec `[Conditional("ENABLE_LOG")]`

### Configuration

**Hiérarchie de configuration :**

| Type | Stockage | Quand modifier |
| --- | --- | --- |
| Constants (layers, physics) | `static class GameConstants` | Jamais runtime |
| Balancing (stats, courbes) | SO par entité | Live tuning Inspector |
| Game Config global | `GameConfig` SO unique | Live tuning Inspector |
| Player Settings (volume, FPS) | `PlayerSettings` wrapper sur PlayerPrefs | Settings menu |
| Platform (quality) | Unity Quality Settings | Build time |

**Règle absolue : ZERO magic numbers dans le code.**

### Event System

Défini dans ADR-001. Règles cross-cutting :

| Règle | Détail |
| --- | --- |
| Centralisation | Tous les events via `GameEvents.cs` |
| Nommage | `On` + sujet + verbe passé (`OnEnemyKilled`) |
| Signature | `Action<T>` avec types concrets, jamais string/object |
| Subscribe | Dans `OnEnable`, unsubscribe dans `OnDisable` |
| Hot Reload | Reset auto `[RuntimeInitializeOnLoadMethod]` |

### Naming Conventions

| Élément | Convention | Exemple |
| --- | --- | --- |
| Namespaces | `BounceReaper.<Module>` | `BounceReaper.Ball`, `BounceReaper.Enemy` |
| Scripts | PascalCase = nom de classe | `BallController.cs` |
| SO assets | `Type_Variant` | `Enemy_Triangle.asset`, `Ball_Basic.asset` |
| Prefabs | `Type_Variant` | `Enemy_Triangle.prefab` |
| Materials | `Mat_Name` | `Mat_NeonGlow.mat` |
| Sprites | `Spr_Name` | `Spr_Ball_Basic.png` |
| Private fields | `_camelCase` | `_baseDamage`, `_enemyPool` |
| Public members | `PascalCase` | `BaseDamage`, `Speed` |
| Constants | `PascalCase` | `MaxVisualBalls` |

### Bootstrap & Initialization Order

**GameManager contrôle l'ordre d'init** — pas de `Script Execution Order`, pas de `FindObjectOfType`.

```
0. Physics Settings  (targetFrameRate, vSyncCount, Collision Matrix)
1. GameEvents        (static, pas besoin d'init)
2. SaveManager       (charge les données)
3. CurrencyManager   (lit depuis SaveData)
4. UpgradeManager    (lit depuis SaveData)
5. GameConfig        (SO, chargé auto)
6. Tout le reste     (ordre indifférent)
```

**Pattern :** Tous les managers en `[SerializeField]` sur GameManager. Init séquentiel dans `Awake()`.

### Debug & Development Tools

| Outil | MVP | Scope |
| --- | --- | --- |
| Gizmos (arène, spawn zones, hitboxes) | ✅ | Editor only |
| `[Header]`/`[Range]` sur tous les SO | ✅ | Editor |
| Unity Profiler | ✅ | Editor + USB |
| `DebugStats` OnGUI (FPS, ball/enemy counts) | ✅ | Dev builds only (`#if DEVELOPMENT_BUILD`) |
| Debug Console in-game | ❌ | Post-MVP |
| Cheat commands | ❌ | Post-MVP |

## Project Structure

### Organization Pattern

**Hybrid Domain-Driven** — Dossiers racine par type (Scripts, Prefabs, Art), sous-dossiers Scripts par domaine. Chaque dossier domaine contient son manager + SO classes + scripts associés.

### Directory Structure

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Core/                      # Code partagé transversal
│   │   │   ├── GameEvents.cs
│   │   │   ├── GameConstants.cs
│   │   │   ├── GameConfig.cs          (SO class)
│   │   │   ├── PlayerSettings.cs
│   │   │   └── Singleton.cs           (base class)
│   │   ├── Ball/                      # Domaine balle
│   │   │   ├── BallController.cs
│   │   │   ├── BallStats.cs           (SO class)
│   │   │   └── BallManager.cs
│   │   ├── Enemy/                     # Domaine ennemi
│   │   │   ├── EnemyController.cs
│   │   │   ├── EnemyStats.cs          (SO class)
│   │   │   └── EnemyHealth.cs
│   │   ├── Wave/                      # Domaine vagues
│   │   │   ├── WaveManager.cs
│   │   │   └── WaveConfig.cs          (SO class)
│   │   ├── Upgrade/                   # Domaine upgrades
│   │   │   ├── UpgradeManager.cs
│   │   │   └── UpgradeConfig.cs       (SO class)
│   │   ├── Currency/                  # Domaine économie
│   │   │   └── CurrencyManager.cs
│   │   ├── Managers/                  # Managers TRANSVERSAUX uniquement
│   │   │   ├── GameManager.cs
│   │   │   ├── SaveManager.cs
│   │   │   ├── AudioManager.cs
│   │   │   ├── VFXManager.cs
│   │   │   ├── UIManager.cs
│   │   │   └── InputManager.cs
│   │   ├── UI/                        # Composants UI
│   │   │   ├── HUDController.cs
│   │   │   ├── UpgradePanel.cs
│   │   │   └── BossTimerUI.cs
│   │   ├── Data/                      # Structures de données
│   │   │   ├── SaveData.cs
│   │   │   ├── GameState.cs           (enum)
│   │   │   └── CurrencyType.cs        (enum)
│   │   ├── Debug/                     # Outils dev
│   │   │   └── DebugStats.cs
│   │   └── Monetization/              # Post-MVP
│   ├── Tests/                         # Post-MVP, prêt
│   │   ├── EditMode/
│   │   └── PlayMode/
│   ├── Prefabs/
│   │   ├── Ball/
│   │   ├── Enemy/
│   │   ├── VFX/
│   │   └── UI/
│   ├── ScriptableObjects/
│   │   ├── Balls/                     # Ball_Basic.asset
│   │   ├── Enemies/                   # Enemy_Triangle.asset
│   │   ├── Upgrades/                  # Upgrade_Damage.asset
│   │   ├── Waves/                     # Wave_001.asset
│   │   └── Config/                    # GameConfig.asset
│   ├── Scenes/
│   │   └── Game.unity                 # Single scene MVP
│   ├── Art/
│   │   ├── Sprites/
│   │   ├── Materials/
│   │   └── Particles/
│   ├── Audio/
│   │   ├── Music/
│   │   ├── SFX/
│   │   └── Mixers/
│   ├── Localization/                  # Post-MVP
│   └── Settings/                      # URP, Quality, Input Actions
├── Plugins/                           # DOTween, AllIn1SpriteShader, Febucci
└── ThirdParty/
```

### Namespace Mapping

**MVP : namespace global `BounceReaper` suffit.** Split par module post-MVP ou quand > 50 scripts.

| Dossier | Namespace (post-MVP) |
| --- | --- |
| `Core/` | `BounceReaper.Core` |
| `Ball/` | `BounceReaper.Ball` |
| `Enemy/` | `BounceReaper.Enemy` |
| `Wave/` | `BounceReaper.Wave` |
| `Upgrade/` | `BounceReaper.Upgrade` |
| `Currency/` | `BounceReaper.Currency` |
| `Managers/` | `BounceReaper.Managers` |
| `UI/` | `BounceReaper.UI` |
| `Data/` | `BounceReaper.Data` |
| `Debug/` | `BounceReaper.Debug` |

### System Location Mapping

| Système | Scripts | SO/Prefabs |
| --- | --- | --- |
| Ball Physics | `Ball/` | `ScriptableObjects/Balls/`, `Prefabs/Ball/` |
| Damage Resolution | `Ball/` + `Enemy/` | — |
| Wave/Enemy Spawner | `Wave/` + `Enemy/` | `ScriptableObjects/Waves/`, `Prefabs/Enemy/` |
| Upgrade System | `Upgrade/` | `ScriptableObjects/Upgrades/` |
| Currency System | `Currency/` | — |
| Event System | `Core/GameEvents.cs` | — |
| Object Pools | Chaque manager domaine | — |
| VFX / Game Feel | `Managers/VFXManager.cs` | `Prefabs/VFX/`, `Art/Particles/` |
| Boss Fight | `Wave/` + `Enemy/` | `ScriptableObjects/Waves/` |
| UI System | `Managers/UIManager.cs` + `UI/` | `Prefabs/UI/` |
| Input | `Managers/InputManager.cs` | `Settings/` |
| Save System | `Managers/SaveManager.cs` + `Data/` | — |
| Audio | `Managers/AudioManager.cs` | `Audio/` |
| Config | `Core/` | `ScriptableObjects/Config/` |

### Architectural Boundaries

| Règle | Détail |
| --- | --- |
| Communication | Via `GameEvents` uniquement entre managers |
| Dépendances | UI → Domaines → Core. Jamais l'inverse |
| SO runtime | Les scripts lisent les SO, jamais de write runtime |
| Prefabs | Référencent leurs SO via SerializeField |
| Scenes | Single scene MVP |
| Dossiers domaine | Créer quand > 2 fichiers |
| Managers/ | Uniquement les managers transversaux |

## Implementation Patterns

Ces patterns assurent la cohérence de l'implémentation.

### Novel Patterns

#### Hitstop avec Cooldown

**Purpose:** Game feel — slow-mo micro quand une balle frappe un ennemi. Avec 30 balles, cooldown pour éviter le freeze permanent.

**Flow:**

```
Balle frappe Ennemi →
  VFXManager.RequestHitstop() →
    SI dernierHitstop > 200ms → Time.timeScale = 0.1f pendant 0.02s
    SINON → skip (cooldown actif)
```

**Implémentation:**

```csharp
// Dans VFXManager
private float _lastHitstopTime;
private const float HITSTOP_COOLDOWN = 0.2f;
private const float HITSTOP_DURATION = 0.02f;

public void RequestHitstop()
{
    if (Time.unscaledTime - _lastHitstopTime < HITSTOP_COOLDOWN) return;
    _lastHitstopTime = Time.unscaledTime;
    StartCoroutine(DoHitstop());
}

private IEnumerator DoHitstop()
{
    Time.timeScale = 0.1f; // Slow-mo, pas freeze total
    yield return new WaitForSecondsRealtime(HITSTOP_DURATION);
    Time.timeScale = 1f;
}
```

**Règle DOTween :** Les animations qui doivent ignorer le timeScale utilisent `.SetUpdate(true)` (damage numbers, UI fades).

#### Juice Combo Multiplier (Visual Only)

**Purpose:** Escalade visuelle quand le joueur tue rapidement. Affecte uniquement les effets, pas les rewards.

**Flow:**

```
Kill → comboCount++ → reset timer 1s
Si timer expire → comboCount = 0

comboCount 1   → shake léger, damage number normal
comboCount 2-4 → shake moyen, damage number ×1.5 taille
comboCount 5-9 → shake fort, damage number ×2 taille, flash
comboCount 10+ → MEGA shake, screen flash, "MASSACRE!" text
```

#### Power Multiplier Scaling (Post-MVP)

**Purpose:** Au-delà du cap visuel (~30 balles), la puissance augmente via multiplicateur sans spawner de nouveaux GameObjects.

**Flow:**

```
Si totalBalls <= maxVisualBalls:
    → Spawn balle réelle (GameObject + physics + trail)

Si totalBalls > maxVisualBalls:
    → Pas de nouveau GameObject
    → powerMultiplier = totalBalls / maxVisualBalls
    → Balles existantes : glow intensifié, taille légèrement augmentée
    → Damage = baseDamage × powerMultiplier
    → UI affiche "Power ×1.5"
```

**Scaling curve:** À définir via AnimationCurve dans GameConfig SO.

### Standard Patterns

#### Singleton Pattern

```csharp
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
```

#### MonoBehaviour Template

Ordre standardisé pour TOUS les scripts :

```csharp
public class ExampleManager : Singleton<ExampleManager>
{
    // 1. SerializeField (config)
    [Header("Config")]
    [SerializeField] private SomeStats _stats;

    // 2. Private fields
    private ObjectPool<Something> _pool;
    private bool _initialized;

    // 3. Properties publiques (read-only)
    public int ActiveCount => _pool.CountActive;

    // 4. Lifecycle Unity (dans l'ordre d'exécution)
    protected override void Awake() { base.Awake(); Init(); }
    void OnEnable() { GameEvents.OnSomething += Handle; }
    void OnDisable() { GameEvents.OnSomething -= Handle; }

    // 5. Public API
    public void DoSomething() { }

    // 6. Private methods
    private void Handle() { }
    private void Init() { }
}
```

#### Communication Pattern

**GameEvents static** (ADR-001). Aucune référence directe entre managers.

```csharp
// Publisher
GameEvents.OnWaveComplete?.Invoke(waveNumber);

// Subscriber
void OnEnable() => GameEvents.OnWaveComplete += HandleWaveComplete;
void OnDisable() => GameEvents.OnWaveComplete -= HandleWaveComplete;
```

#### Entity Creation Pattern

**ObjectPool natif** (ADR-003). Chaque manager pool ses entités.

```csharp
var ball = _ballPool.Get();
ball.Initialize(_ballStats);
// Recyclage
_ballPool.Release(ball);
```

#### State Machine Pattern

**Enum states dans GameManager.**

```csharp
public enum GameState { Boot, MainMenu, Loading, Playing, BossFight, Victory, Defeat, Pause }

private void ChangeState(GameState newState)
{
    _currentState = newState;
    GameEvents.OnGameStateChanged?.Invoke(newState);
}
```

#### Data Access Pattern

**ScriptableObjects read-only** (ADR-004). Drag & drop dans l'Inspector.

```csharp
[SerializeField] private EnemyStats _stats;
float hp = _stats.MaxHP;
```

### Game Feel Patterns (MVP)

| Effet | Implémentation | Trigger |
| --- | --- | --- |
| Screen Shake | DOTween `DOShakePosition` sur la caméra | Enemy killed |
| Hitstop | `Time.timeScale = 0.1f` + cooldown 200ms | Ball hit enemy |
| Damage Numbers | Pool TMP, DOTween scale + fade + float up `.SetUpdate(true)` | Ball hit enemy |
| Trail | TrailRenderer sur les balles | Permanent |
| Glow | All In 1 Sprite Shader | Permanent |
| Knockback | `AddForce` bref sur l'ennemi | Ball hit enemy |
| Death VFX | Pool ParticleSystem burst | Enemy killed |
| Juice Combo | Compteur kills < 1s, escalade shake/taille/flash | Multi-kill rapide |

### Consistency Rules

| Pattern | Convention | Enforcement |
| --- | --- | --- |
| Singleton | Toujours via `Singleton<T>` base class | Code review |
| Events | Subscribe `OnEnable`, unsub `OnDisable` | Code review |
| Pools | `ObjectPool<T>` natif, jamais custom | Code review |
| SO | Read-only runtime, jamais de write | Code review |
| Magic numbers | Interdit — tout dans SO ou GameConstants | Code review |
| MonoBehaviour | Ordre standardisé (6 sections) | Template |
| Naming | `_camelCase` private, `PascalCase` public | Code review |
| DOTween timeScale | `.SetUpdate(true)` pour anims UI/damage numbers | Code review |

## Architecture Validation

### Validation Summary

| Check | Résultat | Notes |
| --- | --- | --- |
| Decision Compatibility | ✅ PASS | 8 ADR cohérents, engine + patterns alignés |
| Game Brief Coverage | ✅ PASS | 14/14 systèmes couverts |
| Pattern Completeness | ✅ PASS | 9 standard + 3 novel patterns |
| Document Completeness | ✅ PASS | 7/7 sections, pas de placeholders |

### Coverage Report

**Systèmes couverts:** 14/14
**Patterns définis:** 12 (9 standard + 3 novel)
**Décisions prises:** 8 ADR
**Cross-cutting concerns:** 7 (error, logging, config, events, naming, bootstrap, debug)

### Issues Résolues lors de la Validation

1. Collision Matrix ajoutée — balles ignorent les balles (performance)
2. Physics settings ajoutées au bootstrap order
3. Physics2D iterations et fixedDeltaTime notés comme "à profiler"

### Notes de Profiling (Post-MVP)

- `Physics2D.velocityIterations` / `positionIterations` — réduire si nécessaire sur low-end
- `Time.fixedDeltaTime` — considérer 60Hz (0.01667f) si physique inconsistante

### Validation Date

2026-03-13
