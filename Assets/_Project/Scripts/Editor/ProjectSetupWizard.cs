#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace BounceReaper.Editor
{
    public static class ProjectSetupWizard
    {
        private const string SOPath = "Assets/_Project/ScriptableObjects";
        private const string PrefabPath = "Assets/_Project/Prefabs";
        private const string MaterialPath = "Assets/_Project/Art/Materials";
        private const string BrackeysPack = "Assets/Brackeys/2D Mega Pack";

        [MenuItem("BounceReaper/Setup/1 - Full Setup (Brick Breaker)", priority = 1)]
        public static void FullSetup()
        {
            SetupLayers();
            CreateAssets();
            SetupScene();
            Debug.Log("[Setup] Full brick breaker setup complete!");
        }

        [MenuItem("BounceReaper/Setup/2 - Reset Scene Only", priority = 2)]
        public static void ResetScene()
        {
            SetupScene();
        }

        // --- Internal setup methods ---

        private static void SetupLayers()
        {
            AddLayer("Ball", 6);
            AddLayer("Enemy", 7);
            AddLayer("Wall", 8);

            Physics2D.IgnoreLayerCollision(6, 6, true);   // Ball-Ball OFF
            Physics2D.IgnoreLayerCollision(7, 7, true);   // Enemy-Enemy OFF
            Physics2D.IgnoreLayerCollision(6, 7, false);  // Ball-Enemy ON
            Physics2D.IgnoreLayerCollision(6, 8, false);  // Ball-Wall ON
            Physics2D.IgnoreLayerCollision(7, 8, false);  // Enemy-Wall ON

            Debug.Log("[Setup] Layers and collision matrix configured");
        }

        private static void CreateAssets()
        {
            EnsureDirectory($"{SOPath}/Config");
            EnsureDirectory($"{SOPath}/Balls");
            EnsureDirectory($"{PrefabPath}/Ball");
            EnsureDirectory($"{PrefabPath}/Enemy");
            EnsureDirectory(MaterialPath);

            // GameConfig SO
            if (!AssetExists($"{SOPath}/Config/GameConfig.asset"))
            {
                var gc = ScriptableObject.CreateInstance<GameConfig>();
                AssetDatabase.CreateAsset(gc, $"{SOPath}/Config/GameConfig.asset");
            }

            // BallStats SO
            if (!AssetExists($"{SOPath}/Balls/Ball_Basic.asset"))
            {
                var bs = ScriptableObject.CreateInstance<BallStats>();
                AssetDatabase.CreateAsset(bs, $"{SOPath}/Balls/Ball_Basic.asset");
            }

            // PhysicsMaterial2D
            string matPath = $"{MaterialPath}/BallPhysics.physicsMaterial2D";
            PhysicsMaterial2D ballPhysMat;
            if (!AssetExists(matPath))
            {
                ballPhysMat = new PhysicsMaterial2D("BallPhysics") { bounciness = 1f, friction = 0f };
                AssetDatabase.CreateAsset(ballPhysMat, matPath);
            }
            else
            {
                ballPhysMat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(matPath);
            }

            // Ball prefab
            string ballPrefabPath = $"{PrefabPath}/Ball/Ball_Basic.prefab";
            if (!AssetExists(ballPrefabPath))
            {
                var ballGO = new GameObject("Ball_Basic");
                var sr = ballGO.AddComponent<SpriteRenderer>();
                sr.sprite = LoadBrackeysSprite("Shapes/Circle.png")
                    ?? AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                sr.color = new Color(0.3f, 0.8f, 1f);
                sr.sortingOrder = GameConstants.SortOrderBalls;
                ballGO.transform.localScale = Vector3.one * 0.25f;

                var rb = ballGO.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.linearDamping = 0f;
                rb.angularDamping = 0f;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;
                rb.freezeRotation = true;

                var col = ballGO.AddComponent<CircleCollider2D>();
                col.sharedMaterial = ballPhysMat;
                ballGO.AddComponent<BallController>();
                ballGO.layer = 6;

                PrefabUtility.SaveAsPrefabAsset(ballGO, ballPrefabPath);
                Object.DestroyImmediate(ballGO);
            }

            // Block prefab
            string blockPrefabPath = $"{PrefabPath}/Enemy/Block_Base.prefab";
            if (!AssetExists(blockPrefabPath))
            {
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
                blockGO.layer = 7;

                // HP Text
                var textGO = new GameObject("HPText");
                textGO.transform.SetParent(blockGO.transform);
                textGO.transform.localPosition = Vector3.zero;
                var tmp = textGO.AddComponent<TMPro.TextMeshPro>();
                tmp.alignment = TMPro.TextAlignmentOptions.Center;
                tmp.fontSize = 4;
                tmp.color = Color.white;
                tmp.sortingOrder = GameConstants.SortOrderDamageNumbers;
                textGO.GetComponent<RectTransform>().sizeDelta = new Vector2(1f, 1f);

                PrefabUtility.SaveAsPrefabAsset(blockGO, blockPrefabPath);
                Object.DestroyImmediate(blockGO);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Setup] Assets created");
        }

        private static void SetupScene()
        {
            // Clean old objects
            DestroyIfExists("Arena");
            DestroyIfExists("SpawnPoint");
            DestroyIfExists("WaveManager");
            DestroyByType<BallManager>();
            DestroyByType<GridManager>();
            DestroyByType<AimController>();
            DestroyByType<TurnManager>();

            // Arena (portrait, no bottom wall)
            var arena = new GameObject("Arena");
            float halfW = 2.75f, halfH = 4.75f, thick = 0.5f;
            CreateWall("Wall_Top", arena.transform, new Vector3(0, halfH + thick / 2, 0), new Vector2(halfW * 2 + thick * 2, thick));
            CreateWall("Wall_Left", arena.transform, new Vector3(-(halfW + thick / 2), 0, 0), new Vector2(thick, halfH * 2 + thick));
            CreateWall("Wall_Right", arena.transform, new Vector3(halfW + thick / 2, 0, 0), new Vector2(thick, halfH * 2 + thick));

            // BallManager
            var bmGO = new GameObject("BallManager");
            var bm = bmGO.AddComponent<BallManager>();
            var bmSo = new SerializedObject(bm);
            AssignAsset<GameConfig>(bmSo, "_gameConfig", $"{SOPath}/Config/GameConfig.asset");
            AssignAsset<BallStats>(bmSo, "_defaultStats", $"{SOPath}/Balls/Ball_Basic.asset");
            AssignAsset<BallController>(bmSo, "_ballPrefab", $"{PrefabPath}/Ball/Ball_Basic.prefab");
            bmSo.ApplyModifiedProperties();

            // GridManager
            var gmGO = new GameObject("GridManager");
            var gm = gmGO.AddComponent<GridManager>();
            var gmSo = new SerializedObject(gm);
            AssignAsset<EnemyController>(gmSo, "_blockPrefab", $"{PrefabPath}/Enemy/Block_Base.prefab");
            gmSo.ApplyModifiedProperties();

            // AimController
            var aimGO = new GameObject("AimController");
            var aimCtrl = aimGO.AddComponent<AimController>();
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

            // TurnManager
            var tmGO = new GameObject("TurnManager");
            var tm = tmGO.AddComponent<TurnManager>();
            var tmSo = new SerializedObject(tm);
            tmSo.FindProperty("_aimController").objectReferenceValue = aimCtrl;
            tmSo.ApplyModifiedProperties();

            // Camera
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0, 0, -10);
                cam.orthographicSize = 5.5f;
                cam.backgroundColor = new Color(0.04f, 0.04f, 0.1f);
            }

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("Setup Complete!",
                "Brick breaker scene ready.\n\n" +
                "Play Mode: Click/drag to aim, release to fire!\n" +
                "Blocks descend each turn. Game Over when they reach the bottom.",
                "OK");
        }

        // --- Helpers ---

        private static void CreateWall(string name, Transform parent, Vector3 pos, Vector2 size)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(parent);
            wall.transform.position = pos;
            var sr = wall.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            sr.color = new Color(0.15f, 0.15f, 0.25f);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            sr.sortingOrder = GameConstants.SortOrderBackground;
            var col = wall.AddComponent<BoxCollider2D>();
            col.size = size;
            wall.layer = 8;
        }

        private static void AddLayer(string name, int index)
        {
            var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var prop = tm.FindProperty("layers").GetArrayElementAtIndex(index);
            if (string.IsNullOrEmpty(prop.stringValue))
            {
                prop.stringValue = name;
                tm.ApplyModifiedProperties();
            }
        }

        private static Sprite LoadBrackeysSprite(string path)
        {
            string full = $"{BrackeysPack}/{path}";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(full);
            if (sprite != null) return sprite;
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(full))
                if (asset is Sprite s) return s;
            return null;
        }

        private static void DestroyIfExists(string name)
        {
            var go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
        }

        private static void DestroyByType<T>() where T : Object
        {
            var obj = Object.FindFirstObjectByType<T>();
            if (obj != null)
            {
                if (obj is Component c) Object.DestroyImmediate(c.gameObject);
                else Object.DestroyImmediate(obj);
            }
        }

        private static void AssignAsset<T>(SerializedObject so, string prop, string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) so.FindProperty(prop).objectReferenceValue = asset;
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
