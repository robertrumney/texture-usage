using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class TextureUsageWindow : EditorWindow
{
    private Vector2 scrollPos;
    private Dictionary<Texture, int> textureMemoryUsage = new Dictionary<Texture, int>();

    [MenuItem("Window/Texture Usage")]
    public static void ShowWindow()
    {
        GetWindow<TextureUsageWindow>("Texture Usage");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Get Texture Usage"))
        {
            GetTextureUsage();
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
        
        var sortedTextureUsage = textureMemoryUsage.OrderByDescending(x => x.Value).ToList();
        
        foreach (KeyValuePair<Texture, int> entry in sortedTextureUsage)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(entry.Key, typeof(Texture), false);
            EditorGUILayout.LabelField(" | Memory (KB): " + entry.Value / 1024f);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    void GetTextureUsage()
    {
        textureMemoryUsage.Clear();

        foreach (Texture texture in Resources.FindObjectsOfTypeAll<Texture>())
        {
            if(!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(texture))) // Ensure it's an asset and not an internal texture
            {
                int memSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(texture);
                textureMemoryUsage[texture] = memSize;
            }
        }
    }
}
