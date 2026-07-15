using UnityEngine;

public class WatterController : MonoBehaviour
{
    private void OnEnable()
    {
        InformationPool.Set("Watter", this);
    }

    private void OnDisable()
    {
        InformationPool.Remove("Watter");
    }
}
