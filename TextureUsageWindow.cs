using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public class TextureUsageWindow : EditorWindow
{
    private Vector2 scrollPos;
    private Dictionary<Texture, long> textureMemoryUsage = new Dictionary<Texture, long>();

    [MenuItem("Window/Texture Usage")]
    public static void ShowWindow()
    {
        GetWindow<TextureUsageWindow>("Texture Usage");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Get Texture Usage"))
        {
            GetTextureUsage();
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

        var sortedTextureUsage = textureMemoryUsage.OrderByDescending(x => x.Value).ToList();

        foreach (KeyValuePair<Texture, long> entry in sortedTextureUsage)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(entry.Key, typeof(Texture), false);
            EditorGUILayout.LabelField(" | Memory (KB): " + entry.Value / 1024.0);

            // Button to Ping objects in the scene using this texture
            if (GUILayout.Button("Ping in Scene"))
            {
                PingObjectsUsingTexture(entry.Key);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void GetTextureUsage()
    {
        textureMemoryUsage.Clear();

        foreach (Texture texture in Resources.FindObjectsOfTypeAll<Texture>())
        {
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(texture)))
            {
                long memSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(texture);
                textureMemoryUsage[texture] = memSize;
            }
        }
    }

    private void PingObjectsUsingTexture(Texture targetTexture)
    {
        List<GameObject> objectsUsingTexture = new List<GameObject>();
        Renderer[] renderers = GameObject.FindObjectsOfType<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (renderer.sharedMaterials != null) // Check if materials are assigned to the renderer
            {
                foreach (Material mat in renderer.sharedMaterials)
                {
                    if (mat != null && mat.HasProperty("_MainTex") && mat.mainTexture == targetTexture)
                    {
                        objectsUsingTexture.Add(renderer.gameObject);
                        break; // break out of the inner loop as we've found a match for this renderer
                    }
                }
            }
        }

        // Select the found objects in the scene
        Selection.objects = objectsUsingTexture.ToArray();
    }
}
