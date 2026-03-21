#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;

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

            // Upgrade SOs
            EnsureDirectory($"{SOPath}/Upgrades");
            CreateUpgradeSO("Upgrade_Damage", "Damage", 10, 1.5f, 20, 1f);
            CreateUpgradeSO("Upgrade_Speed", "Speed", 15, 1.5f, 15, 0.5f);
            CreateUpgradeSO("Upgrade_Balls", "Extra Balls", 25, 2f, 10, 1f);

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

                // Trail
                var trail = ballGO.AddComponent<TrailRenderer>();
                trail.time = 0.3f;
                trail.startWidth = 0.12f;
                trail.endWidth = 0f;
                trail.startColor = new Color(0.3f, 0.8f, 1f, 0.8f);
                trail.endColor = new Color(0.3f, 0.8f, 1f, 0f);
                trail.material = new Material(Shader.Find("Sprites/Default"));
                trail.sortingOrder = GameConstants.SortOrderVFX;
                trail.minVertexDistance = 0.05f;
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
            DestroyByType<CurrencyManager>();
            DestroyByType<UpgradeManager>();
            DestroyByType<VFXManager>();
            DestroyByType<AudioManager>();
            DestroyByType<SaveManager>();
            DestroyIfExists("UpgradePanel");
            DestroyByType<MainMenuController>();

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

            // AudioManager
            var audioGO = new GameObject("AudioManager");
            var audioMgr = audioGO.AddComponent<AudioManager>();
            string sfxPath = "Assets/Brackeys/2D Mega Pack/Sounds";
            var audioSo = new SerializedObject(audioMgr);
            AssignAsset<AudioClip>(audioSo, "_hitClip", $"{sfxPath}/Hit.wav");
            AssignAsset<AudioClip>(audioSo, "_destroyClip", $"{sfxPath}/Explosion.wav");
            AssignAsset<AudioClip>(audioSo, "_bonusClip", $"{sfxPath}/Bonus.wav");
            AssignAsset<AudioClip>(audioSo, "_shotClip", $"{sfxPath}/Shot.wav");
            AssignAsset<AudioClip>(audioSo, "_gameOverClip", $"{sfxPath}/GameOver.wav");
            AssignAsset<AudioClip>(audioSo, "_upgradeClip", $"{sfxPath}/Click.wav");
            audioSo.ApplyModifiedProperties();

            // SaveManager
            var saveGO = new GameObject("SaveManager");
            saveGO.AddComponent<SaveManager>();

            // CurrencyManager
            var currGO = new GameObject("CurrencyManager");
            currGO.AddComponent<CurrencyManager>();

            // UpgradeManager
            var upGO = new GameObject("UpgradeManager");
            var upMgr = upGO.AddComponent<UpgradeManager>();
            var upSo = new SerializedObject(upMgr);
            AssignAsset<UpgradeConfig>(upSo, "_damageUpgrade", $"{SOPath}/Upgrades/Upgrade_Damage.asset");
            AssignAsset<UpgradeConfig>(upSo, "_speedUpgrade", $"{SOPath}/Upgrades/Upgrade_Speed.asset");
            AssignAsset<UpgradeConfig>(upSo, "_extraBallsUpgrade", $"{SOPath}/Upgrades/Upgrade_Balls.asset");
            upSo.ApplyModifiedProperties();

            // VFXManager + damage number prefab
            EnsureDirectory($"{PrefabPath}/VFX");
            string dmgPrefabPath = $"{PrefabPath}/VFX/DamageNumber.prefab";
            if (!AssetExists(dmgPrefabPath))
            {
                var dmgGO = new GameObject("DamageNumber");
                var tmp = dmgGO.AddComponent<TextMeshPro>();
                tmp.fontSize = 5;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                tmp.sortingOrder = GameConstants.SortOrderDamageNumbers;
                dmgGO.GetComponent<RectTransform>().sizeDelta = new Vector2(2, 1);
                PrefabUtility.SaveAsPrefabAsset(dmgGO, dmgPrefabPath);
                Object.DestroyImmediate(dmgGO);
            }

            var vfxGO = new GameObject("VFXManager");
            var vfx = vfxGO.AddComponent<VFXManager>();
            var vfxSo = new SerializedObject(vfx);
            AssignAsset<TextMeshPro>(vfxSo, "_damageNumberPrefab", dmgPrefabPath);
            vfxSo.ApplyModifiedProperties();

            // TurnManager
            var tmGO = new GameObject("TurnManager");
            var tm = tmGO.AddComponent<TurnManager>();
            var tmSo = new SerializedObject(tm);
            tmSo.FindProperty("_aimController").objectReferenceValue = aimCtrl;
            // _upgradePanel and _mainMenu wired after HUD creation below
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

            // HUD Canvas
            DestroyIfExists("HUD_Canvas");
            var canvasGO = new GameObject("HUD_Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = GameConstants.SortOrderUI;
            var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var hud = canvasGO.AddComponent<HUDController>();

            // Top bar background
            var topBar = CreatePanel("TopBar", canvasGO.transform,
                new Vector2(0, 1), new Vector2(1, 1), // anchor top
                new Vector2(0, -80), new Vector2(0, 0), // offset
                new Color(0.05f, 0.05f, 0.15f, 0.8f));

            // Shards text (top left)
            var shardsGO = CreateAnchoredTMP("ShardsText", topBar.transform,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f), // anchor left-center
                new Vector2(120, 0), new Vector2(200, 50),
                "0", 32, TextAlignmentOptions.Left, new Color(1f, 0.85f, 0.2f));

            // Shards icon label
            CreateAnchoredTMP("ShardsIcon", topBar.transform,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(30, 0), new Vector2(50, 50),
                "\u25C6", 28, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f)); // diamond symbol

            // Wave text (top center)
            var waveGO = CreateAnchoredTMP("WaveText", topBar.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(200, 50),
                "Wave 0", 28, TextAlignmentOptions.Center, Color.white);

            // Ball count (bottom center, above launch area)
            var ballGO = CreateAnchoredTMP("BallCountText", canvasGO.transform,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), // anchor bottom-center
                new Vector2(40, 100), new Vector2(120, 50),
                "x1", 24, TextAlignmentOptions.Left, new Color(0.3f, 0.8f, 1f));

            // Game Over Panel
            var goPanelGO = new GameObject("GameOverPanel");
            goPanelGO.transform.SetParent(canvasGO.transform, false);
            var goRect = goPanelGO.AddComponent<RectTransform>();
            goRect.anchorMin = Vector2.zero;
            goRect.anchorMax = Vector2.one;
            goRect.sizeDelta = Vector2.zero;
            var goImg = goPanelGO.AddComponent<UnityEngine.UI.Image>();
            goImg.color = new Color(0.02f, 0.02f, 0.08f, 0.85f);

            CreateAnchoredTMP("Title", goPanelGO.transform,
                new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f),
                Vector2.zero, new Vector2(600, 80),
                "GAME OVER", 56, TextAlignmentOptions.Center, new Color(1f, 0.3f, 0.3f));

            var goScoreGO = CreateAnchoredTMP("Score", goPanelGO.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(500, 120),
                "Wave 0\n0 Shards", 32, TextAlignmentOptions.Center, Color.white);

            // Restart button
            var btnGO = new GameObject("RestartButton");
            btnGO.transform.SetParent(goPanelGO.transform, false);
            var btnRect = btnGO.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.3f);
            btnRect.anchorMax = new Vector2(0.5f, 0.3f);
            btnRect.anchoredPosition = Vector2.zero;
            btnRect.sizeDelta = new Vector2(350, 90);
            var btnImg = btnGO.AddComponent<UnityEngine.UI.Image>();
            btnImg.color = new Color(0.2f, 0.6f, 1f);
            var btn = btnGO.AddComponent<UnityEngine.UI.Button>();
            CreateAnchoredTMP("Text", btnGO.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(300, 80),
                "RESTART", 36, TextAlignmentOptions.Center, Color.white);

            goPanelGO.SetActive(false);

            // Upgrade Panel
            var upgPanelGO = new GameObject("UpgradePanel");
            upgPanelGO.transform.SetParent(canvasGO.transform, false);
            var upgRect = upgPanelGO.AddComponent<RectTransform>();
            upgRect.anchorMin = new Vector2(0, 0);
            upgRect.anchorMax = new Vector2(1, 0.25f);
            upgRect.offsetMin = Vector2.zero;
            upgRect.offsetMax = Vector2.zero;
            var upgImg = upgPanelGO.AddComponent<UnityEngine.UI.Image>();
            upgImg.color = new Color(0.05f, 0.05f, 0.15f, 0.9f);

            var upgPanel = upgPanelGO.AddComponent<UpgradePanel>();

            // Upgrade buttons
            var dmgBtn = CreateUpgradeButton("DamageBtn", upgPanelGO.transform, new Vector2(-200, 60), "Damage Lv.0\n10 shards", new Color(1f, 0.4f, 0.3f));
            var spdBtn = CreateUpgradeButton("SpeedBtn", upgPanelGO.transform, new Vector2(0, 60), "Speed Lv.0\n15 shards", new Color(0.3f, 0.7f, 1f));
            var ballBtn = CreateUpgradeButton("BallsBtn", upgPanelGO.transform, new Vector2(200, 60), "Balls Lv.0\n25 shards", new Color(0.3f, 1f, 0.5f));
            var skipBtn = CreateUpgradeButton("SkipBtn", upgPanelGO.transform, new Vector2(0, -30), "SKIP >>", new Color(0.3f, 0.3f, 0.4f));
            skipBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);

            var upgPanelSo = new SerializedObject(upgPanel);
            upgPanelSo.FindProperty("_panel").objectReferenceValue = upgPanelGO;
            upgPanelSo.FindProperty("_damageButton").objectReferenceValue = dmgBtn.GetComponent<UnityEngine.UI.Button>();
            upgPanelSo.FindProperty("_damageText").objectReferenceValue = dmgBtn.GetComponentInChildren<TextMeshProUGUI>();
            upgPanelSo.FindProperty("_speedButton").objectReferenceValue = spdBtn.GetComponent<UnityEngine.UI.Button>();
            upgPanelSo.FindProperty("_speedText").objectReferenceValue = spdBtn.GetComponentInChildren<TextMeshProUGUI>();
            upgPanelSo.FindProperty("_ballsButton").objectReferenceValue = ballBtn.GetComponent<UnityEngine.UI.Button>();
            upgPanelSo.FindProperty("_ballsText").objectReferenceValue = ballBtn.GetComponentInChildren<TextMeshProUGUI>();
            upgPanelSo.FindProperty("_skipButton").objectReferenceValue = skipBtn.GetComponent<UnityEngine.UI.Button>();
            upgPanelSo.ApplyModifiedProperties();

            upgPanelGO.SetActive(false);

            // Wire HUD references
            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("_shardsText").objectReferenceValue = shardsGO.GetComponent<TextMeshProUGUI>();
            hudSo.FindProperty("_waveText").objectReferenceValue = waveGO.GetComponent<TextMeshProUGUI>();
            hudSo.FindProperty("_ballCountText").objectReferenceValue = ballGO.GetComponent<TextMeshProUGUI>();
            hudSo.FindProperty("_gameOverPanel").objectReferenceValue = goPanelGO;
            hudSo.FindProperty("_gameOverScoreText").objectReferenceValue = goScoreGO.GetComponent<TextMeshProUGUI>();
            hudSo.ApplyModifiedProperties();

            // Wire restart button
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, hud.OnRestartButton);

            // Main Menu Panel
            var menuPanelGO = new GameObject("MainMenuPanel");
            menuPanelGO.transform.SetParent(canvasGO.transform, false);
            var menuRect = menuPanelGO.AddComponent<RectTransform>();
            menuRect.anchorMin = Vector2.zero;
            menuRect.anchorMax = Vector2.one;
            menuRect.sizeDelta = Vector2.zero;
            var menuImg = menuPanelGO.AddComponent<UnityEngine.UI.Image>();
            menuImg.color = new Color(0.03f, 0.03f, 0.08f, 0.95f);

            var menuCtrl = menuPanelGO.AddComponent<MainMenuController>();

            var titleGO = CreateAnchoredTMP("Title", menuPanelGO.transform,
                new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f),
                Vector2.zero, new Vector2(700, 120),
                "BOUNCE\nREAPER", 64, TextAlignmentOptions.Center, new Color(0.3f, 0.8f, 1f));

            var tapGO = CreateAnchoredTMP("TapText", menuPanelGO.transform,
                new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f),
                Vector2.zero, new Vector2(400, 60),
                "TAP TO PLAY", 32, TextAlignmentOptions.Center, Color.white);

            var bestGO = CreateAnchoredTMP("BestWave", menuPanelGO.transform,
                new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f),
                Vector2.zero, new Vector2(400, 50),
                "", 24, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f));

            var menuSo = new SerializedObject(menuCtrl);
            menuSo.FindProperty("_menuPanel").objectReferenceValue = menuPanelGO;
            menuSo.FindProperty("_titleText").objectReferenceValue = titleGO.GetComponent<TextMeshProUGUI>();
            menuSo.FindProperty("_tapText").objectReferenceValue = tapGO.GetComponent<TextMeshProUGUI>();
            menuSo.FindProperty("_bestWaveText").objectReferenceValue = bestGO.GetComponent<TextMeshProUGUI>();
            menuSo.ApplyModifiedProperties();

            // Wire TurnManager references (upgPanel + mainMenu now exist)
            var tmSo2 = new SerializedObject(tm);
            tmSo2.FindProperty("_upgradePanel").objectReferenceValue = upgPanel;
            tmSo2.FindProperty("_mainMenu").objectReferenceValue = menuCtrl;
            tmSo2.ApplyModifiedProperties();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("Setup Complete!",
                "Brick breaker scene ready.\n\n" +
                "Play Mode: Click/drag to aim, release to fire!\n" +
                "Blocks descend each turn. Game Over when they reach the bottom.",
                "OK");
        }

        // --- Helpers ---

        private static GameObject CreateAnchoredTMP(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta,
            string text, float fontSize, TextAlignmentOptions align, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = color;
            return go;
        }

        private static GameObject CreatePanel(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = color;
            return go;
        }

        private static void CreateUpgradeSO(string fileName, string displayName, int baseCost, float costScale, int maxLevel, float effectPerLevel)
        {
            string path = $"{SOPath}/Upgrades/{fileName}.asset";
            if (AssetExists(path)) return;
            var config = ScriptableObject.CreateInstance<UpgradeConfig>();
            var so = new SerializedObject(config);
            so.FindProperty("_upgradeId").stringValue = displayName.ToLower().Replace(" ", "_");
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_baseCost").intValue = baseCost;
            so.FindProperty("_costScale").floatValue = costScale;
            so.FindProperty("_maxLevel").intValue = maxLevel;
            so.FindProperty("_effectPerLevel").floatValue = effectPerLevel;
            so.ApplyModifiedProperties();
            AssetDatabase.CreateAsset(config, path);
        }

        private static GameObject CreateUpgradeButton(string name, Transform parent, Vector2 pos, string text, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(180, 70);
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = color;
            go.AddComponent<UnityEngine.UI.Button>();

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return go;
        }

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
