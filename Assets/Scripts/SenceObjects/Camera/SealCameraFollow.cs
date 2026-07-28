using UnityEngine;

/// <summary>
/// 摄像机平滑跟随 Seal 移动。
/// 优先从 InformationPool 获取 Seal 引用，也支持直接拖拽赋值。
/// </summary>
public class SealCameraFollow : MonoBehaviour
{
    [Header("目标")]
    [SerializeField] private Seal _target;

    [Header("跟随参数")]
    [SerializeField] private Vector3 _offset = new Vector3(0, 0, -10);
    [SerializeField] private float _smoothTime = 0.15f;
    [SerializeField] private float _lookAheadFactor = 0.3f;
    [SerializeField] private float _lookAheadMax = 2f;

    [Header("边界限制（可选, 0 表示不限制）")]
    [SerializeField] private Vector2 _minBounds;
    [SerializeField] private Vector2 _maxBounds;

    private Vector3 _velocity = Vector3.zero;
    private Rigidbody2D _targetRb;

    private void Start()
    {
        if (_target == null)
            _target = InformationPool.Get<Seal>("Seal");

        if (_target != null)
            _targetRb = _target.GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            _target = InformationPool.Get<Seal>("Seal");
            if (_target != null)
                _targetRb = _target.GetComponent<Rigidbody2D>();
            return;
        }

        Vector3 targetPos = _target.transform.position + _offset;

        // 基于速度的预瞄偏移
        if (_targetRb != null)
        {
            Vector2 lookAhead = _targetRb.velocity * _lookAheadFactor;
            lookAhead = Vector2.ClampMagnitude(lookAhead, _lookAheadMax);
            targetPos += (Vector3)lookAhead;
        }

        // 边界限制
        if (_maxBounds != _minBounds)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, _minBounds.x, _maxBounds.x);
            targetPos.y = Mathf.Clamp(targetPos.y, _minBounds.y, _maxBounds.y);
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, _smoothTime);
    }

    /// <summary>
    /// 手动设置跟随目标
    /// </summary>
    public void SetTarget(Seal seal)
    {
        _target = seal;
        _targetRb = seal != null ? seal.GetComponent<Rigidbody2D>() : null;
    }
}
