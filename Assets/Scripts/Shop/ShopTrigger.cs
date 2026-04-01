using UnityEngine;
using UnityEngine.InputSystem;

public class ShopTrigger : MonoBehaviour
{
    [Header("Shop Settings")]
    public string cropName = "Graan";  // Naam van het zaad (voor de log)
    public int price = 10;
    public GameObject visualIndicator; // De cirkel/icoon die verschijnt

    [Header("Spawn Settings")]
    public GameObject seedPrefab;      // De 3D prefab van het zaadje/zakje
    public Transform spawnLocation;    // Waar het zaadje moet verschijnen (Empty GameObject)

    [Header("Input Action")]
    public InputActionReference interactAction;

    private bool playerInRange = false;

    private void OnEnable() => interactAction.action.Enable();
    private void OnDisable() => interactAction.action.Disable();

    void Start()
    {
        if (visualIndicator != null) visualIndicator.SetActive(false);
    }

    void Update()
    {
        // Check of de speler in de zone staat en de knop indrukt
        if (playerInRange && interactAction.action.WasPressedThisFrame())
        {
            BuyItem();
        }
    }

    void BuyItem()
    {
        // 1. Check of er genoeg goud is in de ShopManager
        if (ShopManager.Instance != null && ShopManager.Instance.TryBuyItem(price))
        {
            // 2. Spawn het fysieke GameObject
            SpawnSeed();
            Debug.Log($"Gekocht: {cropName} voor {price} goud!");
        }
        else
        {
            Debug.Log("Niet genoeg goud!");
        }
    }

    void SpawnSeed()
    {
        if (seedPrefab != null && spawnLocation != null)
        {
            // Deze lijn laat in de console zien WELKE prefab er op WELKE trigger wordt gespawnd
            Debug.Log("Spawning " + seedPrefab.name + " op trigger: " + gameObject.name);

            GameObject newSeed = Instantiate(seedPrefab, spawnLocation.position, spawnLocation.rotation);
            // ... rest van je code
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (visualIndicator != null) visualIndicator.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (visualIndicator != null) visualIndicator.SetActive(false);
        }
    }
}