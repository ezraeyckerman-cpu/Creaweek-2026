using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class BoatManager : MonoBehaviour
{
    [System.Serializable]
    public class CropVisuals
    {
        public string cropName;
        public GameObject cropPrefab;
    }

    [Header("Boat Visuals")]
    public GameObject[] boats; // Index 0 = Gold, Index 1 = Score

    [Header("Movement Points")]
    public Transform startPoint;
    public Transform dockPoint;
    public Transform exitPoint;
    public float speed = 5f;

    [Header("Scalable Cargo Visuals")]
    public List<CropVisuals> allCropVisuals;

    [Header("Rewards & Penalties")]
    public int goldPerItem = 15;
    public int scorePerItem = 10;
    public int scorePenalty = 10;

    [Header("UI References")]
    public TextMeshPro boatTimerText;
    public ScoreUI scoreUIScript;

    [Header("Timing Settings")]
    public float currentRespawnTime = 15f;
    public float timeAtDock = 6f;
    private float minRespawnTime = 15f;
    private float timeReduction = 0f;

    [Header("Player & Delivery Settings")]
    public Transform playerBackpack;
    public int totalSlots = 6;
    public float timePerItem = 0.8f;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip sfxClip;

    private Dictionary<string, GameObject> cropPrefabDict = new Dictionary<string, GameObject>();
    private List<string> possibleCrops = new List<string>();

    [HideInInspector] public int PlayersInZone = 0;
    private bool playerInZone = false;
    public string currentRequiredCrop;
    public int currentFilledSlots = 0;

    private bool isSellBoat = true;
    private int currentCrate = 0;

    private enum BoatState { Coming, Waiting, Leaving, Gone }
    [SerializeField] private BoatState currentState = BoatState.Gone;

    private float _amountPerCrate;

    void Start()
    {
        InitializeCropDictionary();
        _amountPerCrate = (float)totalSlots / 6f;

        // Startpositie instellen en zorgen dat hij onzichtbaar begint
        transform.position = startPoint.position;
        UpdateBoatVisibility(false);

        StartCoroutine(BoatRoutine());
    }

    void InitializeCropDictionary()
    {
        cropPrefabDict.Clear();
        possibleCrops.Clear();
        foreach (CropVisuals visual in allCropVisuals)
        {
            if (!cropPrefabDict.ContainsKey(visual.cropName))
            {
                cropPrefabDict.Add(visual.cropName, visual.cropPrefab);
                possibleCrops.Add(visual.cropName);
            }
        }
    }

    public void AddPlayerToZone() { PlayersInZone++; playerInZone = true; }
    public void RemovePlayerFromZone() { PlayersInZone = Mathf.Max(0, PlayersInZone - 1); playerInZone = PlayersInZone > 0; }

    IEnumerator BoatRoutine()
    {
        while (true)
        {
            // --- STATE: GONE ---
            currentState = BoatState.Gone;
            UpdateBoatVisibility(false); // Zorg dat de boot niet zichtbaar is tijdens het wachten
            transform.position = startPoint.position;

            if (boatTimerText != null) boatTimerText.gameObject.SetActive(true);

            float respawnTimer = currentRespawnTime;
            while (respawnTimer > 0)
            {
                boatTimerText.text = $"{Mathf.Ceil(respawnTimer)}";
                respawnTimer -= Time.deltaTime;
                yield return null;
            }

            if (possibleCrops.Count == 0) yield break;

            // Voorbereiden voor vertrek
            currentRequiredCrop = possibleCrops[Random.Range(0, possibleCrops.Count)];
            currentFilledSlots = 0;
            currentCrate = 0;

            // Reset kratten (kinderen van dit object)
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(false);

            if (TileManager.Instance != null)
                TileManager.Instance.GenerateBombs();

            // --- STATE: COMING ---
            currentState = BoatState.Coming;
            UpdateBoatVisibility(true); // Boot wordt nu pas zichtbaar!
            if (boatTimerText != null) boatTimerText.gameObject.SetActive(false);

            while (Vector3.Distance(transform.position, dockPoint.position) > 0.1f)
            {
                MoveBoat(dockPoint.position);
                yield return null;
            }

            // --- STATE: WAITING ---
            currentState = BoatState.Waiting;
            if (boatTimerText != null) boatTimerText.gameObject.SetActive(true);
            if (sfxSource != null && sfxClip != null) sfxSource.PlayOneShot(sfxClip);

            float dockTimer = timeAtDock;
            while (dockTimer > 0 && currentFilledSlots < totalSlots)
            {
                if (boatTimerText != null)
                    boatTimerText.text = $"{Mathf.Ceil(dockTimer)}"; // Toon resterende dock tijd

                if (playerInZone && CropManager.Instance != null)
                {
                    if (CropManager.Instance.TryRemoveHarvestedCrop(currentRequiredCrop, 1))
                    {
                        SpawnFlyingItem();
                        GiveReward();
                        currentFilledSlots++;

                        while (currentFilledSlots >= _amountPerCrate * (currentCrate + 1) && currentCrate < 6)
                        {
                            if (currentCrate < transform.childCount)
                                transform.GetChild(currentCrate).gameObject.SetActive(true);
                            currentCrate++;
                        }
                        yield return new WaitForSeconds(timePerItem / Mathf.Max(1, PlayersInZone));
                    }
                }
                dockTimer -= Time.deltaTime;
                yield return null;
            }

            // Strafpunten check
            if (currentFilledSlots < totalSlots && scoreUIScript != null)
                scoreUIScript.AddScore(-scorePenalty);

            if (boatTimerText != null) boatTimerText.gameObject.SetActive(false);

            // --- STATE: LEAVING ---
            yield return new WaitForSeconds(0.5f);
            currentState = BoatState.Leaving;

            while (Vector3.Distance(transform.position, exitPoint.position) > 0.1f)
            {
                MoveBoat(exitPoint.position);
                yield return null;
            }

            // --- DESPAWN & RESET ---
            UpdateBoatVisibility(false); // Boot is weg, dus zet visuals uit

            // Type wisselen voor de volgende keer
            isSellBoat = !isSellBoat;
            currentRespawnTime = Mathf.Max(minRespawnTime, currentRespawnTime - timeReduction);

            // Onzichtbaar terug naar start voor de volgende timer loop
            transform.position = startPoint.position;
        }
    }

    // Handige functie om de juiste boot aan of uit te zetten
    void UpdateBoatVisibility(bool visible)
    {
        if (boats.Length >= 2)
        {
            // Als visible true is, check dan ook welk type (goud/score) actief moet zijn
            boats[0].SetActive(visible && isSellBoat);
            boats[1].SetActive(visible && !isSellBoat);
        }
    }

    void GiveReward()
    {
        if (isSellBoat)
        {
            if (ShopManager.Instance != null)
                ShopManager.Instance.currentGold += goldPerItem;
        }
        else
        {
            if (scoreUIScript != null)
                scoreUIScript.AddScore(scorePerItem);
        }
    }

    void SpawnFlyingItem()
    {
        if (cropPrefabDict.TryGetValue(currentRequiredCrop, out GameObject prefabToSpawn))
        {
            GameObject item = Instantiate(prefabToSpawn, playerBackpack.position, Quaternion.identity);
            FlyingItem flyer = item.AddComponent<FlyingItem>();
            flyer.StartFlight(playerBackpack, transform, 0.6f);
        }
    }

    void MoveBoat(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        Vector3 dir = (target - transform.position).normalized;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
    }
}