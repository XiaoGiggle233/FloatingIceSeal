using UnityEngine;

public class SpawnPointController : MonoBehaviour
{
    private void OnEnable()
    {
        InformationPool.Set("SpawnPoint", this);
    }

    private void OnDisable()
    {
        InformationPool.Remove("SpawnPoint");
    }
}
