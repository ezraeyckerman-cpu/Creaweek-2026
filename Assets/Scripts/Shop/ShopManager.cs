using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("Economy")]
    public int currentGold = 100;
    public TextMeshProUGUI goldText; // Optioneel: sleep hier een UI tekst in

    void Awake()
    {
        if (Instance == null) Instance = this;
        UpdateGoldUI();
    }

    public bool TryBuyItem(int price)
    {
        if (currentGold >= price)
        {
            currentGold -= price;
            UpdateGoldUI();
            Debug.Log("Item gekocht! Goud over: " + currentGold);
            return true;
        }

        Debug.Log("Niet genoeg goud!");
        return false;
    }

    public void UpdateGoldUI()
    {
        if (goldText != null) goldText.text = $"{currentGold}";
    }
}