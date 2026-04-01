using TMPro;
using UnityEngine;

public class CropUIElement : MonoBehaviour
{
    public string CropName;
    public TextMeshProUGUI AmountText;

    public void UpdateAmount(int amount)
    {
        AmountText.text = amount.ToString();
    }
}