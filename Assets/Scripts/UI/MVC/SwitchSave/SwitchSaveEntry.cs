using System;
using UnityEngine;

#region ========== 切换存档入口 ==========

/// <summary>
/// 切换存档入口 —— 场景启动时创建并绑定切换存档的 MVC 组件
/// 挂载到 StartSenece 场景的常驻 GameObject 上
/// </summary>
public class SwitchSaveEntry : MonoBehaviour
{
    [SerializeField] private SwitchSaveView saveView;

    private SwitchSaveView _view;
    private SwitchSaveController _controller;

    private void Start()
    {
        _view = saveView;
        if (_view == null)
        {
            // 面板默认未激活，FindObjectOfType 查不到，改用包含未激活对象的查找
            var all = Resources.FindObjectsOfTypeAll<SwitchSaveView>();
            if (all != null && all.Length > 0)
                _view = all[0];
        }

        if (_view == null)
        {
            Debug.LogError("[SwitchSaveEntry] 未找到 SwitchSaveView，切换存档 MVC 初始化失败");
            return;
        }

        _controller = new SwitchSaveController();
        _controller.Init();
        _view.Controller = _controller;
        _controller.OnLoadView(_view);

        MVCManager.ControllerManager.Register(ControllerType.SwitchSave, _controller);

        // 初始隐藏面板
        _view.SetMenuVisible(false);
    }

    private void OnDestroy()
    {
        if (_controller != null)
        {
            MVCManager.ControllerManager.Unregister(Convert.ToInt32(ControllerType.SwitchSave));
            _controller.Destroy();
        }
    }
}

#endregion
