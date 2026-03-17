#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace BounceReaper.Editor
{
    public static class ProjectSetupWizard
    {
        private const string SOPath = "Assets/_Project/ScriptableObjects";
        private const string PrefabPath = "Assets/_Project/Prefabs";
        private const string MaterialPath = "Assets/_Project/Art/Materials";
        private const string BrackeysPack = "Assets/Brackeys/2D Mega Pack";

        [MenuItem("BounceReaper/Setup/1 - Configure Layers and Physics", priority = 1)]
        public static void SetupLayers()
        {
            // Add layers
            AddLayer("Ball", 6);
            AddLayer("Enemy", 7);
            AddLayer("Wall", 8);

            // Configure collision matrix: Ball-Ball OFF, Enemy-Enemy OFF
            // Ball(6) vs Enemy(7) ON, Ball(6) vs Wall(8) ON, Enemy(7) vs Wall(8) ON
            Physics2D.IgnoreLayerCollision(6, 6, true);   // Ball-Ball OFF
            Physics2D.IgnoreLayerCollision(7, 7, true);   // Enemy-Enemy OFF
            Physics2D.IgnoreLayerCollision(6, 7, false);  // Ball-Enemy ON
            Physics2D.IgnoreLayerCollision(6, 8, false);  // Ball-Wall ON
            Physics2D.IgnoreLayerCollision(7, 8, false);  // Enemy-Wall ON

            Debug.Log("[Setup] Layers configured: Ball(6), Enemy(7), Wall(8). Collision matrix set.");
            EditorUtility.DisplayDialog("Setup Complete", "Layers and Physics2D collision matrix configured!", "OK");
        }

        [MenuItem("BounceReaper/Setup/2 - Create ScriptableObjects", priority = 2)]
        public static void CreateScriptableObjects()
        {
            EnsureDirectory($"{SOPath}/Config");
            EnsureDirectory($"{SOPath}/Balls");

            // GameConfig
            if (!AssetExists($"{SOPath}/Config/GameConfig.asset"))
            {
                var gameConfig = ScriptableObject.CreateInstance<GameConfig>();
                AssetDatabase.CreateAsset(gameConfig, $"{SOPath}/Config/GameConfig.asset");
                Debug.Log("[Setup] Created GameConfig.asset");
            }

            // BallStats
            if (!AssetExists($"{SOPath}/Balls/Ball_Basic.asset"))
            {
                var ballStats = ScriptableObject.CreateInstance<BallStats>();
                AssetDatabase.CreateAsset(ballStats, $"{SOPath}/Balls/Ball_Basic.asset");
                Debug.Log("[Setup] Created Ball_Basic.asset");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Setup] ScriptableObjects created.");
            EditorUtility.DisplayDialog("Setup Complete", "GameConfig and BallStats assets created in ScriptableObjects/", "OK");
        }

        [MenuItem("BounceReaper/Setup/3 - Create Ball Prefab", priority = 3)]
        public static void CreateBallPrefab()
        {
            EnsureDirectory($"{PrefabPath}/Ball");
            EnsureDirectory(MaterialPath);

            // PhysicsMaterial2D
            string matPath = $"{MaterialPath}/BallPhysics.physicsMaterial2D";
            PhysicsMaterial2D ballPhysMat;
            if (!AssetExists(matPath))
            {
                ballPhysMat = new PhysicsMaterial2D("BallPhysics")
                {
                    bounciness = 1f,
                    friction = 0f
                };
                AssetDatabase.CreateAsset(ballPhysMat, matPath);
                Debug.Log("[Setup] Created BallPhysics material (bounciness=1, friction=0)");
            }
            else
            {
                ballPhysMat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(matPath);
            }

            // Ball prefab
            string prefabPath = $"{PrefabPath}/Ball/Ball_Basic.prefab";
            if (AssetExists(prefabPath))
            {
                // Update existing prefab scale
                var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (existingPrefab != null && existingPrefab.transform.localScale.x < 0.6f)
                {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(existingPrefab);
                    instance.transform.localScale = Vector3.one * 0.7f;
                    PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                    Object.DestroyImmediate(instance);
                    Debug.Log("[Setup] Updated Ball_Basic.prefab scale to 0.7");
                }
                else
                {
                    Debug.Log("[Setup] Ball_Basic.prefab already up to date.");
                }
                AssetDatabase.SaveAssets();
                return;
            }

            var ballGO = new GameObject("Ball_Basic");

            // SpriteRenderer
            var sr = ballGO.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            sr.color = new Color(0.3f, 0.8f, 1f, 1f); // neon blue
            sr.sortingOrder = GameConstants.SortOrderBalls;
            ballGO.transform.localScale = Vector3.one * 0.7f;

            // Rigidbody2D
            var rb = ballGO.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.freezeRotation = true;

            // CircleCollider2D
            var col = ballGO.AddComponent<CircleCollider2D>();
            col.sharedMaterial = ballPhysMat;

            // BallController
            ballGO.AddComponent<BallController>();

            // Layer
            ballGO.layer = LayerMask.NameToLayer("Ball");
            if (ballGO.layer == -1) ballGO.layer = 6;

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(ballGO, prefabPath);
            Object.DestroyImmediate(ballGO);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Setup] Created Ball_Basic.prefab with physics setup.");
            EditorUtility.DisplayDialog("Setup Complete", "Ball_Basic prefab created with Rigidbody2D, CircleCollider2D, PhysicsMaterial2D, and BallController.", "OK");
        }

        [MenuItem("BounceReaper/Setup/4 - Create Arena and Scene Objects", priority = 4)]
        public static void CreateArenaAndManagers()
        {
            // Arena
            var existingArena = GameObject.Find("Arena");
            if (existingArena != null)
                Object.DestroyImmediate(existingArena);

            {
                var arena = new GameObject("Arena");

                float halfWidth = 2.75f;   // 9:16 portrait (~5.5 wide)
                float halfHeight = 4.75f;  // ~9.5 tall
                float wallThickness = 0.5f;

                CreateWall("Wall_Top", arena.transform, new Vector3(0, halfHeight + wallThickness / 2, 0), new Vector2(halfWidth * 2 + wallThickness * 2, wallThickness));
                CreateWall("Wall_Bottom", arena.transform, new Vector3(0, -(halfHeight + wallThickness / 2), 0), new Vector2(halfWidth * 2 + wallThickness * 2, wallThickness));
                CreateWall("Wall_Left", arena.transform, new Vector3(-(halfWidth + wallThickness / 2), 0, 0), new Vector2(wallThickness, halfHeight * 2));
                CreateWall("Wall_Right", arena.transform, new Vector3(halfWidth + wallThickness / 2, 0, 0), new Vector2(wallThickness, halfHeight * 2));

                Debug.Log("[Setup] Arena created (16:9, walls on layer Wall)");
            }

            // Spawn Point
            if (GameObject.Find("SpawnPoint") == null)
            {
                var spawnPoint = new GameObject("SpawnPoint");
                spawnPoint.transform.position = Vector3.zero;
                Debug.Log("[Setup] SpawnPoint created at origin");
            }

            // BallManager
            if (Object.FindFirstObjectByType<BallManager>() == null)
            {
                var ballManagerGO = new GameObject("BallManager");
                var bm = ballManagerGO.AddComponent<BallManager>();

                // Try to auto-assign references
                var gameConfig = AssetDatabase.LoadAssetAtPath<GameConfig>($"{SOPath}/Config/GameConfig.asset");
                var ballStats = AssetDatabase.LoadAssetAtPath<BallStats>($"{SOPath}/Balls/Ball_Basic.asset");
                var ballPrefab = AssetDatabase.LoadAssetAtPath<BallController>($"{PrefabPath}/Ball/Ball_Basic.prefab");
                var spawnPoint = GameObject.Find("SpawnPoint");

                var so = new SerializedObject(bm);
                if (gameConfig != null) so.FindProperty("_gameConfig").objectReferenceValue = gameConfig;
                if (ballStats != null) so.FindProperty("_defaultStats").objectReferenceValue = ballStats;
                if (ballPrefab != null) so.FindProperty("_ballPrefab").objectReferenceValue = ballPrefab;
                if (spawnPoint != null) so.FindProperty("_spawnPoint").objectReferenceValue = spawnPoint.transform;
                so.ApplyModifiedProperties();

                Debug.Log("[Setup] BallManager created with references assigned");
            }

            // Camera setup — portrait 9:16
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0, 0, -10);
                cam.orthographicSize = 5.5f;
                cam.backgroundColor = new Color(0.04f, 0.04f, 0.1f, 1f); // dark neon background #0A0A1A
            }

            // Player Settings — Portrait orientation
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[Setup] Scene setup complete!");
            EditorUtility.DisplayDialog("Setup Complete",
                "Arena (4 walls), SpawnPoint, BallManager, and Camera configured.\n\n" +
                "IMPORTANT: Assign the BallStats SO to the Ball_Basic prefab's BallController component in the Inspector!",
                "OK");
        }

        [MenuItem("BounceReaper/Setup/5 - Create Enemy Prefab and Wave System", priority = 5)]
        public static void CreateEnemyAndWaveSystem()
        {
            EnsureDirectory($"{PrefabPath}/Enemy");
            EnsureDirectory($"{SOPath}/Enemies");
            EnsureDirectory($"{SOPath}/Waves");

            // PhysicsMaterial2D for bouncing enemies (Diamond)
            string matPath = $"{MaterialPath}/BallPhysics.physicsMaterial2D";
            PhysicsMaterial2D bounceMat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(matPath);

            // Enemy prefab
            string enemyPrefabPath = $"{PrefabPath}/Enemy/Enemy_Base.prefab";
            if (!AssetExists(enemyPrefabPath))
            {
                var enemyGO = new GameObject("Enemy_Base");

                var sr = enemyGO.AddComponent<SpriteRenderer>();
                sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                sr.color = Color.red;
                sr.sortingOrder = GameConstants.SortOrderEnemies;

                var rb = enemyGO.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                rb.freezeRotation = true;

                var col = enemyGO.AddComponent<CircleCollider2D>();
                if (bounceMat != null) col.sharedMaterial = bounceMat;

                enemyGO.AddComponent<EnemyHealth>();
                enemyGO.AddComponent<EnemyController>();

                int enemyLayer = LayerMask.NameToLayer("Enemy");
                enemyGO.layer = enemyLayer >= 0 ? enemyLayer : 7;

                PrefabUtility.SaveAsPrefabAsset(enemyGO, enemyPrefabPath);
                Object.DestroyImmediate(enemyGO);
                Debug.Log("[Setup] Created Enemy_Base.prefab");
            }

            // Enemy Stats SOs
            CreateEnemyStatsSO("Enemy_Triangle", 1, 0.5f, 5f, false, 0.5f, new Color(1f, 0.3f, 0.3f), 1);
            CreateEnemyStatsSO("Enemy_Square", 3, 0.8f, 2.5f, false, 0.7f, new Color(1f, 0.6f, 0.2f), 3);
            CreateEnemyStatsSO("Enemy_Hexagon", 5, 1.2f, 1.5f, false, 0.8f, new Color(0.8f, 0.2f, 1f), 5);
            CreateEnemyStatsSO("Enemy_Diamond", 10, 3f, 0.5f, true, 0.6f, new Color(1f, 1f, 0.2f), 10);

            // WaveConfig
            if (!AssetExists($"{SOPath}/Waves/WaveConfig_Default.asset"))
            {
                var waveConfig = ScriptableObject.CreateInstance<WaveConfig>();
                AssetDatabase.CreateAsset(waveConfig, $"{SOPath}/Waves/WaveConfig_Default.asset");
                Debug.Log("[Setup] Created WaveConfig_Default.asset");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // WaveManager in scene
            if (Object.FindFirstObjectByType<WaveManager>() == null)
            {
                var waveManagerGO = new GameObject("WaveManager");
                var wm = waveManagerGO.AddComponent<WaveManager>();

                var so = new SerializedObject(wm);

                var waveConfig = AssetDatabase.LoadAssetAtPath<WaveConfig>($"{SOPath}/Waves/WaveConfig_Default.asset");
                var enemyPrefab = AssetDatabase.LoadAssetAtPath<EnemyController>(enemyPrefabPath);
                var triangle = AssetDatabase.LoadAssetAtPath<EnemyStats>($"{SOPath}/Enemies/Enemy_Triangle.asset");
                var square = AssetDatabase.LoadAssetAtPath<EnemyStats>($"{SOPath}/Enemies/Enemy_Square.asset");
                var hexagon = AssetDatabase.LoadAssetAtPath<EnemyStats>($"{SOPath}/Enemies/Enemy_Hexagon.asset");
                var diamond = AssetDatabase.LoadAssetAtPath<EnemyStats>($"{SOPath}/Enemies/Enemy_Diamond.asset");

                if (waveConfig != null) so.FindProperty("_config").objectReferenceValue = waveConfig;
                if (enemyPrefab != null) so.FindProperty("_enemyPrefab").objectReferenceValue = enemyPrefab;
                if (triangle != null) so.FindProperty("_triangleStats").objectReferenceValue = triangle;
                if (square != null) so.FindProperty("_squareStats").objectReferenceValue = square;
                if (hexagon != null) so.FindProperty("_hexagonStats").objectReferenceValue = hexagon;
                if (diamond != null) so.FindProperty("_diamondStats").objectReferenceValue = diamond;
                so.ApplyModifiedProperties();

                Debug.Log("[Setup] WaveManager created with references assigned");
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("Setup Complete",
                "Enemy_Base prefab, 4 EnemyStats SOs, WaveConfig, and WaveManager created.\n\n" +
                "Run Play Mode to see waves spawn!",
                "OK");
        }

        private static void CreateEnemyStatsSO(string name, int hp, float speed, float dirInterval, bool usePhysics, float size, Color color, int reward)
        {
            string path = $"{SOPath}/Enemies/{name}.asset";
            if (AssetExists(path)) return;

            var stats = ScriptableObject.CreateInstance<EnemyStats>();
            var so = new SerializedObject(stats);
            so.FindProperty("_maxHP").intValue = hp;
            so.FindProperty("_moveSpeed").floatValue = speed;
            so.FindProperty("_directionInterval").floatValue = dirInterval;
            so.FindProperty("_usePhysicsMovement").boolValue = usePhysics;
            so.FindProperty("_size").floatValue = size;
            so.FindProperty("_color").colorValue = color;
            so.FindProperty("_shardReward").intValue = reward;
            so.ApplyModifiedProperties();

            AssetDatabase.CreateAsset(stats, path);
            Debug.Log($"[Setup] Created {name}.asset");
        }

        [MenuItem("BounceReaper/Setup/6 - Upgrade Sprites (Brackeys Pack)", priority = 6)]
        public static void UpgradeSprites()
        {
            // --- Ball sprite ---
            string ballPrefabPath = $"{PrefabPath}/Ball/Ball_Basic.prefab";
            var circleSprite = LoadBrackeysSprite("Shapes/Circle.png");
            if (circleSprite != null && AssetExists(ballPrefabPath))
            {
                var ballPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ballPrefabPath);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(ballPrefab);
                var sr = instance.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = circleSprite;
                    sr.color = new Color(0.3f, 0.8f, 1f, 1f); // neon blue
                }
                instance.transform.localScale = Vector3.one * 0.15f;
                PrefabUtility.SaveAsPrefabAsset(instance, ballPrefabPath);
                Object.DestroyImmediate(instance);
                Debug.Log("[Setup] Ball sprite updated to Circle");
            }

            // --- Enemy sprites in SO ---
            SetEnemySpriteInSO("Enemy_Triangle", "Enemies/Insects/Beetle.png", new Color(0.6f, 1f, 0.4f));
            SetEnemySpriteInSO("Enemy_Square", "Enemies/Gothic/GothicEnemy01.png", new Color(1f, 0.6f, 0.2f));
            SetEnemySpriteInSO("Enemy_Hexagon", "Enemies/Gothic/GothicEnemy02.png", new Color(0.8f, 0.2f, 1f));
            SetEnemySpriteInSO("Enemy_Diamond", "Enemies/Gothic/FireheadEnemy.png", new Color(1f, 0.2f, 0.2f));

            // --- Enemy prefab sprite (default) ---
            string enemyPrefabPath = $"{PrefabPath}/Enemy/Enemy_Base.prefab";
            var beetleSprite = LoadBrackeysSprite("Enemies/Insects/Beetle.png");
            if (beetleSprite != null && AssetExists(enemyPrefabPath))
            {
                var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(enemyPrefabPath);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);
                var sr = instance.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = beetleSprite;
                    sr.color = Color.white; // use original sprite colors
                }
                PrefabUtility.SaveAsPrefabAsset(instance, enemyPrefabPath);
                Object.DestroyImmediate(instance);
                Debug.Log("[Setup] Enemy prefab sprite updated to Beetle");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Sprites Upgraded!",
                "Ball → Circle sprite\n" +
                "Triangle → Beetle\n" +
                "Square → Gothic Enemy 01\n" +
                "Hexagon → Gothic Enemy 02\n" +
                "Diamond → Firehead Enemy\n\n" +
                "Enemy sprites are stored in SO and applied at spawn via EnemyController.",
                "OK");
        }

        private static Sprite LoadBrackeysSprite(string relativePath)
        {
            string fullPath = $"{BrackeysPack}/{relativePath}";
            // Try loading as single sprite
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
            if (sprite != null) return sprite;

            // Try loading from sprite sheet (multiple mode) — get first sub-sprite
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(fullPath);
            foreach (var asset in allAssets)
            {
                if (asset is Sprite s) return s;
            }

            Debug.LogWarning($"[Setup] Sprite not found at {fullPath}");
            return null;
        }

        private static void SetEnemySpriteInSO(string soName, string spritePath, Color tintColor)
        {
            string soFullPath = $"{SOPath}/Enemies/{soName}.asset";
            if (!AssetExists(soFullPath)) return;

            var stats = AssetDatabase.LoadAssetAtPath<EnemyStats>(soFullPath);
            if (stats == null) return;

            var sprite = LoadBrackeysSprite(spritePath);

            var so = new SerializedObject(stats);
            so.FindProperty("_color").colorValue = tintColor;
            if (sprite != null)
                so.FindProperty("_sprite").objectReferenceValue = sprite;
            so.ApplyModifiedProperties();

            Debug.Log($"[Setup] {soName} sprite + tint updated");
        }

        [MenuItem("BounceReaper/Setup/7 - Fix Enemy Sizes and Movement", priority = 7)]
        public static void FixEnemySizes()
        {
            // Smaller sizes, no movement (static enemies), bigger collider via size
            FixEnemySO("Enemy_Triangle", hp: 1, speed: 0f, size: 0.3f, reward: 1);
            FixEnemySO("Enemy_Square", hp: 3, speed: 0f, size: 0.4f, reward: 3);
            FixEnemySO("Enemy_Hexagon", hp: 5, speed: 0f, size: 0.45f, reward: 5);
            FixEnemySO("Enemy_Diamond", hp: 10, speed: 0f, size: 0.35f, reward: 10);

            // Also update enemy prefab collider radius
            string enemyPrefabPath = $"{PrefabPath}/Enemy/Enemy_Base.prefab";
            if (AssetExists(enemyPrefabPath))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(enemyPrefabPath);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                var col = instance.GetComponent<CircleCollider2D>();
                if (col != null)
                    col.radius = 1.5f; // bigger hitbox relative to sprite
                PrefabUtility.SaveAsPrefabAsset(instance, enemyPrefabPath);
                Object.DestroyImmediate(instance);
                Debug.Log("[Setup] Enemy prefab collider radius updated");
            }

            // Also fix ball prefab size
            string ballPrefabPath = $"{PrefabPath}/Ball/Ball_Basic.prefab";
            if (AssetExists(ballPrefabPath))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ballPrefabPath);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.localScale = Vector3.one * 0.25f;
                PrefabUtility.SaveAsPrefabAsset(instance, ballPrefabPath);
                Object.DestroyImmediate(instance);
                Debug.Log("[Setup] Ball prefab scale updated to 0.25");
            }

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Fixed!",
                "Enemies: smaller, static (no movement), bigger hitbox.\nBall: bigger.\n\nPlay to test!",
                "OK");
        }

        private static void FixEnemySO(string soName, int hp, float speed, float size, int reward)
        {
            string path = $"{SOPath}/Enemies/{soName}.asset";
            if (!AssetExists(path)) return;

            var stats = AssetDatabase.LoadAssetAtPath<EnemyStats>(path);
            var so = new SerializedObject(stats);
            so.FindProperty("_maxHP").intValue = hp;
            so.FindProperty("_moveSpeed").floatValue = speed;
            so.FindProperty("_size").floatValue = size;
            so.FindProperty("_shardReward").intValue = reward;
            so.FindProperty("_usePhysicsMovement").boolValue = false;
            so.ApplyModifiedProperties();
            Debug.Log($"[Setup] Fixed {soName}: size={size}, speed={speed}");
        }

        [MenuItem("BounceReaper/Setup/8 - Setup Brick Breaker Scene", priority = 8)]
        public static void SetupBrickBreakerScene()
        {
            // Remove old WaveManager if exists
            var oldWave = Object.FindFirstObjectByType<WaveManager>();
            if (oldWave != null) Object.DestroyImmediate(oldWave.gameObject);

            // Remove old BallManager if exists
            var oldBall = Object.FindFirstObjectByType<BallManager>();
            if (oldBall != null) Object.DestroyImmediate(oldBall.gameObject);

            // Create block prefab with TMP
            string blockPrefabPath = $"{PrefabPath}/Enemy/Block_Base.prefab";
            if (!AssetExists(blockPrefabPath))
            {
                EnsureDirectory($"{PrefabPath}/Enemy");
                var blockGO = new GameObject("Block_Base");

                var sr = blockGO.AddComponent<SpriteRenderer>();
                sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
                sr.color = Color.green;
                sr.sortingOrder = GameConstants.SortOrderEnemies;
                sr.drawMode = SpriteDrawMode.Sliced;
                sr.size = new Vector2(0.9f, 0.9f);

                var rb = blockGO.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                rb.freezeRotation = true;

                var col = blockGO.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.9f, 0.9f);

                blockGO.AddComponent<EnemyHealth>();
                blockGO.AddComponent<EnemyController>();

                int enemyLayer = LayerMask.NameToLayer("Enemy");
                blockGO.layer = enemyLayer >= 0 ? enemyLayer : 7;

                // HP Text
                var textGO = new GameObject("HPText");
                textGO.transform.SetParent(blockGO.transform);
                textGO.transform.localPosition = Vector3.zero;
                var tmp = textGO.AddComponent<TMPro.TextMeshPro>();
                tmp.alignment = TMPro.TextAlignmentOptions.Center;
                tmp.fontSize = 4;
                tmp.color = Color.white;
                tmp.sortingOrder = GameConstants.SortOrderDamageNumbers;
                var tmpRect = textGO.GetComponent<RectTransform>();
                tmpRect.sizeDelta = new Vector2(1f, 1f);

                PrefabUtility.SaveAsPrefabAsset(blockGO, blockPrefabPath);
                Object.DestroyImmediate(blockGO);
                Debug.Log("[Setup] Created Block_Base.prefab with HP text");
            }

            // BallManager
            if (Object.FindFirstObjectByType<BallManager>() == null)
            {
                var bmGO = new GameObject("BallManager");
                var bm = bmGO.AddComponent<BallManager>();

                var so = new SerializedObject(bm);
                var gameConfig = AssetDatabase.LoadAssetAtPath<GameConfig>($"{SOPath}/Config/GameConfig.asset");
                var ballStats = AssetDatabase.LoadAssetAtPath<BallStats>($"{SOPath}/Balls/Ball_Basic.asset");
                var ballPrefab = AssetDatabase.LoadAssetAtPath<BallController>($"{PrefabPath}/Ball/Ball_Basic.prefab");

                if (gameConfig != null) so.FindProperty("_gameConfig").objectReferenceValue = gameConfig;
                if (ballStats != null) so.FindProperty("_defaultStats").objectReferenceValue = ballStats;
                if (ballPrefab != null) so.FindProperty("_ballPrefab").objectReferenceValue = ballPrefab;
                so.ApplyModifiedProperties();
                Debug.Log("[Setup] BallManager created");
            }

            // GridManager
            if (Object.FindFirstObjectByType<GridManager>() == null)
            {
                var gmGO = new GameObject("GridManager");
                var gm = gmGO.AddComponent<GridManager>();

                var so = new SerializedObject(gm);
                var blockPrefab = AssetDatabase.LoadAssetAtPath<EnemyController>(blockPrefabPath);
                if (blockPrefab != null) so.FindProperty("_blockPrefab").objectReferenceValue = blockPrefab;

                var blockSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
                if (blockSprite != null) so.FindProperty("_blockSprite").objectReferenceValue = blockSprite;
                so.ApplyModifiedProperties();
                Debug.Log("[Setup] GridManager created");
            }

            // AimController
            AimController aimCtrl = null;
            if (Object.FindFirstObjectByType<AimController>() == null)
            {
                var aimGO = new GameObject("AimController");
                aimCtrl = aimGO.AddComponent<AimController>();

                // Add LineRenderer for aim line
                var lr = aimGO.AddComponent<LineRenderer>();
                lr.startWidth = 0.05f;
                lr.endWidth = 0.02f;
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = new Color(1f, 1f, 1f, 0.5f);
                lr.endColor = new Color(1f, 1f, 1f, 0.1f);
                lr.sortingOrder = GameConstants.SortOrderUI;
                lr.positionCount = 2;
                lr.enabled = false;

                var aimSo = new SerializedObject(aimCtrl);
                aimSo.FindProperty("_aimLine").objectReferenceValue = lr;
                aimSo.ApplyModifiedProperties();

                Debug.Log("[Setup] AimController created with LineRenderer");
            }
            else
            {
                aimCtrl = Object.FindFirstObjectByType<AimController>();
            }

            // TurnManager
            if (Object.FindFirstObjectByType<TurnManager>() == null)
            {
                var tmGO = new GameObject("TurnManager");
                var tm = tmGO.AddComponent<TurnManager>();

                var so = new SerializedObject(tm);
                if (aimCtrl != null) so.FindProperty("_aimController").objectReferenceValue = aimCtrl;
                so.ApplyModifiedProperties();
                Debug.Log("[Setup] TurnManager created");
            }

            // Remove bottom wall (balls fall through)
            var arena = GameObject.Find("Arena");
            if (arena != null)
            {
                var bottomWall = arena.transform.Find("Wall_Bottom");
                if (bottomWall != null)
                    Object.DestroyImmediate(bottomWall.gameObject);
                Debug.Log("[Setup] Removed bottom wall (balls fall through)");
            }

            // Remove SpawnPoint (no longer needed)
            var spawnPoint = GameObject.Find("SpawnPoint");
            if (spawnPoint != null) Object.DestroyImmediate(spawnPoint);

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("Brick Breaker Setup Complete!",
                "Scene configured for PunBall-style gameplay:\n\n" +
                "- Block prefab with HP display\n" +
                "- BallManager (volley mode)\n" +
                "- GridManager (block grid)\n" +
                "- AimController (touch/mouse aiming)\n" +
                "- TurnManager (turn-based phases)\n" +
                "- Bottom wall removed\n\n" +
                "Play Mode: Click and drag to aim, release to fire!",
                "OK");
        }

        // --- Helpers ---

        private static void CreateWall(string name, Transform parent, Vector3 position, Vector2 size)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(parent);
            wall.transform.position = position;

            var sr = wall.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            sr.color = new Color(0.15f, 0.15f, 0.25f, 1f); // dark wall
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            sr.sortingOrder = GameConstants.SortOrderBackground;

            var col = wall.AddComponent<BoxCollider2D>();
            col.size = size;

            int wallLayer = LayerMask.NameToLayer("Wall");
            wall.layer = wallLayer >= 0 ? wallLayer : 8;
        }

        private static void AddLayer(string layerName, int layerIndex)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");

            var layerProp = layers.GetArrayElementAtIndex(layerIndex);
            if (string.IsNullOrEmpty(layerProp.stringValue))
            {
                layerProp.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                Debug.Log($"[Setup] Added layer '{layerName}' at index {layerIndex}");
            }
            else if (layerProp.stringValue != layerName)
            {
                Debug.LogWarning($"[Setup] Layer {layerIndex} already has '{layerProp.stringValue}', expected '{layerName}'");
            }
        }

        private static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parts = path.Split('/');
                var current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    var next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }
        }

        private static bool AssetExists(string path)
        {
            return !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path, AssetPathToGUIDOptions.OnlyExistingAssets));
        }
    }
}
#endif
