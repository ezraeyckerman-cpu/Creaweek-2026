using UnityEngine;
using System.Collections;
using TMPro;

public class BoardUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text _amountText;

    [Header("Images")]
    [SerializeField] private GameObject _cornImage;
    [SerializeField] private GameObject _potatoImage;
    [SerializeField] private GameObject _pumpkinImage;
    [SerializeField] private GameObject _carrotImage;

    [Header("Done")]
    [SerializeField] private GameObject _doneImage;

    private BoatManager boatManager;
    private bool hasShownDone = false;

    void Update()
    {
        if (boatManager == null || !boatManager.gameObject.activeInHierarchy)
        {
            FindBoat();
            hasShownDone = false; // reset for new boat
        }

        if (boatManager == null) return;

        UpdateUI();
    }

    void FindBoat()
    {
        GameObject boat = GameObject.FindGameObjectWithTag("Boat");
        if (boat != null && boat.activeInHierarchy)
        {
            boatManager = boat.GetComponent<BoatManager>();
        }
    }

    void UpdateUI()
    {
        int filled = boatManager.currentFilledSlots;
        int total = boatManager.totalSlots;

        _amountText.text = $"{total - filled}";

        // ✅ When boat is full
        if (filled >= total)
        {
            if (!hasShownDone)
            {
                hasShownDone = true;
                StartCoroutine(ShowDone());
            }
            return; // stop normal UI updates
        }

        // Normal state → hide done image
        _doneImage.SetActive(false);

        // Disable all crop images first
        _cornImage.SetActive(false);
        _potatoImage.SetActive(false);
        _pumpkinImage.SetActive(false);
        _carrotImage.SetActive(false);

        // Enable correct crop
        switch (boatManager.currentRequiredCrop)
        {
            case "Corn":
                _cornImage.SetActive(true);
                break;
            case "Potato":
                _potatoImage.SetActive(true);
                break;
            case "Pumpkin":
                _pumpkinImage.SetActive(true);
                break;
            case "Carrot":
                _carrotImage.SetActive(true);
                break;
        }
    }

    IEnumerator ShowDone()
    {
        _cornImage.SetActive(false);
        _potatoImage.SetActive(false);
        _pumpkinImage.SetActive(false);
        _carrotImage.SetActive(false);

        _doneImage.SetActive(true);

        yield return new WaitForSeconds(2f);

        _doneImage.SetActive(false);
    }
}