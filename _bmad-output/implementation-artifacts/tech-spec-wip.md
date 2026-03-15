---
title: 'Ball System'
slug: 'ball-system'
created: '2026-03-15'
status: 'complete'
stepsCompleted: [1, 2, 3, 4]
tech_stack: ['Unity 6.3 LTS', 'Physics2D', 'ObjectPool<T>']
files_to_modify:
  - 'Assets/_Project/Scripts/Ball/BallStats.cs'
  - 'Assets/_Project/Scripts/Ball/BallController.cs'
  - 'Assets/_Project/Scripts/Ball/BallManager.cs'
code_patterns: ['Singleton<T>', 'GameEvents', 'ObjectPool<T>', 'ScriptableObject']
test_patterns: []
---

# Tech-Spec: Ball System

**Created:** 2026-03-15

## Overview

### Problem Statement

Le jeu n'a aucune entité jouable. Le Ball System est le coeur du gameplay — des balles autonomes qui rebondissent dans une arène et infligent des dégâts aux ennemis.

### Solution

Créer un BallController (physique rebond), BallStats (SO données), BallManager (pool + spawn), et une arène basique avec 4 murs.

### Scope

**In Scope:**
- BallStats ScriptableObject (vitesse, dégâts, clamps)
- BallController MonoBehaviour (physique, rebond, collision ennemis)
- BallManager Singleton (pool, spawn, despawn)
- PhysicsMaterial2D (bounciness=1, friction=0)
- Speed clamp (min/max) dans FixedUpdate
- Collision avec ennemis → GameEvents.OnEnemyHit
- Spawn au centre, direction aléatoire 360°

**Out of Scope:**
- Upgrades de balles (UpgradeManager Phase 2)
- Trails / VFX (VFXManager Phase 3)
- Types de balles multiples (post-MVP)
- Power multiplier scaling (post-MVP)

## Context for Development

### Codebase Patterns

- Namespace : `BounceReaper`
- Singleton<T> base class dans Core/Singleton.cs
- GameEvents static dans Core/GameEvents.cs
- GameConfig SO dans Core/GameConfig.cs
- `[SerializeField] private` partout
- MonoBehaviour template 6 sections
- ObjectPool<T> natif Unity
- `rb.linearVelocity` (pas velocity — Unity 6)
- `OnCollisionEnter2D` (pas Trigger — rebond physique)

### Files to Reference

| File | Purpose |
| ---- | ------- |
| `Core/Singleton.cs` | Base class pour BallManager |
| `Core/GameEvents.cs` | OnEnemyHit, OnBallSpawned events + Raise() safe invoke |
| `Core/GameConfig.cs` | MaxVisualBalls config |
| `Core/GameConstants.cs` | Layer names, sort orders |
| `Data/GameState.cs` | Pour vérifier état jeu avant spawn |

### Technical Decisions

- **Rebond élastique parfait** : PhysicsMaterial2D (bounciness=1, friction=0) + Rigidbody2D (gravity=0, drag=0)
- **Continuous collision detection** : évite le tunneling à haute vitesse
- **Speed clamp dans FixedUpdate** : compense les micro-pertes numériques du physics engine
- **Pool par BallManager** : ObjectPool<T> natif, warm-up au Start
- **GameEvents.Raise** pour les events : exception isolation per-subscriber

## Implementation Plan

### Tasks

- [ ] **T1: BallStats.cs** — ScriptableObject dans `Scripts/Ball/`
  - Champs : BaseSpeed(8f), MinSpeed(6f), MaxSpeed(15f), BaseDamage(1f)
  - `[CreateAssetMenu]`, `[Header]`, `[Range]`, `OnValidate()`
  - Read-only properties

- [ ] **T2: BallController.cs** — MonoBehaviour dans `Scripts/Ball/`
  - `[SerializeField] private BallStats _stats`
  - Cached `Rigidbody2D _rb` dans Awake
  - `Initialize(BallStats stats)` : set stats, apply random velocity
  - `FixedUpdate` : speed clamp (min/max via linearVelocity)
  - `OnCollisionEnter2D` : si layer Enemy → calcul damage → GameEvents.Raise(OnEnemyHit)
  - `OnDisable` : DOTween.Kill(gameObject)
  - Sorting order = GameConstants.SortOrderBalls

- [ ] **T3: BallManager.cs** — Singleton dans `Scripts/Ball/`
  - `[SerializeField] private BallStats _defaultStats`
  - `[SerializeField] private BallController _ballPrefab`
  - `[SerializeField] private Transform _spawnPoint` (centre arène)
  - ObjectPool<BallController> avec warm-up
  - `SpawnBall()` : pool.Get, position spawnPoint, Initialize, GameEvents.Raise(OnBallSpawned)
  - `DespawnBall(BallController ball)` : pool.Release
  - `ActiveBallCount` property
  - Subscribe OnGameStateChanged pour cleanup

### Acceptance Criteria

- **AC1:** Given une balle spawnée, When elle est dans l'arène, Then elle rebondit indéfiniment sans perte de vitesse
- **AC2:** Given une balle en mouvement, When sa vitesse tombe sous MinSpeed, Then elle est reclampée à MinSpeed
- **AC3:** Given une balle en mouvement, When sa vitesse dépasse MaxSpeed, Then elle est clampée à MaxSpeed
- **AC4:** Given une balle, When elle entre en collision avec un objet layer Enemy, Then GameEvents.OnEnemyHit est fired avec le damage correct
- **AC5:** Given le BallManager, When SpawnBall() est appelé, Then une balle apparaît au centre avec direction aléatoire
- **AC6:** Given le BallManager, When ActiveBallCount >= GameConfig.MaxVisualBalls, Then pas de nouveau spawn

## Additional Context

### Dependencies

- Core/ (GameEvents, Singleton, GameConstants, GameConfig) — ✅ déjà implémenté
- PhysicsMaterial2D asset — à créer via Unity Editor ou MCP
- Prefab Ball — à créer avec SpriteRenderer + CircleCollider2D + Rigidbody2D + BallController
- Physics2D Layers (Ball, Enemy, Wall) — à configurer dans Project Settings

### Testing Strategy

- Test visuel : lancer Play mode, spawn une balle, vérifier qu'elle rebondit
- `Debug.Assert` sur _stats != null dans Initialize
- `Debug.Log` dans OnCollisionEnter2D pour vérifier les hits
- `DebugStats` OnGUI pour afficher ActiveBallCount

### Notes

- Les balles utilisent `GameObject` dans les events pour l'instant (refactor en type concret plus tard)
- PhysicsMaterial2D et Prefab doivent être créés dans Unity (pas via code)
- Layers Ball/Enemy/Wall doivent être configurés dans Project Settings > Tags and Layers
