using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 临时氧气滑条控制器 —— 从 SealModel 读取氧气值并更新 Slider
/// </summary>
public class OxygenSliderController : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Text labelText;

    private SealModel sealModel;
    private float searchTimer;

    private void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (labelText == null)
            labelText = GetComponentInChildren<Text>();

        FindSealModel();
    }

    private void Update()
    {
        // 定期重新搜索 SealModel（角色可能在运行时生成）
        if (sealModel == null)
        {
            searchTimer += Time.deltaTime;
            if (searchTimer > 1f)
            {
                searchTimer = 0f;
                FindSealModel();
            }
        }

        if (slider == null) return;

        float currentOxygen = 0f;
        float maxOxygen = 100f;

        if (sealModel != null)
        {
            currentOxygen = sealModel.OxygenValue;
            maxOxygen = sealModel.OxygenMaxValue;
        }
        else if (InformationPool.TryGet("Oxygen", out int oxy))
        {
            currentOxygen = oxy;
            maxOxygen = InformationPool.Get("OxygenMax", 100);
        }

        slider.maxValue = maxOxygen;
        slider.value = currentOxygen;

        if (labelText != null)
        {
            labelText.text = string.Format("O2: {0}/{1}", Mathf.FloorToInt(currentOxygen), Mathf.FloorToInt(maxOxygen));
        }
    }

    private void FindSealModel()
    {
        var seal = Object.FindObjectOfType<Seal>();
        if (seal != null)
            sealModel = seal.GetComponent<SealModel>();
    }
}
