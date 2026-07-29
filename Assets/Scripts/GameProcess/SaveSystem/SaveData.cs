using System;

/// <summary>
/// 存档数据结构
/// </summary>
[Serializable]
public class SaveData
{
    /// <summary>当前关卡，默认为 1</summary>
    public int currentLevel = 1;

    /// <summary>存档是否为空（未曾使用过）</summary>
    public bool isEmpty = true;
}
