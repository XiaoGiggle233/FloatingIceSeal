using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#region ========== 视图配置 ==========

/// <summary>
/// 视图注册信息
/// 定义视图的加载参数
/// </summary>
public class ViewInfo
{
    /// <summary>Prefab 名称（从 Resources 目录加载）</summary>
    public string PrefabName;

    /// <summary>视图父节点 Transform</summary>
    public Transform ParentTf;

    /// <summary>视图对应的控制器</summary>
    public BaseController Controller;

    /// <summary>渲染层级（Canvas sortingOrder）</summary>
    public int SortingOrder;

    /// <summary>视图脚本的完整类型名（如 "BattleLogView"），用于 AddComponent。
    /// 若未指定，则尝试从 ViewType 枚举名推断</summary>
    public string ViewTypeName;

    /// <summary>
    /// 创建视图配置
    /// </summary>
    /// <param name="prefabName">Prefab 名称（位于 Resources/ 下）</param>
    /// <param name="parent">父节点</param>
    /// <param name="controller">关联的控制器</param>
    /// <param name="viewTypeName">视图脚本类型名（如 "MyView"），用于 AddComponent</param>
    /// <param name="sortingOrder">渲染层级</param>
    public ViewInfo(string prefabName, Transform parent, BaseController controller, string viewTypeName = null, int sortingOrder = 0)
    {
        PrefabName = prefabName;
        ParentTf = parent;
        Controller = controller;
        ViewTypeName = viewTypeName;
        SortingOrder = sortingOrder;
    }
}

#endregion

#region ========== 视图管理器 ==========

/// <summary>
/// 视图管理器
/// 负责所有 View 的加载、打开、关闭、缓存和销毁
/// 
/// 核心流程：
///   1. Register() 注册视图信息
///   2. Open()  从 Resources 加载 Prefab → 实例化 → 初始化
///   3. Close() 隐藏视图
///   4. Destroy() 销毁视图
/// 
/// 依赖：
///   - Unity 引擎（Resources.Load、GameObject.Instantiate、Canvas）
///   - 需要场景中存在名为 "Canvas" 的根物体
/// </summary>
public class ViewManager
{
    #region 字段

    /// <summary>UI Canvas 的 Transform（默认查找 "Canvas"）</summary>
    public Transform CanvasTf;

    /// <summary>世界空间 Canvas 的 Transform（默认查找 "WorldCanvas"）</summary>
    public Transform WorldCanvasTf;

    /// <summary>已打开的视图：视图ID → 视图实例</summary>
    private Dictionary<int, IBaseView> _opens;

    /// <summary>视图缓存池：视图ID → 视图实例（已加载但未显示的视图）</summary>
    private Dictionary<int, IBaseView> _viewCache;

    /// <summary>已注册的视图信息：视图ID → 视图配置</summary>
    private Dictionary<int, ViewInfo> _views;

    /// <summary>Prefab 加载根路径，默认 "View/"</summary>
    public string PrefabRootPath = "View/";

    #endregion

    #region 构造

    public ViewManager()
    {
        CanvasTf = GameObject.Find("Canvas")?.transform;
        WorldCanvasTf = GameObject.Find("WorldCanvas")?.transform;
        _opens = new Dictionary<int, IBaseView>();
        _viewCache = new Dictionary<int, IBaseView>();
        _views = new Dictionary<int, ViewInfo>();
    }

    #endregion

    #region 注册与注销

    /// <summary>注册视图（通过视图类型枚举）</summary>
    public void Register(Enum viewType, ViewInfo viewInfo)
    {
        Register(Convert.ToInt32(viewType), viewInfo);
    }

    /// <summary>注册视图（通过整型 ID）</summary>
    public void Register(int key, ViewInfo viewInfo)
    {
        if (!_views.ContainsKey(key))
        {
            _views.Add(key, viewInfo);
        }
    }

    /// <summary>注销视图信息</summary>
    public void Unregister(int key)
    {
        if (_views.ContainsKey(key))
        {
            _views.Remove(key);
        }
    }

    /// <summary>移除视图（从所有字典中彻底清除）</summary>
    public void RemoveView(int key)
    {
        _views.Remove(key);
        _viewCache.Remove(key);
        _opens.Remove(key);
    }

    /// <summary>移除指定控制器关联的所有视图</summary>
    public void RemoveViewByController(BaseController ctl)
    {
        foreach (var item in _views.ToList())
        {
            if (item.Value.Controller == ctl)
            {
                RemoveView(item.Key);
            }
        }
    }

    #endregion

    #region 查询

    /// <summary>视图是否已打开</summary>
    public bool IsOpen(int key)
    {
        return _opens.ContainsKey(key);
    }

    /// <summary>获取视图实例（优先从打开列表，其次缓存）</summary>
    public IBaseView GetView(int key)
    {
        if (_opens.ContainsKey(key))
            return _opens[key];
        if (_viewCache.ContainsKey(key))
            return _viewCache[key];
        return null;
    }

    /// <summary>获取类型安全的视图实例</summary>
    public T GetView<T>(int key) where T : class, IBaseView
    {
        return GetView(key) as T;
    }

    #endregion

    #region 打开视图

    /// <summary>打开视图（通过枚举）</summary>
    public void Open(Enum viewType, params object[] args)
    {
        Open(Convert.ToInt32(viewType), args);
    }

    /// <summary>
    /// 打开视图
    /// 如果视图未加载，从 Resources 加载 Prefab 并实例化
    /// 如果已打开，不重复打开
    /// </summary>
    /// <param name="key">视图 ID</param>
    /// <param name="args">传递给 Open() 的参数</param>
    public void Open(int key, params object[] args)
    {
        IBaseView view = GetView(key);

        if (!_views.TryGetValue(key, out ViewInfo viewInfo))
        {
            Debug.LogError($"[ViewManager.Open] 视图 ID={key} 未注册");
            return;
        }

        if (view == null)
        {
            // 视图不在内存中，加载并实例化
            view = LoadView(key, viewInfo);
            if (view == null) return;

            // 缓存
            _viewCache[key] = view;
            viewInfo.Controller.OnLoadView(view);
        }

        // 已打开则直接返回
        if (_opens.ContainsKey(key))
        {
            return;
        }

        _opens[key] = view;

        // 初始化流程
        if (view.IsInit())
        {
            view.SetVisible(true);
            view.Open(args);
            viewInfo.Controller.OpenView(view);
        }
        else
        {
            view.InitUI();
            view.InitData();
            view.Open(args);
            viewInfo.Controller.OpenView(view);
        }
    }

    /// <summary>
    /// 从 Resources 加载视图 Prefab 并实例化
    /// 重写此方法可自定义加载策略（如 AssetBundle / Addressables）
    /// </summary>
    protected virtual IBaseView LoadView(int key, ViewInfo viewInfo)
    {
        // 确定视图脚本类型名：优先使用 ViewInfo 中指定的，其次用 key 对应的 ViewType 枚举名
        string typeName = viewInfo.ViewTypeName;
        if (string.IsNullOrEmpty(typeName))
        {
            typeName = key.ToString();
        }

        string prefabPath = PrefabRootPath + viewInfo.PrefabName;

        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[ViewManager.LoadView] 未找到 Prefab: Resources/{prefabPath}");
            return null;
        }

        Transform parent = viewInfo.ParentTf ?? CanvasTf;
        GameObject uiObj = UnityEngine.Object.Instantiate(prefab, parent);

        // 确保有 Canvas 和 GraphicRaycaster 组件
        Canvas canvas = uiObj.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = uiObj.AddComponent<Canvas>();
        }
        if (uiObj.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
        {
            uiObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        canvas.overrideSorting = true;
        canvas.sortingOrder = viewInfo.SortingOrder;

        // 添加视图脚本组件
        Type viewType = Type.GetType(typeName);
        if (viewType == null)
        {
            // 尝试在主程序集中查找
            viewType = Type.GetType(typeName + ", Assembly-CSharp");
        }
        if (viewType == null)
        {
            Debug.LogError($"[ViewManager.LoadView] 找不到视图类型: {typeName}，请在 ViewInfo 中指定正确的 ViewTypeName");
            UnityEngine.Object.Destroy(uiObj);
            return null;
        }

        IBaseView view = uiObj.AddComponent(viewType) as IBaseView;
        if (view == null)
        {
            Debug.LogError($"[ViewManager.LoadView] 类型 {typeName} 未实现 IBaseView");
            UnityEngine.Object.Destroy(uiObj);
            return null;
        }

        view.ViewId = key;
        view.Controller = viewInfo.Controller;
        return view;
    }

    #endregion

    #region 关闭视图

    /// <summary>关闭视图（通过枚举）</summary>
    public void Close(Enum viewType, params object[] args)
    {
        Close(Convert.ToInt32(viewType), args);
    }

    /// <summary>关闭视图</summary>
    public void Close(int key, params object[] args)
    {
        if (!IsOpen(key))
        {
            return;
        }

        IBaseView view = GetView(key);
        if (view != null)
        {
            _opens.Remove(key);
            view.Close(args);
            _views[key].Controller.CloseView(view);
        }
    }

    /// <summary>关闭所有已打开的视图</summary>
    public void CloseAll()
    {
        List<IBaseView> list = _opens.Values.ToList();
        for (int i = list.Count - 1; i >= 0; i--)
        {
            Close(list[i].ViewId);
        }
    }

    #endregion

    #region 销毁

    /// <summary>销毁指定视图（从所有字典中移除并销毁 GameObject）</summary>
    public void Destroy(int key)
    {
        IBaseView oldView = GetView(key);
        if (oldView != null)
        {
            Unregister(key);
            oldView.DestroyView();
            _viewCache.Remove(key);
            _opens.Remove(key);
        }
    }

    #endregion

    #region 特效辅助（可选）

    /// <summary>
    /// 显示浮动伤害/治疗数字
    /// 这是一个扩展功能示例，非核心必须
    /// </summary>
    /// <param name="text">显示文本</param>
    /// <param name="color">文本颜色</param>
    /// <param name="worldPos">世界坐标位置</param>
    /// <param name="duration">持续时间（秒）</param>
    public void ShowFloatingText(string text, Color color, Vector3 worldPos, float duration = 0.75f)
    {
        GameObject obj = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(PrefabRootPath + "HitNum"), CanvasTf);
        if (obj == null) return;

        obj.transform.position = worldPos;
        UnityEngine.Object.Destroy(obj, duration);

        var hitTxt = obj.GetComponent<UnityEngine.UI.Text>();
        if (hitTxt != null)
        {
            hitTxt.text = text;
            hitTxt.color = color;
        }
    }

    #endregion
}

#endregion
