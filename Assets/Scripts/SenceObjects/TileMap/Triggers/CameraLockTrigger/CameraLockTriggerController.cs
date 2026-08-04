using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 摄像机锁定触发控制器 —— 角色进入时锁定摄像机，离开时恢复跟随。
/// </summary>
public class CameraLockTriggerController : MonoBehaviour
{
    private static readonly string PoolKey = "CameraLockTrigger";
    private static readonly string CameraPoolKey = "SealCameraFollow";

    [FoldoutGroup("摄像机锁定数据", expanded: true)]
    [SerializeField, LabelText("锁定配置")]
    private CameraLockData _lockData = new CameraLockData();

    public CameraLockData LockData => _lockData;

    private void OnEnable()
    {
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
        InformationPool.Get<SealCameraFollow>(CameraPoolKey)?.ApplyLock(_lockData);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Seal>() == null) return;
        InformationPool.Get<SealCameraFollow>(CameraPoolKey)?.ReleaseLock();
    }
}
