using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存档管理器 —— 负责存档的读写（基于 PlayerPrefs + JSON）
/// </summary>
public static class SaveManager
{
    private const string SAVE_KEY_PREFIX = "SaveSlot_";
    public const int MaxSaveSlots = 3;

    /// <summary>保存存档到指定槽位</summary>
    public static void Save(int slotIndex, SaveData data)
    {
        if (slotIndex < 0 || slotIndex >= MaxSaveSlots) return;

        data.isEmpty = false;
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY_PREFIX + slotIndex, json);
        PlayerPrefs.Save();
    }

    /// <summary>读取指定槽位的存档，若无则返回 null</summary>
    public static SaveData Load(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxSaveSlots) return null;

        if (!PlayerPrefs.HasKey(SAVE_KEY_PREFIX + slotIndex))
            return null;

        string json = PlayerPrefs.GetString(SAVE_KEY_PREFIX + slotIndex);
        return JsonUtility.FromJson<SaveData>(json);
    }

    /// <summary>获取所有槽位的存档列表（包括空槽位）</summary>
    public static List<SaveData> GetAllSaves()
    {
        var list = new List<SaveData>();
        for (int i = 0; i < MaxSaveSlots; i++)
        {
            SaveData data = Load(i);
            list.Add(data ?? new SaveData());
        }
        return list;
    }

    /// <summary>删除指定槽位的存档</summary>
    public static void Delete(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxSaveSlots) return;
        PlayerPrefs.DeleteKey(SAVE_KEY_PREFIX + slotIndex);
        PlayerPrefs.Save();
    }

    /// <summary>更新指定槽位的当前关卡</summary>
    public static void SetCurrentLevel(int slotIndex, int level)
    {
        SaveData data = Load(slotIndex) ?? new SaveData();
        data.currentLevel = level;
        Save(slotIndex, data);
    }
}
