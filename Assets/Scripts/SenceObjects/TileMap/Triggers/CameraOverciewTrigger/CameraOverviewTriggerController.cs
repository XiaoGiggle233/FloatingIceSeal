using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 摄像机全览触发控制器 —— 角色进入时执行全览指令，离开时恢复跟随。
/// </summary>
public class CameraOverviewTriggerController : MonoBehaviour
{
    private static readonly string PoolKey = "CameraOverviewTrigger";
    private static readonly string CameraPoolKey = "SealCameraFollow";

    [FoldoutGroup("摄像机全览数据", expanded: true)]
    [SerializeField, LabelText("全览配置")]
    private CameraOverviewData _overviewData = new CameraOverviewData();

    public CameraOverviewData OverviewData => _overviewData;

    private void OnEnable()
    {
        // 注册到信息池，供其他系统读取
        InformationPool.Set(PoolKey, this);
    }

    private void OnDisable()
    {
        // 只有当前是 PoolKey 指向自己时才移除，避免误删其他触发器的引用
        if (InformationPool.TryGet(PoolKey, out object obj) && ReferenceEquals(obj, this))
            InformationPool.Remove(PoolKey);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Seal>() == null) return;
        InformationPool.Get<SealCameraFollow>(CameraPoolKey)?.ApplyOverview(_overviewData);
    }
}
