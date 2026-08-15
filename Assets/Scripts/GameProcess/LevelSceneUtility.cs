using System.IO;
using UnityEngine.SceneManagement;

/// <summary>
/// 关卡场景名称匹配工具 —— 按关卡序号查找 BuildSettings 中的场景
/// </summary>
public static class LevelSceneUtility
{
    /// <summary>
    /// 查找关卡场景名：匹配 "Level{level}" 或 "Level{level}_{后缀}"
    /// </summary>
    public static string FindSceneName(int level)
    {
        string exactName = $"Level{level}";
        string prefix = exactName + "_";

        string suffixMatch = null;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            if (string.IsNullOrEmpty(path)) continue;

            string name = Path.GetFileNameWithoutExtension(path);
            if (name == exactName) return name;      // 精确匹配优先
            if (suffixMatch == null && name.StartsWith(prefix)) suffixMatch = name;
        }

        return suffixMatch;
    }
}
