using System.Collections.Generic;
using UnityEngine;

public class CropsUIManager : MonoBehaviour
{
    // Sleep hier al je UI-elementen in via de Inspector
    [SerializeField] private List<CropUIElement> _uiElements;

    private void Start()
    {
        CropManager.Instance.CropHarvested += UpdateUI;
    }

    private void UpdateUI(string cropName, int cropAmount)
    {
        // Zoek in de lijst naar het element met de juiste naam
        var element = _uiElements.Find(x => x.CropName.ToLower() == cropName.ToLower());

        if (element != null)
        {
            element.UpdateAmount(cropAmount);
        }
    }
}