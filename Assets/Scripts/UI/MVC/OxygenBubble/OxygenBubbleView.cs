using UnityEngine;
using UnityEngine.UI;

#region ========== 氧气气泡视图 ==========

/// <summary>
/// 氧气气泡视图 —— 通过 Image fillAmount 控制气泡从上方消失
/// fillMethod=Vertical, fillOrigin=Bottom：氧气越低，上半部消失越多
/// </summary>
public class OxygenBubbleView : BaseView
{
    /// <summary>气泡图片（需在 Prefab/场景中配置 fillMethod=Vertical, fillOrigin=Bottom）</summary>
    [SerializeField] private Image _bubbleImage;

    protected override void OnAwake()
    {
        base.OnAwake();
        if (_bubbleImage == null)
            _bubbleImage = GetComponent<Image>();

        if (_bubbleImage != null)
        {
            _bubbleImage.type = Image.Type.Filled;
            _bubbleImage.fillMethod = Image.FillMethod.Vertical;
            _bubbleImage.fillOrigin = (int)Image.OriginVertical.Bottom;
        }
    }

    /// <summary>由 Controller 调用，按氧气百分比更新气泡填充</summary>
    public void Render(OxygenBubbleModel model)
    {
        if (_bubbleImage == null || model == null) return;
        _bubbleImage.fillAmount = model.OxygenPercentage;
    }
}

#endregion
