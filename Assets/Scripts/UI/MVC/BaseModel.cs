#region ========== 模型基类 ==========

/// <summary>
/// 模型基类
/// 负责持有数据，供 Controller 读写
/// 每个 Controller 通常对应一个 Model
/// 
/// 使用方式：
///   继承 BaseModel，在 Init() 中初始化数据
///   Controller 通过 GetModel&lt;T&gt;() 获取类型安全的模型引用
/// </summary>
public class BaseModel
{
    /// <summary>所属的控制器引用</summary>
    public BaseController Controller { get; set; }

    /// <summary>
    /// 构造函数（带控制器引用）
    /// </summary>
    public BaseModel(BaseController ctl)
    {
        Controller = ctl;
    }

    /// <summary>
    /// 无参构造函数
    /// </summary>
    public BaseModel()
    {
    }

    /// <summary>
    /// 初始化模型数据
    /// 在 Controller.Init() 阶段被调用
    /// </summary>
    public virtual void Init()
    {
    }
}

#endregion
