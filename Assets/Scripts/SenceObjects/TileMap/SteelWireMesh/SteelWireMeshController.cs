using UnityEngine;

public class SteelWireMeshController : MonoBehaviour
{
    private void OnEnable()
    {
        InformationPool.Set("SteelWireMesh", this);
    }

    private void OnDisable()
    {
        InformationPool.Remove("SteelWireMesh");
    }
}
