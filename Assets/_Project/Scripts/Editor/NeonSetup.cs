#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;

namespace BounceReaper.Editor
{
    public static class NeonSetup
    {
        [MenuItem("BounceReaper/Visual/Apply Neon Look", priority = 20)]
        public static void ApplyNeonLook()
        {
            // 1. Find or create Global Volume for Bloom
            var volume = Object.FindFirstObjectByType<UnityEngine.Rendering.Volume>();
            if (volume == null)
            {
                var volGO = new GameObject("Global Volume");
                volume = volGO.AddComponent<UnityEngine.Rendering.Volume>();
                volume.isGlobal = true;
            }

            // Create or get volume profile
            if (volume.profile == null)
            {
                var profile = ScriptableObject.CreateInstance<UnityEngine.Rendering.VolumeProfile>();
                EnsureDir("Assets/_Project/Settings");
                string profilePath = "Assets/_Project/Settings/NeonVolumeProfile.asset";
                AssetDatabase.CreateAsset(profile, profilePath);
                volume.profile = profile;
            }

            // Add Bloom override
            if (!volume.profile.Has<Bloom>())
            {
                var bloom = volume.profile.Add<Bloom>();
                bloom.active = true;
                bloom.threshold.Override(0.8f);
                bloom.intensity.Override(2.5f);
                bloom.scatter.Override(0.7f);
            }
            else
            {
                var bloom = volume.profile.Get<Bloom>();
                bloom.threshold.Override(0.8f);
                bloom.intensity.Override(2.5f);
                bloom.scatter.Override(0.7f);
            }

            // 2. Enable post-processing on camera
            var cam = Camera.main;
            if (cam != null)
            {
                var camData = cam.GetComponent<UniversalAdditionalCameraData>();
                if (camData == null)
                    camData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
                camData.renderPostProcessing = true;
            }

            // 3. Add Light2D for ambient glow
            if (Object.FindFirstObjectByType<Light2D>() == null)
            {
                var lightGO = new GameObject("Global Light 2D");
                var light = lightGO.AddComponent<Light2D>();
                light.lightType = Light2D.LightType.Global;
                light.intensity = 0.3f;
                light.color = new Color(0.1f, 0.1f, 0.2f);
            }

            EditorUtility.SetDirty(volume.profile);
            AssetDatabase.SaveAssets();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("Neon Look Applied!",
                "Bloom post-processing enabled.\n" +
                "Camera post-processing ON.\n\n" +
                "Tip: Use 'Make Ball Glow Brighter' next for full neon effect.",
                "OK");
        }

        [MenuItem("BounceReaper/Visual/Make Ball Glow Brighter", priority = 21)]
        public static void MakeBallGlow()
        {
            string prefabPath = "Assets/_Project/Prefabs/Ball/Ball_Basic.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var sr = instance.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // HDR color (values > 1) triggers bloom
                sr.color = new Color(0.5f, 1.5f, 2f, 1f);
                var mat = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
                if (mat != null) sr.material = mat;
            }

            var trail = instance.GetComponent<TrailRenderer>();
            if (trail != null)
            {
                trail.startColor = new Color(0.5f, 1.5f, 2f, 0.9f);
                trail.endColor = new Color(0.3f, 1f, 1.5f, 0f);
                var mat = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
                if (mat != null) trail.material = mat;
            }

            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
            EditorUtility.DisplayDialog("Ball Glow", "Ball + trail now use HDR glow colors for bloom.", "OK");
        }

        private static void EnsureDir(string path)
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
}
#endif
