using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#region ========== 切换存档视图 ==========

/// <summary>
/// 切换存档视图 —— 绑定存档槽位、复制、删除、返回按钮，将交互通过事件上报 Controller
/// </summary>
public class SwitchSaveView : BaseView
{
    /// <summary>存档槽位按钮点击事件（携带槽位索引）</summary>
    public event Action<int> SaveSlotClicked;

    /// <summary>复制存档按钮点击事件</summary>
    public event Action CopyClicked;

    /// <summary>删除存档按钮点击事件</summary>
    public event Action DeleteClicked;

    /// <summary>返回按钮点击事件</summary>
    public event Action BackClicked;

    private Button[] _slotBtns;
    private Button _copyBtn;
    private Button _deleteBtn;
    private Button _backBtn;

    protected override void OnAwake()
    {
        base.OnAwake();

        _slotBtns = new Button[SaveManager.MaxSaveSlots];
        for (int i = 0; i < _slotBtns.Length; i++)
        {
            int index = i;
            _slotBtns[i] = Find<Button>($"SaveSlot{i}");
            _slotBtns[i]?.onClick.AddListener(() => SaveSlotClicked?.Invoke(index));
        }

        _copyBtn = Find<Button>("CopyBtn");
        _deleteBtn = Find<Button>("DeleteBtn");
        _backBtn = Find<Button>("BackBtn");

        _copyBtn?.onClick.AddListener(() => CopyClicked?.Invoke());
        _deleteBtn?.onClick.AddListener(() => DeleteClicked?.Invoke());
        _backBtn?.onClick.AddListener(() => BackClicked?.Invoke());
    }

    /// <summary>渲染存档信息（由 Controller 调用）</summary>
    public void Render(SwitchSaveModel model)
    {
        if (model == null || model.SaveSlots == null) return;

        for (int i = 0; i < _slotBtns.Length && i < model.SaveSlots.Count; i++)
        {
            Button btn = _slotBtns[i];
            if (btn == null) continue;

            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            SaveData save = model.SaveSlots[i];
            bool hasSave = save != null && !save.isEmpty;
            if (label != null)
                label.text = hasSave ? $"存档 {i + 1}\n关卡 {save.currentLevel}" : $"存档 {i + 1}\n（空）";

            // 高亮选中的槽位
            var colors = btn.colors;
            colors.normalColor = i == model.SelectedSlotIndex ? new Color(0.6f, 0.85f, 1f) : Color.white;
            btn.colors = colors;
        }
    }

    /// <summary>显示/隐藏切换存档面板</summary>
    public void SetMenuVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _slotBtns.Length; i++)
            _slotBtns[i]?.onClick.RemoveAllListeners();
        _copyBtn?.onClick.RemoveAllListeners();
        _deleteBtn?.onClick.RemoveAllListeners();
        _backBtn?.onClick.RemoveAllListeners();
    }
}

#endregion
