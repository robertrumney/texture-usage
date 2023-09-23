using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public class TextureUsageWindow : EditorWindow
{
    // The scroll position for the texture list
    private Vector2 scrollPos;

    // Dictionary to store each texture and its memory usage
    private Dictionary<Texture, long> textureMemoryUsage = new Dictionary<Texture, long>();

    // Menu item to open the Texture Usage window
    [MenuItem("Window/Texture Usage")]
    public static void ShowWindow()
    {
        GetWindow<TextureUsageWindow>("Texture Usage");
    }

    private void OnGUI()
    {
        // Button to fetch and display texture usage
        if (GUILayout.Button("Get Texture Usage"))
        {
            GetTextureUsage();
        }

        // Begin a scroll view for the texture list
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

        // Sort the textures by their memory usage in descending order
        var sortedTextureUsage = textureMemoryUsage.OrderByDescending(x => x.Value).ToList();

        // Display each texture and its memory usage
        foreach (KeyValuePair<Texture, long> entry in sortedTextureUsage)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(entry.Key, typeof(Texture), false);
            EditorGUILayout.LabelField(" | Memory (KB): " + entry.Value / 1024.0); // Convert bytes to KB
            EditorGUILayout.EndHorizontal();
        }

        // End the scroll view
        EditorGUILayout.EndScrollView();
    }

    // Method to fetch texture memory usage
    private void GetTextureUsage()
    {
        // Clear the existing data
        textureMemoryUsage.Clear();

        // Iterate over all textures in the project
        foreach (Texture texture in Resources.FindObjectsOfTypeAll<Texture>())
        {
            // Only consider textures that are assets (i.e., not internal or temporary textures)
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(texture)))
            {
                // Get the memory size of the texture in bytes
                long memSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(texture);

                // Store the texture and its memory size in the dictionary
                textureMemoryUsage[texture] = memSize;
            }
        }
    }
}
