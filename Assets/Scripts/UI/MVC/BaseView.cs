using System.Collections.Generic;
using UnityEngine;

#region ========== 视图基类 ==========

/// <summary>
/// 视图基类（MonoBehaviour）
/// 提供 UI 元素查找缓存、生命周期管理等通用功能
/// 
/// 使用方式：
///   继承 BaseView，在 OnAwake() 中绑定 UI 组件
///   在 InitUI() / InitData() 中做初始化
///   在 Open(args) / Close(args) 中处理打开/关闭逻辑
/// 
/// 注意：
///   - 依赖 Unity 引擎（MonoBehaviour）
///   - 非 Unity 项目请自行实现 IBaseView 接口
/// </summary>
public class BaseView : MonoBehaviour, IBaseView
{
    #region IBaseView 实现

    /// <summary>视图 ID，由 ViewManager 注册时分配</summary>
    public int ViewId { get; set; }

    /// <summary>视图对应的控制器</summary>
    public BaseController Controller { get; set; }

    #endregion

    #region 私有字段

    /// <summary>Canvas 组件（用于控制显隐）</summary>
    protected Canvas _canvas;

    /// <summary>UI 子元素查找缓存：路径 -> GameObject</summary>
    protected Dictionary<string, GameObject> _cacheGos = new Dictionary<string, GameObject>();

    /// <summary>是否已完成初始化</summary>
    private bool _isInit = false;

    #endregion

    #region Unity 生命周期

    void Awake()
    {
        _canvas = gameObject.GetComponent<Canvas>();
        OnAwake();
    }

    void Start()
    {
        OnStart();
    }

    /// <summary>Awake 回调，子类重写以绑定 UI 组件</summary>
    protected virtual void OnAwake()
    {
    }

    /// <summary>Start 回调，子类重写以执行依赖其他对象的初始化</summary>
    protected virtual void OnStart()
    {
    }

    #endregion

    #region IBaseView 方法实现

    /// <summary>是否已完成初始化</summary>
    public bool IsInit()
    {
        return _isInit;
    }

    /// <summary>视图是否正在显示（通过 Canvas.enabled 判断）</summary>
    public bool IsShow()
    {
        return _canvas != null && _canvas.enabled;
    }

    /// <summary>设置视图可见性</summary>
    public void SetVisible(bool visible)
    {
        if (_canvas != null)
        {
            _canvas.enabled = visible;
        }
    }

    /// <summary>初始化 UI（绑定引用、设置初始状态）</summary>
    public virtual void InitUI()
    {
    }

    /// <summary>初始化数据（在 UI 初始化后调用）</summary>
    public virtual void InitData()
    {
        _isInit = true;
    }

    /// <summary>打开视图</summary>
    /// <param name="args">可变参数，由调用方传入</param>
    public virtual void Open(params object[] args)
    {
    }

    /// <summary>关闭视图（默认隐藏，子类可重写以增加清理逻辑）</summary>
    public virtual void Close(params object[] args)
    {
        SetVisible(false);
    }

    /// <summary>销毁视图 GameObject</summary>
    public void DestroyView()
    {
        Controller = null;
        Destroy(gameObject);
    }

    /// <summary>向所属 Controller 发送消息</summary>
    public void ApplyFunc(string eventName, params object[] args)
    {
        Controller?.ApplyFunc(eventName, args);
    }

    /// <summary>向其他 Controller 发送跨控制器消息</summary>
    public void ApplyControllerFunc(int controllerKey, string eventName, params object[] args)
    {
        Controller?.ApplyControllerFunc(controllerKey, eventName, args);
    }

    #endregion

    #region UI 查找工具方法

    /// <summary>
    /// 按路径查找子物体（带缓存）
    /// 首次查找后缓存结果，后续直接从字典获取
    /// </summary>
    /// <param name="res">相对于 transform 的路径，如 "bg/title"</param>
    /// <returns>找到的 GameObject，未找到返回 null</returns>
    public GameObject Find(string res)
    {
        if (_cacheGos.ContainsKey(res))
        {
            return _cacheGos[res];
        }

        Transform foundTransform = transform.Find(res);
        if (foundTransform == null)
        {
            Debug.LogError($"[BaseView.Find] 在 '{gameObject.name}' 中未找到路径 '{res}'");
            return null;
        }

        _cacheGos.Add(res, foundTransform.gameObject);
        return _cacheGos[res];
    }

    /// <summary>
    /// 按路径查找子物体上的组件（带缓存）
    /// </summary>
    /// <typeparam name="T">目标组件类型</typeparam>
    /// <param name="res">相对于 transform 的路径</param>
    /// <returns>找到的组件，未找到返回 null</returns>
    public T Find<T>(string res) where T : Component
    {
        GameObject obj = Find(res);
        if (obj == null)
        {
            Debug.LogError($"[BaseView.Find<T>] 未找到路径 '{res}' 以获取组件 {typeof(T).Name}");
            return null;
        }

        T component = obj.GetComponent<T>();
        if (component == null)
        {
            Debug.LogError($"[BaseView.Find<T>] 在 '{res}' 上未找到组件 {typeof(T).Name}");
        }
        return component;
    }

    #endregion
}

#endregion
