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

        [MenuItem("BounceReaper/Setup/5 - Test: Spawn a Ball", priority = 10)]
        public static void TestSpawnBall()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Error", "Enter Play Mode first, then use this menu to spawn a ball.", "OK");
                return;
            }

            if (BallManager.IsAvailable)
            {
                BallManager.Instance.SpawnBall();
            }
            else
            {
                Debug.LogError("[Setup] BallManager not available. Is it in the scene?");
            }
        }

        // --- Helpers ---

        private static void CreateWall(string name, Transform parent, Vector3 position, Vector2 size)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(parent);
            wall.transform.position = position;

            var sr = wall.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            sr.color = new Color(0.1f, 0.12f, 0.3f, 1f); // visible dark blue wall
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
