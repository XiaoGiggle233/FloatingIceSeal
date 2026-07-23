using UnityEngine;

public class WallController : MonoBehaviour
{
    private void OnEnable()
    {
        InformationPool.Set("Wall", this);
    }

    private void OnDisable()
    {
        InformationPool.Remove("Wall");
    }
}