using UnityEngine;

public class SpikeController : MonoBehaviour
{
    private void OnEnable()
    {
        InformationPool.Set("Spike", this);
    }

    private void OnDisable()
    {
        InformationPool.Remove("Spike");
    }
}
