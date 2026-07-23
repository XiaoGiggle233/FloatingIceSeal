using UnityEngine;
using UnityEditor;

/// <summary>
/// 视差图层编辑器辅助工具
/// 提供右键菜单快速设置和管理视差图层
/// </summary>
public static class ParallaxEditorHelper
{
    #region 右键菜单 —— 快速添加组件

    [MenuItem("GameObject/视差系统/添加 ParallaxLayer 组件", false, 0)]
    private static void AddParallaxLayer()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            if (go.GetComponent<SpriteRenderer>() == null)
            {
                Debug.LogWarning($"[ParallaxEditor] {go.name} 没有 SpriteRenderer 组件，跳过");
                continue;
            }

            ParallaxLayer layer = go.GetComponent<ParallaxLayer>();
            if (layer == null)
            {
                Undo.AddComponent<ParallaxLayer>(go);
                Debug.Log($"[ParallaxEditor] 已为 {go.name} 添加 ParallaxLayer");
            }
            else
            {
                Debug.Log($"[ParallaxEditor] {go.name} 已有 ParallaxLayer，跳过");
            }
        }
    }

    [MenuItem("GameObject/视差系统/添加 ParallaxLayer 并启用平铺", false, 1)]
    private static void AddParallaxLayerWithTiling()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            if (go.GetComponent<SpriteRenderer>() == null)
            {
                Debug.LogWarning($"[ParallaxEditor] {go.name} 没有 SpriteRenderer 组件，跳过");
                continue;
            }

            ParallaxLayer layer = Undo.AddComponent<ParallaxLayer>(go);
            // 通过反射设置 enableTiling（因为是private字段）
            var so = new SerializedObject(layer);
            var tilingProp = so.FindProperty("enableTiling");
            if (tilingProp != null)
            {
                tilingProp.boolValue = true;
                so.ApplyModifiedProperties();
            }
            Debug.Log($"[ParallaxEditor] 已为 {go.name} 添加 ParallaxLayer（平铺模式）");
        }
    }

    [MenuItem("GameObject/视差系统/创建 ParallaxController", false, 10)]
    private static void CreateParallaxController()
    {
        GameObject go = new GameObject("ParallaxController");
        Undo.RegisterCreatedObjectUndo(go, "Create ParallaxController");
        go.AddComponent<ParallaxController>();
        Selection.activeGameObject = go;
        Debug.Log("[ParallaxEditor] 已创建 ParallaxController");
    }

    #endregion

    #region 右键菜单验证

    [MenuItem("GameObject/视差系统/添加 ParallaxLayer 组件", true)]
    [MenuItem("GameObject/视差系统/添加 ParallaxLayer 并启用平铺", true)]
    private static bool ValidateSelection()
    {
        if (Selection.gameObjects.Length == 0) return false;
        foreach (GameObject go in Selection.gameObjects)
        {
            if (go.GetComponent<SpriteRenderer>() != null)
                return true;
        }
        return false;
    }

    #endregion

    #region 批量设置视差系数

    [MenuItem("Tools/视差系统/批量设置选中图层的视差系数", false, 20)]
    private static void BatchSetParallaxFactor()
    {
        ParallaxFactorWindow.ShowWindow();
    }

    #endregion
}

/// <summary>
/// 批量设置视差系数的编辑器窗口
/// </summary>
internal class ParallaxFactorWindow : EditorWindow
{
    private float factor = 0.5f;
    private string factorLabel = "0.5 (中景)";

    public static void ShowWindow()
    {
        ParallaxFactorWindow window = GetWindow<ParallaxFactorWindow>(true, "批量设置视差系数");
        window.minSize = new Vector2(350, 220);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("设置选中GameObject的视差系数", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "< 0  = 超远景（朝相机反方向移动）\n" +
            "  0  = 远景（完全跟随镜头）\n" +
            "0.5  = 中景\n" +
            "  1  = 近景（固定在世界中）\n" +
            "> 1  = 超近景（比相机移动更快）",
            MessageType.Info);

        GUILayout.Space(5);

        // 滑块：默认范围 [-1, 2]，方便快速调整
        factor = EditorGUILayout.Slider("视差系数（滑块）", factor, -1f, 2f);

        // 精确输入框
        factor = EditorGUILayout.FloatField("视差系数（精确）", factor);

        // 显示描述
        UpdateFactorLabel();

        EditorGUILayout.LabelField("等效距离", factorLabel);
        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("应用", GUILayout.Height(30)))
        {
            ApplyFactor();
        }
        if (GUILayout.Button("应用并关闭", GUILayout.Height(30)))
        {
            ApplyFactor();
            Close();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void UpdateFactorLabel()
    {
        if (factor < 0f)
            factorLabel = $"{factor:F2} (超远景/反向移动)";
        else if (factor < 0.15f)
            factorLabel = $"{factor:F2} (远景/天空)";
        else if (factor < 0.35f)
            factorLabel = $"{factor:F2} (远中景)";
        else if (factor < 0.65f)
            factorLabel = $"{factor:F2} (中景)";
        else if (factor < 0.85f)
            factorLabel = $"{factor:F2} (近中景)";
        else if (factor <= 1f)
            factorLabel = $"{factor:F2} (近景/前景)";
        else
            factorLabel = $"{factor:F2} (超近景/高速移动)";
    }

    private void ApplyFactor()
    {
        int count = 0;
        foreach (GameObject go in Selection.gameObjects)
        {
            ParallaxLayer layer = go.GetComponent<ParallaxLayer>();
            if (layer == null) continue;

            var so = new SerializedObject(layer);
            var prop = so.FindProperty("parallaxFactor");
            if (prop != null)
            {
                prop.floatValue = factor;
                so.ApplyModifiedProperties();
                count++;
            }
        }

        Debug.Log($"[ParallaxEditor] 已设置 {count} 个图层的视差系数为 {factor:F2}");
        if (count == 0)
            Debug.LogWarning("[ParallaxEditor] 选中的对象中没有找到 ParallaxLayer 组件");
    }
}
