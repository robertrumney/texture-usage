using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Collections.Generic;

public class TextureUsageWindow : EditorWindow
{
    // The position for the scroll view in the Editor window.
    private Vector2 scrollPos;

    // Dictionary to hold each texture and its corresponding memory usage.
    private Dictionary<Texture, long> textureMemoryUsage = new Dictionary<Texture, long>();

    [MenuItem("Window/Texture Usage")]
    public static void ShowWindow()
    {
        // Open the TextureUsageWindow.
        GetWindow<TextureUsageWindow>("Texture Usage");
    }

    private void OnGUI()
    {
        // Button to fetch texture memory usage.
        if (GUILayout.Button("Get Texture Usage"))
        {
            GetTextureUsage();
        }

        // Start of the scroll view.
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

        // Sort the texture usage by memory consumption.
        var sortedTextureUsage = textureMemoryUsage.OrderByDescending(x => x.Value).ToList();

        // Display each texture and its memory usage.
        foreach (KeyValuePair<Texture, long> entry in sortedTextureUsage)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(entry.Key, typeof(Texture), false);
            EditorGUILayout.LabelField(" | Memory (KB): " + entry.Value / 1024.0);

            // Button to locate game objects in the scene using this texture.
            if (GUILayout.Button("Ping in Scene"))
            {
                PingObjectsUsingTexture(entry.Key);
            }

            EditorGUILayout.EndHorizontal();
        }

        // End of the scroll view.
        EditorGUILayout.EndScrollView();
    }

    // Populates the textureMemoryUsage dictionary with memory usage of each texture in the project.
    private void GetTextureUsage()
    {
        // Clear the previous texture memory usage data.
        textureMemoryUsage.Clear();

        // Go through all the textures in the project.
        foreach (Texture texture in Resources.FindObjectsOfTypeAll<Texture>())
        {
            // Ensure the texture has a path in the AssetDatabase.
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(texture)))
            {
                long memSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(texture);
                textureMemoryUsage[texture] = memSize;
            }
        }
    }

    // Locates and selects all game objects in the scene that are using the targetTexture.
    private void PingObjectsUsingTexture(Texture targetTexture)
    {
        // List to store game objects using the target texture.
        List<GameObject> objectsUsingTexture = new List<GameObject>();

        // Fetch all renderers in the scene.
        Renderer[] renderers = GameObject.FindObjectsOfType<Renderer>();

        // Go through each renderer to check if it uses the target texture.
        foreach (Renderer renderer in renderers)
        {
            // Ensure the renderer has materials assigned.
            if (renderer.sharedMaterials != null)
            {
                foreach (Material mat in renderer.sharedMaterials)
                {
                    // Check if the material uses the target texture as its main texture.
                    if (mat != null && mat.HasProperty("_MainTex") && mat.mainTexture == targetTexture)
                    {
                        objectsUsingTexture.Add(renderer.gameObject);
                        break;
                    }
                }
            }
        }

        // Set the Unity selection to the found game objects.
        Selection.objects = objectsUsingTexture.ToArray();
    }
}
