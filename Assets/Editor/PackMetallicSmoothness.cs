using UnityEngine;
using UnityEditor;
using System.IO;

public class PackMetallicSmoothness : EditorWindow
{
    Texture2D metallicMap;
    Texture2D roughnessMap;

    [MenuItem("Tools/Pack Metallic + Roughness")]
    static void ShowWindow()
    {
        GetWindow<PackMetallicSmoothness>("Pack Metallic + Roughness");
    }

    void OnGUI()
    {
        GUILayout.Label("Pack Metallic + Roughness into Metallic(A)", EditorStyles.boldLabel);

        metallicMap = (Texture2D)EditorGUILayout.ObjectField("Metallic Map (R)", metallicMap, typeof(Texture2D), false);
        roughnessMap = (Texture2D)EditorGUILayout.ObjectField("Roughness Map", roughnessMap, typeof(Texture2D), false);

        if (GUILayout.Button("Pack and Save"))
        {
            if (metallicMap == null || roughnessMap == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign both Metallic and Roughness textures.", "OK");
                return;
            }

            string path = EditorUtility.SaveFilePanel("Save Packed Texture", "Assets", "MetallicSmoothness", "png");
            if (string.IsNullOrEmpty(path)) return;

            Texture2D packedTex = PackTextures(metallicMap, roughnessMap);
            File.WriteAllBytes(path, packedTex.EncodeToPNG());
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Done", "Packed texture saved successfully!", "OK");
        }
    }

    Texture2D PackTextures(Texture2D metallic, Texture2D roughness)
    {
        int width = metallic.width;
        int height = metallic.height;

        Texture2D packed = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color[] metallicPixels = metallic.GetPixels();
        Color[] roughnessPixels = roughness.GetPixels();

        for (int i = 0; i < metallicPixels.Length; i++)
        {
            float metallicValue = metallicPixels[i].r; // Red channel for Metallic
            float smoothnessValue = 1f - roughnessPixels[i].r; // Invert roughness to get smoothness

            packed.SetPixel(i % width, i / width, new Color(metallicValue, 0f, 0f, smoothnessValue));
        }

        packed.Apply();
        return packed;
    }
}
