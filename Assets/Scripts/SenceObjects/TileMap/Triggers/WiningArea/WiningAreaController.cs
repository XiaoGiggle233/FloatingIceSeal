using UnityEngine;

public class WiningAreaController : MonoBehaviour
{
    private void OnEnable()
    {
        InformationPool.Set("WiningArea", this);
    }

    private void OnDisable()
    {
        InformationPool.Remove("WiningArea");
    }
}
