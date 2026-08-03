#region ========== 视图接口 ==========

/// <summary>
/// 视图接口
/// 所有 View 类必须实现此接口
/// 
/// 生命周期：
///   创建 → InitUI() → InitData() → Open(args) → ... → Close(args) → DestroyView()
/// </summary>
public interface IBaseView
{
    /// <summary>视图是否已完成初始化</summary>
    bool IsInit();

    /// <summary>视图是否正在显示</summary>
    bool IsShow();

    /// <summary>初始化 UI 元素（绑定组件引用）</summary>
    void InitUI();

    /// <summary>初始化数据（在 UI 初始化之后调用）</summary>
    void InitData();

    /// <summary>打开视图</summary>
    void Open(params object[] args);

    /// <summary>关闭视图</summary>
    void Close(params object[] args);

    /// <summary>销毁视图（移除 GameObject）</summary>
    void DestroyView();

    /// <summary>向所属 Controller 发送消息</summary>
    void ApplyFunc(string eventName, params object[] args);

    /// <summary>向指定 Controller 发送消息</summary>
    void ApplyControllerFunc(int controllerKey, string eventName, params object[] args);

    /// <summary>设置视图显隐</summary>
    void SetVisible(bool visible);

    /// <summary>视图唯一 ID（由 ViewManager 分配）</summary>
    int ViewId { get; set; }

    /// <summary>视图对应的 Controller</summary>
    BaseController Controller { get; set; }
}

#endregion
