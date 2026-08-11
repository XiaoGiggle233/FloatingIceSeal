using UnityEngine;

#region ========== 氧气气泡控制器 ==========

/// <summary>
/// 氧气气泡控制器 —— 监听 SealModel 氧气变化，更新 Model 并驱动 View 渲染
/// </summary>
public class OxygenBubbleController : BaseController
{
    private OxygenBubbleView _view;
    private OxygenBubbleModel _bubbleModel;

    private SealModel _sealModel;
    private float _lastPercentage = -1f;

    public override void Init()
    {
        _bubbleModel = new OxygenBubbleModel();
        SetModel(_bubbleModel);
        _bubbleModel.Init();
    }

    public override void OnLoadView(IBaseView view)
    {
        _view = view as OxygenBubbleView;
        if (_view != null)
        {
            FindSealModel();
            RenderIfChanged();
        }
    }

    /// <summary>每帧由 Entry 调用，检测氧气变化并刷新 UI</summary>
    public void Tick()
    {
        if (_view == null) return;

        if (_sealModel == null)
        {
            FindSealModel();
            return;
        }

        RenderIfChanged();
    }

    private void FindSealModel()
    {
        var seal = Object.FindObjectOfType<Seal>();
        if (seal != null)
            _sealModel = seal.GetComponent<SealModel>();
    }

    private void RenderIfChanged()
    {
        if (_sealModel == null) return;

        _bubbleModel.OxygenValue = _sealModel.OxygenValue;
        _bubbleModel.OxygenMaxValue = _sealModel.OxygenMaxValue;

        float pct = _bubbleModel.OxygenPercentage;
        if (!Mathf.Approximately(pct, _lastPercentage))
        {
            _lastPercentage = pct;
            _view?.Render(_bubbleModel);
        }
    }

    public override void Destroy()
    {
        base.Destroy();
        _view = null;
        _sealModel = null;
    }
}

#endregion
