using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace MissingScript
{
    public class MissingScriptWindow : EditorWindow
    {
        #region Variables

        private SceneAsset selectedScene;
        private List<GameObject> objectsWithMissingScripts = new List<GameObject>();
        private bool showWarnings = true;
        private string actionLog = "";
        private Vector2 scrollPosition = Vector2.zero;

        #endregion

        #region Editor Window Setup

        [MenuItem("Tools/Missing Script Tool %#u")]
        public static void ShowWindow()
        {
            var window = GetWindow<MissingScriptWindow>("Missing Script Tool");
            window.maxSize = new Vector2(720, 480);
            window.minSize = new Vector2(720, 480);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(15);

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.7f, 0.85f, 1.0f) }
            };
            GUILayout.Label("Missing Script Tool", labelStyle);

            GUILayout.Space(15);

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(15, 15, 10, 10)
            };

            buttonStyle.normal.textColor = Color.white;
            buttonStyle.hover.textColor = new Color(0.4f, 0.7f, 1.0f);
            buttonStyle.active.textColor = new Color(0.2f, 0.6f, 1.0f);
            buttonStyle.normal.background = MakeTexture(2, 2, new Color(0.15f, 0.2f, 0.3f));

            GUIStyle textAreaStyle = new GUIStyle(GUI.skin.textArea)
            {
                fontSize = 13,
                wordWrap = true,
                stretchHeight = true
            };

            textAreaStyle.normal.textColor = Color.white;
            textAreaStyle.focused.textColor = new Color(0.5f, 0.8f, 1.0f);
            textAreaStyle.active.textColor = new Color(0.3f, 0.7f, 1.0f);
            textAreaStyle.padding = new RectOffset(8, 8, 8, 8);
            textAreaStyle.margin = new RectOffset(5, 5, 5, 5);

            EditorGUILayout.HelpBox("Project Operations", MessageType.Info);
            EditorGUILayout.BeginVertical("box");
            if (GUILayout.Button("Find Missing Scripts in Entire Project", buttonStyle))
            {
                objectsWithMissingScripts.Clear();
                FindMissingScriptsInProject();
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            EditorGUILayout.HelpBox("Scene Operations", MessageType.Info);
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            selectedScene = (SceneAsset)EditorGUILayout.ObjectField("Scene", selectedScene, typeof(SceneAsset), false, GUILayout.Height(30));
            if (EditorGUI.EndChangeCheck() && selectedScene != null)
            {
                UnityEngine.Debug.Log("Scene selected, but not opened yet.");
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Find Missing Scripts in Scene", buttonStyle))
            {
                if (selectedScene != null)
                {
                    string scenePath = AssetDatabase.GetAssetPath(selectedScene);
                    Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    LogAction($"Found {objectsWithMissingScripts.Count} objects with missing scripts.");
                    OnSceneOpen(scene);
                }
                else
                {
                    LogAction("Please select a scene.");
                }
            }

            if (GUILayout.Button("Save Scene", buttonStyle))
            {
                SaveScene();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            if (objectsWithMissingScripts.Count > 0)
            {
                EditorGUILayout.HelpBox("Cleanup Operations", MessageType.Warning);
                EditorGUILayout.BeginVertical("box");
                if (GUILayout.Button("Remove Missing Components", buttonStyle) && objectsWithMissingScripts.Count > 0)
                {
                    ShowConfirmationDialog("Are you sure you want to remove missing components?", RemoveMissingComponents);
                }
                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(15);

            GUILayout.Label("Action Log", labelStyle);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
            actionLog = EditorGUILayout.TextArea(actionLog, textAreaStyle, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }

        #endregion

        #region Operations

        private void OnSceneOpen(Scene scene)
        {
            FindMissingScriptsInScene(scene);
        }

        private void FindMissingScriptsInScene(Scene scene)
        {
            foreach (GameObject obj in scene.GetRootGameObjects())
            {
                FindMissingScripts(obj);
            }
        }

        private void FindMissingScripts(GameObject obj)
        {
            Component[] components = obj.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    if (!objectsWithMissingScripts.Contains(obj))
                    {
                        objectsWithMissingScripts.Add(obj);
                        if (showWarnings)
                        {
                            UnityEngine.Debug.LogWarning("Missing script on object: " + obj.name);
                        }
                        LogAction("Missing script on object: " + obj.name);
                    }
                    break;
                }
            }

            foreach (Transform child in obj.transform)
            {
                FindMissingScripts(child.gameObject);
            }
        }

        private void FindMissingScriptsInProject()
        {
            float cpuUsageThreshold = 80f;
            string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");

            foreach (string prefabGUID in allPrefabs)
            {
                if (GetCpuUsage() > cpuUsageThreshold)
                {
                    UnityEngine.Debug.LogWarning("CPU usage is too high! Consider stopping or optimizing the process.");
                    break;
                }

                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUID);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab != null)
                {
                    try
                    {
                        FindMissingScripts(prefab);
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogWarning($"Error processing prefab {prefab.name}: {e.Message}");
                        LogAction($"Error processing prefab {prefab.name}: {e.Message}");
                    }
                }
            }

            string[] allScenes = AssetDatabase.FindAssets("t:Scene");
            foreach (string sceneGUID in allScenes)
            {
                if (GetCpuUsage() > cpuUsageThreshold)
                {
                    UnityEngine.Debug.LogWarning("CPU usage is too high! Consider stopping or optimizing the process.");
                    break;
                }

                string scenePath = AssetDatabase.GUIDToAssetPath(sceneGUID);
                try
                {
                    Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                    FindMissingScriptsInScene(scene);
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogWarning($"Error processing scene {scenePath}: {e.Message}");
                    LogAction($"Error processing scene {scenePath}: {e.Message}");
                }
            }

            if (objectsWithMissingScripts.Count == 0)
            {
                LogAction("No missing scripts found in project.");
            }
        }

        private float GetCpuUsage()
        {
            return SystemInfo.processorFrequency / 1000f;
        }

        private void RemoveMissingComponents()
        {
            foreach (GameObject obj in objectsWithMissingScripts)
            {
                if (obj != null)
                {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
                    LogAction("Removed missing components from: " + obj.name);
                }
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            objectsWithMissingScripts.Clear();
            LogAction("Missing components removed from scene.");
        }

        private void SaveScene()
        {
            if (selectedScene != null)
            {
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                LogAction("Scene saved.");
            }
        }

        private void ShowConfirmationDialog(string message, System.Action onConfirmed)
        {
            if (EditorUtility.DisplayDialog("Confirmation", message, "Yes", "No"))
            {
                onConfirmed.Invoke();
            }
        }

        private void LogAction(string message)
        {
            actionLog += $"{System.DateTime.Now}: {message}\n";

            UnityEngine.Debug.Log(message);
        }

        #endregion
    }
}

[InitializeOnLoad]
public class ScriptIconHandler
{
    static ScriptIconHandler()
    {
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
    }

    private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
    {
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);

        if (assetPath.EndsWith("MissingScriptWindow.cs"))
        {
            if (Event.current.type == EventType.MouseDown && selectionRect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                MissingScript.MissingScriptWindow.ShowWindow();
            }
        }
    }
}

#endif