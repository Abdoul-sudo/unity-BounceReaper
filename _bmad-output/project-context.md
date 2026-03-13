---
project_name: 'BounceReaper'
user_name: 'Abdoul'
date: '2026-03-13'
sections_completed: ['technology_stack', 'engine_rules', 'performance_rules', 'code_organization', 'testing_rules', 'platform_build', 'critical_rules']
status: 'complete'
rule_count: 95
optimized_for_llm: true
---

# Project Context for AI Agents

_This file contains critical rules and patterns that AI agents must follow when implementing game code in this project. Focus on unobvious details that agents might otherwise miss._

---

## Technology Stack & Versions

### Game Engine

- **Unity 6.3 LTS** (6000.3.10f1) — Template 2D URP
- **URP** 17.3.0 — SRP Batcher ON par défaut
- **Platform:** Android (principal), iOS (conditionnel)

### Key Packages (UPM)

| Package | Version | Notes |
| --- | --- | --- |
| Input System | 1.18.0 | Active Input Handling = "Both" ou "New" dans Player Settings |
| URP | 17.3.0 | SRP Batcher actif — materials custom peuvent casser le batching |
| Physics2D | Built-in | Box2D, Collision Matrix configurée |
| TextMeshPro (ugui) | 2.0.0 | Tout texte via TMP, jamais legacy Text |
| Test Framework | 1.6.0 | Post-MVP — ne pas créer de tests sauf story explicite |
| Newtonsoft Json.NET | **À ajouter** | `com.unity.nuget.newtonsoft-json` — requis pour SaveManager |

### Imported Assets (Plugins/, pas UPM)

| Asset | Usage |
| --- | --- |
| DOTween Pro | `using DG.Tweening;` — tweens, juice, UI |
| All In 1 Sprite Shader | Glow, outline, dissolve — peut casser SRP Batcher si mal configuré |
| Text Animator for Unity 3.5.1 | Damage numbers animés, UI text |
| Hot Reload | Itération rapide — incompatibilité PlayerInput (re-enable après reload) |
| vHierarchy 2.0 | Organisation hiérarchie éditeur |
| Music Pack | Musique de jeu |

### Packages hérités du template (NE PAS UTILISER)

- `com.unity.2d.animation`, `com.unity.2d.spriteshape`, `com.unity.2d.aseprite`, `com.unity.2d.psdimporter` — hérités du 2D template, pas utilisés par le projet

### Editor-Only Tools

- **Unity MCP** (`com.unity.ai.assistant` 2.0.0-pre.1) — outil éditeur, JAMAIS de dépendance runtime
- **Context7** — documentation Unity à jour

## Critical Implementation Rules

### Engine-Specific Rules (Unity 6.3 LTS)

#### Lifecycle

- Subscribe events dans `OnEnable`, unsub dans `OnDisable` — jamais dans Awake/Start
- Ordre MonoBehaviour : SerializeFields → Private fields → Properties → Lifecycle → Public API → Private methods
- Tous les managers héritent de `Singleton<T>` avec reset via `OnDestroy`
- Bootstrap séquentiel dans `GameManager.Awake()` — pas de Script Execution Order
- **JAMAIS `DontDestroyOnLoad`** — MVP single scene, sinon doublons silencieux

#### Serialization

- `[SerializeField] private` partout — jamais de `public` fields
- ScriptableObjects = read-only runtime, jamais de write
- `[Header]` et `[Range]` sur tous les SO pour tuning Inspector
- Enums pour UI only, jamais sérialisés dans les saves

#### Physics2D

- Collision Matrix : Ball-Ball OFF, Enemy-Enemy OFF
- **Layers obligatoires** : Ball, Enemy, Wall — setter sur chaque prefab sinon collision matrix ignorée
- `targetFrameRate = 60` + `vSyncCount = 0` au boot (sinon Unity default 30fps)
- **`OnCollisionEnter2D`** (pas Trigger) — balles = Colliders pour rebond physique
- **Rigidbody2D balles** : Dynamic, Gravity Scale = 0, Linear Drag = 0
- Incremental GC activé obligatoirement

#### Code Patterns

- Jamais de `GetComponent()` ou `Find()` dans Update/FixedUpdate
- **`FindFirstObjectByType`** au lieu de `FindObjectOfType` (deprecated Unity 6)
- **Cacher `WaitForSeconds`** dans un champ privé — jamais `new` dans une Coroutine loop
- **String interpolation** `$""` au lieu de concaténation `+` dans les logs
- Zero `new` sur le hot path — pré-allouer listes, réutiliser structs

#### DOTween

- **`.SetUpdate(true)`** obligatoire pour animations hors-gameplay (damage numbers, UI fades)
- Sans ce flag, le hitstop (timeScale=0.1f) ralentit ces animations

#### Async

- Coroutines = séquences gameplay (spawn, VFX chains)
- Awaitable (Unity 6 natif) = opérations système (save, load, init)
- JAMAIS d'async sur le hot path (Update, FixedUpdate)
- Zero dépendance externe (pas d'UniTask)

#### Hot Reload

- Singletons : `OnDestroy` reset le static Instance
- Events : `OnDisable` unsub évite les doublons
- Object Pools : détecter domain reload, re-scan objets actifs
- PlayerInput component : re-enable après Hot Reload
- Pas de Coroutines longue durée qui survivent au reload

### Performance Rules

#### Frame Budget (60fps = 16.67ms)

- Physics2D : < 4ms
- Scripts (Update + events) : < 4ms
- Rendering (URP + VFX) : < 6ms
- Overhead (GC, audio, input) : < 2.67ms
- Device de référence low-end : Samsung A13 et équivalents
- Option 120fps sur devices haut de gamme

#### Hot Path (Update/FixedUpdate)

- Zero allocation — pas de `new`, pas de LINQ, pas de `foreach` qui alloue
- Pas de `Debug.Log` dans Update/FixedUpdate
- Pas de `GetComponent`/`Find` — cacher les références dans Awake/Start
- **`Transform.SetPositionAndRotation()`** au lieu de setter `.position` puis `.rotation` séparément
- **`ContactFilter2D` + `List<RaycastHit2D>` pré-alloués** pour physics checks
- **`Mathf.Approximately`** au lieu de `==` pour comparer des floats
- Collision : ~30 balles max visuelles, Ball-Ball OFF (sinon 435 paires/frame)

#### Object Pooling

- `UnityEngine.Pool.ObjectPool<T>` natif — jamais de pool custom
- Chaque manager gère ses propres pools (BallManager, WaveManager, VFXManager, AudioManager)
- Warm-up au `Start()` — max size configuré par pool
- **`ParticleSystem.Clear()`** avant `Play()` sur particules poolées
- Pooler : balles, ennemis, damage numbers, particules, AudioSources

#### VFX Budget (simultanés max)

- Damage numbers : **8** (au-delà, skip les plus anciens)
- Death VFX : **4** (au-delà, version simplifiée)
- Screen shake : **1** (dernier gagne)
- Configurable via `GameConfig` SO
- **VFXManager doit checker `_activeCount < _maxCount` avant chaque spawn**

#### Memory & GC

- Incremental GC obligatoire
- Cacher `WaitForSeconds` instances
- Pré-allouer les listes (`List<T>(capacity)`)
- Structs pour données temporaires sur le hot path
- **Opérations qui alloquent = hors gameplay actif** (save sur OnApplicationPause, spawn en début de wave)

#### Rendering

- SRP Batcher ON — ne pas casser avec des materials incompatibles
- Sprite Atlas post-MVP (trigger : > 50 draw calls au Frame Debugger)
- Bloom/Light2D = GPU cost — profiler sur low-end avant d'intensifier
- **TMP damage numbers** : set texte UNE FOIS au spawn, animer uniquement le transform

#### Mobile-Specific

- **Thermal throttling** : risque documenté après 15+ min — quality scaler post-MVP
- `Application.targetFrameRate` = hint, pas garantie
- Profiler USB sur device réel obligatoire avant ship

#### Profiling

- Unity Profiler + Frame Debugger obligatoires avant optimisation
- GPU Profiler / RenderDoc pour bloom/glow bottlenecks
- `DebugStats` OnGUI (FPS, ball/enemy counts) en `#if DEVELOPMENT_BUILD`

### Code Organization Rules

#### Script Placement

- Hybrid Domain-Driven : dossiers par domaine sous `Assets/_Project/Scripts/`
- Chaque domaine (Ball/, Enemy/, Wave/, Upgrade/, Currency/) contient son manager + SO classes + scripts
- `Managers/` = uniquement managers transversaux (GameManager, SaveManager, AudioManager, VFXManager, UIManager, InputManager)
- `Core/` = code partagé (GameEvents, GameConstants, GameConfig, Singleton, PlayerSettings)
- `Data/` = structures de données et enums
- `UI/` = composants UI purs
- `Debug/` = outils dev
- **Pas de script à la racine de `Scripts/`** — tout dans un sous-dossier

#### Règle de placement nouveau code

- Script touche UN domaine → dossier domaine
- Script touche PLUSIEURS domaines → `Core/`
- Script purement UI → `UI/`
- MonoBehaviour attaché à un prefab → même dossier que le domaine du prefab
- Interfaces et classes abstraites → `Core/Interfaces/`
- Extensions → `Core/Extensions/`
- Helpers → `Core/Helpers/`

#### Enums

- **Un enum par fichier** dans `Data/`, nommé exactement comme l'enum
- Jamais d'enum inline dans un autre script

#### ScriptableObjects : classes vs instances

- **Classes SO** (BallStats.cs) → `Scripts/{Domain}/`
- **Instances SO** (Ball_Basic.asset) → `ScriptableObjects/{Domain}/`

#### Asset Organization

- `ScriptableObjects/` par catégorie : Balls/, Enemies/, Upgrades/, Waves/, Config/
- `Prefabs/` par type : Ball/, Enemy/, VFX/, UI/
- `Art/` : Sprites/, Materials/, Particles/
- `Audio/` : Music/, SFX/, Mixers/
- Single scene MVP : `Scenes/Game.unity`

#### Naming

- Namespaces : `BounceReaper` global pour MVP, split par module post-MVP
- Scripts : PascalCase = nom de classe, un MonoBehaviour par fichier
- Composants : `{Domain}{Component}` — pas de suffixe sauf ambiguïté
- SO assets : `Type_Variant` (Enemy_Triangle.asset)
- Prefabs : `Type_Variant` (Enemy_Triangle.prefab)
- Materials : `Mat_Name`, Sprites : `Spr_Name`
- Private : `_camelCase`, Public : `PascalCase`, Constants : `PascalCase`

#### Boundaries

- Communication via `GameEvents` uniquement entre managers
- Dépendances : UI → Domaines → Core (jamais l'inverse)
- SO read-only runtime
- Créer un dossier domaine quand > 2 fichiers

#### Unity File System

- **`.meta` files** : jamais supprimer, jamais créer manuellement — Unity les gère
- **Ordre de création scripts** : Core/ et Data/ d'abord, puis domaines, puis managers
- **Dossiers** : créer au fur et à mesure, pas tous d'un coup (sauf Core/ et Data/ dès le départ)

#### Testing (post-MVP)

- `Tests/EditMode/{Domain}/` et `Tests/PlayMode/{Domain}/` — mirror la structure Scripts/
- Nommage : `{ClassName}Tests.cs`
- Un fichier test par classe testée
- Les tests nécessiteront des assembly definitions (.asmdef) même si le code prod n'en a pas

### Testing & Defensive Coding Rules

#### Debug Assertions (MVP — zero cost en release)

- **`Debug.Assert()`** obligatoire dans les points critiques :
  - SO assignments : `Debug.Assert(_stats != null, $"[Ball] BallStats SO not assigned on {gameObject.name}");`
  - Pool init : `Debug.Assert(_pool != null, $"[VFX] Pool not initialized before use");`
  - Damage values : `Debug.Assert(damage > 0, $"[Damage] Negative damage: {damage}");`
- Compilé out en release builds — aucun coût perf

#### OnValidate() sur les ScriptableObjects

- Clamper les valeurs dans `OnValidate()` pour empêcher les valeurs absurdes :
  - `if (_maxHP <= 0) _maxHP = 1;`
  - `if (_speed < 0) _speed = 0;`
- Se déclenche quand un champ est modifié dans l'Inspector

#### Static Fields Reset

- **Tout champ static mutable** DOIT avoir un reset via `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`
- Sans ça, Hot Reload garde les anciennes valeurs et crée des bugs silencieux

#### Manager Init Validation

- Chaque manager valide ses dépendances dans `Awake()` via `Debug.Assert`
- Pattern **`_initialized` flag** : chaque manager a `private bool _initialized;`
- Toute méthode publique de manager doit vérifier `_initialized` avant d'exécuter

#### Formal Testing (post-MVP)

- Test Framework 1.6.0 installé, activé quand story le demande
- EditMode = logique pure (calculs dégâts, currency, upgrade costs)
- PlayMode = gameplay intégration (collision, spawning, events)
- Jamais mocker les SO — utiliser de vraies instances de test

### Platform & Build Rules

#### Target Platforms

- **Android** (principal) — Min SDK API 24 (Android 7.0)
- **iOS** (conditionnel — si MacBook disponible)

#### Build Strategy

- **Editor** : Mono, Hot Reload actif, pas d'optimisation
- **Dev Build** : Mono, `DEVELOPMENT_BUILD` défini, profiler connectable
- **Release** : IL2CPP, stripping activé, `Debug.Assert`/`Debug.Log` compilés out

#### IL2CPP & Code Stripping

- **`link.xml`** obligatoire pour préserver les types sérialisés par réflexion
- Newtonsoft Json.NET + `SaveData` (Dictionary<string, int>) nécessitent une entrée link.xml
- Tout type désérialisé par réflexion doit y être listé

#### Input

- **InputAction abstraites** via New Input System — jamais `Input.GetMouseButtonDown()` ou `Input.GetTouch()`
- Touch et Mouse gérés automatiquement par le Input System
- Input response < 100ms pour activation sorts
- Zones UI vs gameplay séparées

#### Mobile Lifecycle

- **`OnApplicationPause(true)`** : sauvegarder immédiatement
- **`OnApplicationPause(false)`** : recalculer temps idle, pas juste reprendre
- **`OnApplicationFocus(false)`** : gérer aussi — sur certains Android, focus perdu AVANT pause
- **Back button Android** : gérer (pause menu ou confirmation quit, jamais ignorer)

#### Screen & Display

- **`Screen.safeArea`** : appliquer au Canvas HUD pour notches, punch-holes, coins arrondis
- `Application.persistentDataPath` dynamique à chaque accès, jamais hardcodé

#### Platform Conditionals

- `#if UNITY_ANDROID` / `#if UNITY_IOS` pour code platform-specific
- `#if DEVELOPMENT_BUILD` pour DebugStats et outils dev
- `Application.platform` pour runtime checks

### Critical Don't-Miss Rules

#### Anti-Patterns INTERDITS

- `public` fields — toujours `[SerializeField] private`
- `GetComponent()`/`Find()` dans Update — cacher dans Awake
- `DontDestroyOnLoad` — MVP single scene
- Magic numbers dans le code — tout dans SO ou GameConstants
- Référence directe entre managers — GameEvents only
- `FindObjectOfType` — deprecated Unity 6, utiliser `FindFirstObjectByType`
- `new` sur le hot path — pré-allouer
- Legacy UI `Text` — toujours TextMeshPro
- String tags (`CompareTag("Enemy")`) — utiliser layers ou SO references
- `rb.velocity` — deprecated Unity 6, utiliser **`rb.linearVelocity`**

#### Event Safety

- **JAMAIS de lambda sur GameEvents** — impossible à unsub, memory leak + double-call après Hot Reload
- Toujours des méthodes nommées pour subscribe/unsub
- Subscribe `OnEnable`, unsub `OnDisable` — sans exception

#### Coroutine Safety

- **Tracker les coroutines** avec `Coroutine _activeCoroutine` — null-check avant relance
- Coroutine s'arrête silencieusement si l'objet est désactivé — ne reprend PAS au réactivation
- Pas de coroutines longue durée qui survivent au Hot Reload

#### Object Pool Safety

- **JAMAIS `Destroy()` sur un objet poolé** — toujours `_pool.Release()`
- **Flag `_isPooled`** sur chaque objet poolable — vérifier avant Release() pour éviter double release
- **`DOTween.Kill(gameObject)` dans `OnDisable()`** de tout objet poolé qui utilise DOTween

#### TimeScale Awareness

- **`Time.deltaTime`** = timers gameplay (ralentit avec hitstop)
- **`Time.unscaledDeltaTime`** = timers UI/réels (boss timer, combo timer)
- DOTween : `.SetUpdate(true)` pour animations hors-gameplay

#### Sorting Order (2D)

- Background : Order 0
- Enemies : Order 10
- Balls : Order 20
- VFX/Trails : Order 30
- Damage Numbers : Order 40
- UI : Canvas sort order 100+
- **Toujours setter Order in Layer** — sinon Z-fighting et flickering

#### Gotchas Unity 6

- `vSyncCount = 0` oublié → 30fps au lieu de 60
- Layer pas assigné sur prefab → collision matrix ignorée
- SO modifié runtime → changement persisté en éditeur (données corrompues)
- DOTween sans `.SetUpdate(true)` → figé pendant hitstop
- `ParticleSystem.Play()` sans `Clear()` sur pooled → vieilles particules

#### Gotchas Mobile

- Pas de `Screen.safeArea` → UI sous le notch
- Pas de back button handler → crash ou fermeture sans save
- `persistentDataPath` hardcodé → cassé après move to SD card
- `OnApplicationFocus` pas géré → save manqué sur certains Android

---

## Usage Guidelines

**Pour les AI Agents :**

- Lire ce fichier AVANT d'implémenter du code game
- Suivre TOUTES les règles exactement comme documentées
- En cas de doute, préférer l'option la plus restrictive
- Référencer `game-architecture.md` pour les patterns détaillés avec exemples de code

**Pour les humains :**

- Garder ce fichier lean et focalisé sur les besoins des agents
- Mettre à jour quand le stack technique change
- Supprimer les règles qui deviennent évidentes avec le temps

Dernière mise à jour : 2026-03-13
