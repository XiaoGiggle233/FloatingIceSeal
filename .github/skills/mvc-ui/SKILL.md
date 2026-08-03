---
name: mvc-ui
description: '使用项目自带 MVC UI 框架（Model-View-Controller）为 Unity 界面建模。Use when: 开发/重构 UI 界面、将现有 MonoBehaviour UI 脚本重构为 MVC、修改现有 MVC UI 的交互逻辑、添加/修改按钮行为、创建主菜单/暂停菜单/HUD/设置面板、使用 BaseView/BaseController/BaseModel、ViewManager/ControllerManager 注册、UI 按钮事件与数据解耦。触发词：MVC、Model、View、Controller、BaseView、BaseController、BaseModel、ViewManager、ControllerManager、OnLoadView、SetModel、UI重构、界面建模、修改UI交互、改按钮逻辑、新增按钮。'
argument-hint: '[要制作/重构/修改的 UI 界面描述]'
---

# Unity MVC UI 框架建模

使用项目自带的 MVC UI 框架（[Assets/Scripts/MVC](../../../Assets/Scripts/MVC/)）为界面建模，将**数据（Model）、逻辑（Controller）、显示（View）**解耦。

## 框架文件位置

- 控制器基类: [BaseController.cs](../../../Assets/Scripts/MVC/BaseController.cs)
- 视图基类: [BaseView.cs](../../../Assets/Scripts/MVC/BaseView.cs)
- 模型基类: [BaseModel.cs](../../../Assets/Scripts/MVC/BaseModel.cs)
- 视图接口: [IBaseView.cs](../../../Assets/Scripts/MVC/IBaseView.cs)
- 视图管理器: [ViewManager.cs](../../../Assets/Scripts/MVC/ViewManager.cs)
- 控制器管理器: [ControllerManager.cs](../../../Assets/Scripts/MVC/ControllerManager.cs)
- 类型枚举模板: [ControllerType_Template.cs](../../../Assets/Scripts/MVC/ControllerType_Template.cs) / [ViewType_Template.cs](../../../Assets/Scripts/MVC/ViewType_Template.cs)
- 全局入口: [MVCManager.cs](../../../Assets/Scripts/MVC/MVCManager.cs)（懒加载单例 `ControllerManager`）
- 使用文档: [使用文档.md](../../../Assets/Scripts/MVC/使用文档.md)
- 参考实现（主菜单）: [MainMenu/](../../../Assets/Scripts/MVC/MainMenu/)、参考实现（暂停）: [PauseMenu/](../../../Assets/Scripts/MVC/PauseMenu/)

## 通信方向

```
View ──Action事件──► Controller ──方法调用──► View（渲染）
Controller ──GameEvents──► 外部模块
外部模块 ──GameEvents──► Controller ──► View（渲染）
```

- **View → Controller**：C# `event`/`Action` 上报用户交互
- **Controller → View**：直接调用 View 的渲染方法（`Render(model)`）
- **外部模块 ↔ Controller**：通过 `GameEvents.Listen/Publish`（见 unity-event-setup skill）

## 工作流程

### 步骤 1：确定需要 MVC 建模的界面

确认界面的性质：

| 界面类型 | 是否适合 MVC | 说明 |
|----------|:---:|------|
| 有按钮交互 + 需要显示数据的面板（主菜单、暂停菜单、HUD、设置） | ✅ | 数据/逻辑/显示分离收益大 |
| 纯静态展示、无交互无数据 | ❌ | 直接挂脚本即可，MVC 过度设计 |
| 单次一次性动画/特效 | ❌ | 不建模 |

### 步骤 2：创建 Model（数据）

继承 `BaseModel`，用公开字段持有数据，在 `Init()` 中给默认值：

```csharp
public class MainMenuModel : BaseModel
{
    public int CurrentSlotIndex;
    public int CurrentLevel;
    public int MaxSaveSlots;

    public override void Init()
    {
        CurrentSlotIndex = 0;
        CurrentLevel = 1;
        MaxSaveSlots = SaveManager.MaxSaveSlots;
    }
}
```

### 步骤 3：创建 View（界面）

继承 `BaseView`，在 `OnAwake()` 中绑定 UI 组件并注册按钮回调。**View 只做纯展示与交互上报，不做业务逻辑**：

```csharp
using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : BaseView
{
    // 1. 定义交互事件，供 Controller 订阅
    public event Action StartGameClicked;

    // 2. 持有 UI 组件引用（用 Find<> 按路径查找，带缓存）
    private Button _startBtn;

    protected override void OnAwake()
    {
        base.OnAwake();
        _startBtn = Find<Button>("StartGameBtn");
        _startBtn?.onClick.AddListener(() => StartGameClicked?.Invoke());
    }

    // 3. 渲染方法由 Controller 调用
    public void Render(MainMenuModel model)
    {
        // 更新 UI 文本...
    }

    private void OnDestroy()
    {
        _startBtn?.onClick.RemoveAllListeners();
    }
}
```

**View 查找工具（BaseView 提供）**：
- `Find(string path)`：按路径查找子物体（缓存），路径相对自身，如 `"Scroll View/Viewport/Content"`
- `Find<T>(string path)`：查找子物体上的组件
- `SetVisible(bool)`：通过 Canvas.enabled 控制显隐；`gameObject.SetActive()` 控制 GameObject 显隐

### 步骤 4：创建 Controller（逻辑）

继承 `BaseController`，持有 Model 与 View 引用，处理业务逻辑：

```csharp
public class MainMenuController : BaseController
{
    private MainMenuView _view;
    private MainMenuModel _menuModel;   // 注意：勿命名为 _model（遮蔽基类字段，触发 CS0108 警告）

    public override void Init()
    {
        _menuModel = new MainMenuModel();
        SetModel(_menuModel);
        _menuModel.Init();
        // 可选：订阅全局事件 GameEvents.Listen(...)
    }

    public override void OnLoadView(IBaseView view)
    {
        _view = view as MainMenuView;
        if (_view != null)
        {
            _view.StartGameClicked += OnStartGame;  // 绑定 View 事件
            _view.Render(_menuModel);               // 首次渲染
        }
    }

    public override void Destroy()
    {
        base.Destroy();                              // 会调用 RemoveGlobalEvent()
        if (_view != null)
            _view.StartGameClicked -= OnStartGame;   // 解绑 View 事件
        // GameEvents.Unlisten(...)
    }

    private void OnStartGame()
    {
        // 业务逻辑...
        _view?.Render(_menuModel);                   // 数据变化 → 刷新 View
    }
}
```

### 步骤 5：创建 Entry（场景装配）

创建场景入口 `MonoBehaviour`，负责组装 Controller/View 并注册到 `MVCManager.ControllerManager`：

```csharp
public class MainMenuEntry : MonoBehaviour
{
    private MainMenuView _menuView;
    private MainMenuController _controller;

    private void Start()
    {
        _menuView = FindObjectOfType<MainMenuView>();   // 或序列化引用
        if (_menuView == null) { /* 报错返回 */ }

        _controller = new MainMenuController();
        _controller.Init();                             // 创建 Model
        _menuView.Controller = _controller;
        _controller.OnLoadView(_menuView);              // 绑定事件 + 渲染

        MVCManager.ControllerManager.Register(ControllerType.MainMenu, _controller);
    }

    private void OnDestroy()
    {
        if (_controller != null)
        {
            MVCManager.ControllerManager.Unregister((int)ControllerType.MainMenu);
            _controller.Destroy();
        }
    }
}
```

### 步骤 6：场景接线（关键！）

将 View/Entry 挂到场景中的对应 GameObject 上。**推荐方式：**

- **方式 A（运行时生成 Prefab）**：用 `ViewManager.Register/Open` 从 Resources 加载视图 Prefab（需配合 `ViewType_Template.cs` 枚举）。适合由游戏启动器统一管理的 UI。
- **方式 B（场景内直接挂载，本仓库常用）**：直接在场景里把 `MainMenuView` 挂到 Canvas、把 `MainMenuEntry` 挂到 Managers，靠序列化引用/`FindObjectOfType` 关联。适合主菜单、暂停菜单这类常驻界面。

接线映射建议：

| 内容 | 挂载位置 | 说明 |
|------|---------|------|
| `XXXView` | UI 根（Canvas 或面板根） | 提供 UI 组件查找的基准 |
| `XXXEntry` | 常驻管理器节点（Managers / PauseSystem） | 监听输入、装配 MVC |

### 步骤 7：移除旧脚本组件

将原 MonoBehaviour 脚本从场景中移除：

1. 从 GameObject 的 `m_Component` 列表删除该组件的 fileID
2. 删除对应的 `--- !u!114 &xxx` MonoBehaviour 块
3. 用脚本 GUID 全局搜索确认无残留引用（`.unity`/`.prefab`）

### 步骤 8：验证

| 检查项 | 说明 |
|--------|------|
| 编译 | `get_errors` + 控制台无 error/warning（注意 CS0108 字段遮蔽警告） |
| 场景加载 | 重载场景后确认新组件已挂载、旧组件已移除 |
| 事件绑定 | Play 模式点击按钮 → Controller 逻辑生效 |
| 订阅配对 | `Listen` 与 `Unlisten`、`+=` 与 `-=` 成对 |
| GUID 残留 | 旧脚本 GUID 在所有场景/预制体中 0 引用（Old/ 备份目录除外） |

## 修改现有 MVC UI 的交互逻辑

针对**已是 MVC 结构**的界面（如主菜单 `MainMenuView/Controller`、暂停菜单 `PauseMenuView/Controller`），按"改什么 → 改哪里"定位：

### 修改链路速查

| 需求 | 修改位置 |
|------|---------|
| 按钮点击后做什么（改功能） | Controller 的 `OnXXX()` 处理方法 |
| 按钮点击后刷新显示 | Controller 调用 `_view?.Render(_model)` + View 的 `Render()` |
| 新增一个按钮 | View（绑定+事件）→ Controller（订阅+处理）→ 场景/预制体（加按钮） |
| 显示的数据变化 | Model 增加字段 + `Init()` 默认值 |
| 需要通知其他模块 | Controller 里 `GameEvents.Publish(...)`（见 unity-event-setup skill） |
| 需要向另一个 Controller 发消息 | `MVCManager.ControllerManager.ApplyFunc(ControllerType.XXX, "funcName", args)` |

### 场景 A：改已有按钮行为（不改 UI 结构）

只需改 **Controller**，不动 View/Model（除非数据或显示变化）：

```csharp
// 修改 MainMenuController 中的处理方法
private void OnStartGame()
{
    // 1. 更新数据 → _model 字段
    // 2. 处理逻辑（读存档、切场景等）
    // 3. 需要时刷新 View
    _view?.Render(_menuModel);
}
```

若需要携带新数据，在 `Model` 加字段并在 `Init()` 给默认值；需要显示新内容，改 `View.Render()`。

### 场景 B：新增按钮

按 **View → Controller → 场景** 顺序改动：

```csharp
// 1. View：OnAwake() 中绑定新按钮 + 定义事件
public event Action NewBtnClicked;          // 新事件
private Button _newBtn;
protected override void OnAwake()
{
    // ...existing code...
    _newBtn = Find<Button>("NewBtnName");
    _newBtn?.onClick.AddListener(() => NewBtnClicked?.Invoke());
}
```

```csharp
// 2. Controller：OnLoadView 订阅、Destroy 解绑、新增处理方法
public override void OnLoadView(IBaseView view)
{
    // ...existing code...
    _view.NewBtnClicked += OnNewBtn;
}
public override void Destroy()
{
    base.Destroy();
    // ...existing code...
    _view.NewBtnClicked -= OnNewBtn;
}
private void OnNewBtn()
{
    // 新按钮逻辑
}
```

```csharp
// 3. 场景/预制体：复制现有按钮生成新按钮，重命名；或直接在 View 所在根下按路径加按钮
```

### 场景 C：修改事件订阅/发布

- 需要新事件 → 在 [EventTypes_Template.cs](../../../Assets/Scripts/Events/EventTypes_Template.cs) 的 `EventType` 枚举添加值（命名 `模块_EVENT_ON_动作`）
- Controller `Init()` 中 `GameEvents.Listen(...)`，`Destroy()` 中成对 `Unlisten(...)`

### 场景 D：修改渲染逻辑

只改 View 的 `Render()` / 更新方法，保持"Controller 提供数据、View 负责显示"的职责边界，**不要把业务逻辑写进 View**。

### 修改后验证

| 检查项 | 说明 |
|--------|------|
| 编译 | `get_errors` + 控制台无 error/warning |
| 事件配对 | 新加的事件在 `OnLoadView` 订阅、`Destroy` 解绑 |
| 场景接线 | 新增按钮的路径与 `Find<>` 路径一致 |
| 功能测试 | Play 模式点击新/旧按钮，确认行为符合预期 |

## 常见陷阱（务必注意）

### 陷阱 1：未激活对象的 Awake 不执行

`BaseView.OnAwake()`（绑定按钮/组件）只在 GameObject **激活时**执行。若视图默认 `m_IsActive: 0`（如暂停菜单），`FindObjectOfType` **找不到**它，且组件绑定要等首次激活才发生。

**解决**：Entry 用 `[SerializeField]` 序列化引用（直接拖引用，如 `menuView: {fileID: ...}`），或用 `Resources.FindObjectsOfTypeAll<T>()` 兜底：

```csharp
if (_menuView == null) _menuView = FindObjectOfType<PauseMenuView>();
if (_menuView == null)
{
    var all = Resources.FindObjectsOfTypeAll<PauseMenuView>();
    if (all != null && all.Length > 0) _menuView = all[0];
}
```

### 陷阱 2：派生 Controller 不要声明 `_model` 字段

`BaseController` 已有 `protected BaseModel _model`，子类再声明同名字段会触发 **CS0108 警告**。改用 `_menuModel` / `_pauseModel` 等语义化命名。

### 陷阱 3：编辑器内存状态 vs 磁盘文件不一致

- MCP 工具改场景只改**内存**，不立即落盘；直接编辑 `.unity` 文件只改**磁盘**。
- 若两处不一致，**先重载场景**（重新打开 .unity）再保存，否则保存会用内存状态覆盖磁盘修改。
- 改完磁盘文件后，编辑器需重载场景才会生效。

### 陷阱 4：BaseView.Find<> 是 transform.Find

`Find("A/B")` 用的是 `transform.Find` 相对路径，**不是**全局路径。路径写错会在控制台打 `[BaseView.Find] 未找到路径` 错误，务必核对场景层级。

### 陷阱 5：UI 文字组件类型

按钮标签可能是 `TextMeshProUGUI`（TMPro）而不是 `UnityEngine.UI.Text`。用 `GetComponentInChildren<TextMeshProUGUI>()` 需要 `using TMPro;`。先查场景 YAML 中文字组件 GUID（`f4688fdb7df04437aeb418b961361dc5` 是 TMP 组件）确认类型。

## 常见问题

| 问题 | 原因 | 解决 |
|------|------|------|
| 点击按钮无反应 | View 事件未绑定到 Controller | 检查 `OnLoadView` 中是否 `+=` 订阅；确认 View 激活时 `OnAwake` 已绑定 Button |
| `Find` 报未找到 | 路径与场景层级不符 | 核对层级，路径相对视图根 |
| CS0108 警告 | 子类字段遮蔽 `_model` | 改字段名 |
| 暂停菜单找不到 View | 默认未激活，`FindObjectOfType` 失效 | 序列化引用或 `FindObjectsOfTypeAll` |
| 场景保存后修改丢失 | 编辑器内存覆盖磁盘 | 先重载场景再保存 |
