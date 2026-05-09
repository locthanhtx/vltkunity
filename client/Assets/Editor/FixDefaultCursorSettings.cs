#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class FixDefaultCursorSettings
{
    private const string CursorTexturePath = "Assets/Resources/SkillIcon/6.png";
    private const string StartScenePath = "Assets/Scenes/Splash.unity";

    static FixDefaultCursorSettings()
    {
        EditorApplication.delayCall += Apply;
    }

    [MenuItem("Tools/Fix Default Cursor Settings")]
    public static void Apply()
    {
        var changed = false;

        if (PlayerSettings.defaultCursor != null)
        {
            PlayerSettings.defaultCursor = null;
            PlayerSettings.cursorHotspot = Vector2.zero;
            changed = true;
        }

        var importer = AssetImporter.GetAtPath(CursorTexturePath) as TextureImporter;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
            changed = true;
        }

        var startScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(StartScenePath);
        if (startScene != null && EditorSceneManager.playModeStartScene != startScene)
        {
            EditorSceneManager.playModeStartScene = startScene;
            changed = true;
        }

        if (changed)
        {
            AssetDatabase.SaveAssets();
            Debug.Log("Fixed cursor settings and set Play Mode start scene to Splash.");
        }
    }

    [MenuItem("Tools/Open Splash Scene")]
    public static void OpenSplashScene()
    {
        EditorSceneManager.OpenScene(StartScenePath);
    }
}
#endif
