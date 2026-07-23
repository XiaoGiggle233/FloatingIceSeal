using UnityEngine;

public class SeagrassController : MonoBehaviour
{
    private void OnEnable()
    {
        InformationPool.Set("Seagrass", this);
    }

    private void OnDisable()
    {
        InformationPool.Remove("Seagrass");
    }
}
