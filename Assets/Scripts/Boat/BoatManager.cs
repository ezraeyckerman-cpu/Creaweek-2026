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

        transform.position = startPoint.position;

        // Zorg dat alles onzichtbaar start
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
            UpdateBoatVisibility(false);
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

            currentRequiredCrop = possibleCrops[Random.Range(0, possibleCrops.Count)];
            currentFilledSlots = 0;
            currentCrate = 0;
            ResetCrates();

            if (TileManager.Instance != null)
                TileManager.Instance.GenerateBombs();

            // --- STATE: COMING ---
            currentState = BoatState.Coming;
            UpdateBoatVisibility(true);
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
                boatTimerText.text = $"{Mathf.Ceil(dockTimer)}";

                if (playerInZone && CropManager.Instance != null)
                {
                    if (CropManager.Instance.TryRemoveHarvestedCrop(currentRequiredCrop, 1))
                    {
                        SpawnFlyingItem();
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

            // --- BELONING OF STRAFPUNT ---
            if (currentFilledSlots >= totalSlots)
            {
                GiveFinalReward();
            }
            else
            {
                if (scoreUIScript != null)
                {
                    scoreUIScript.AddScore(-scorePenalty);
                    // Forceer UI update voor score indien nodig:
                    // scoreUIScript.UpdateUI();
                }
            }

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
            UpdateBoatVisibility(false);
            isSellBoat = !isSellBoat;
            currentRespawnTime = Mathf.Max(minRespawnTime, currentRespawnTime - timeReduction);
            transform.position = startPoint.position;
        }
    }

    void GiveFinalReward()
    {
        if (isSellBoat)
        {
            if (ShopManager.Instance != null)
            {
                int totalGold = currentFilledSlots * goldPerItem;
                ShopManager.Instance.currentGold += totalGold;

                // CRUCIAAL: Roep de functie aan die de UI ververst!
                // Als jouw ShopManager een andere naam gebruikt, verander dit dan hieronder:
                ShopManager.Instance.UpdateGoldUI();

                Debug.Log($"<color=yellow>GOUD TOEGEVOEGD:</color> {totalGold}. Totaal: {ShopManager.Instance.currentGold}");
            }
        }
        else
        {
            if (scoreUIScript != null)
            {
                int totalScore = currentFilledSlots * scorePerItem;
                scoreUIScript.AddScore(totalScore);

                // De meeste ScoreUI scripts doen dit in AddScore, maar voor de zekerheid:
                // scoreUIScript.UpdateScoreText(); 

                Debug.Log($"<color=green>SCORE TOEGEVOEGD:</color> {totalScore}");
            }
        }
    }

    void UpdateBoatVisibility(bool visible)
    {
        // Zet alle kinderen (rook, effecten, boten) aan of uit
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(visible);
        }

        if (visible && boats.Length >= 2)
        {
            boats[0].SetActive(isSellBoat);
            boats[1].SetActive(!isSellBoat);
            ResetCrates();
        }

        // De timer tekst moet zichtbaar blijven tijdens het wachten
        if (boatTimerText != null && currentState == BoatState.Gone)
            boatTimerText.gameObject.SetActive(true);
    }

    void ResetCrates()
    {
        // Zet de kratten (meestal de eerste kinderen) uit
        for (int i = 0; i < 6 && i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
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