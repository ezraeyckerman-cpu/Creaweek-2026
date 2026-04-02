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
    public GameObject[] boats; // Index 0 = Gold Boat, Index 1 = Score Boat

    [Header("Movement Points")]
    public Transform startPoint;
    public Transform dockPoint;
    public Transform exitPoint;
    public float speed = 5f;

    [Header("Scalable Cargo Visuals")]
    public List<CropVisuals> allCropVisuals;

    [Header("Rewards")]
    public int goldPerItem = 15;
    public int scorePerItem = 10; 

    [Header("UI References")]
    public TextMeshPro boatTimerText;
    public ScoreUI scoreUIScript; 

    [Header("Timer Settings")]
    public float currentRespawnTime = 60f;
    private float minRespawnTime = 30f;
    private float timeReduction = 5f;

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

    private float GetAdjustedTimePerItem()
    {
        if (PlayersInZone <= 1) return timePerItem;
        return timePerItem / PlayersInZone;
    }

    IEnumerator BoatRoutine()
    {
        while (true)
        {
            // 1. WACHTEN & TIMER
            currentState = BoatState.Gone;
            transform.position = new Vector3(0, -100, 0);

            if (boatTimerText != null) boatTimerText.gameObject.SetActive(true);

            float timer = currentRespawnTime;
            while (timer > 0)
            {
                if (boatTimerText != null)
                {
                    string typeLabel = isSellBoat ? "GOUD BOOT" : "SCORE BOOT";
                    boatTimerText.text = $"{typeLabel}\n{Mathf.Ceil(timer)}s";
                }
                timer -= Time.deltaTime;
                yield return null;
            }

            if (boatTimerText != null) boatTimerText.gameObject.SetActive(false);
            if (possibleCrops.Count == 0) yield break;

            if (TileManager.Instance != null)
                TileManager.Instance.GenerateBombs();

            currentRequiredCrop = possibleCrops[Random.Range(0, possibleCrops.Count)];
            currentFilledSlots = 0;
            currentCrate = 0;

            
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }

            // 2. VAAR NAAR DOK
            transform.position = startPoint.position;
            currentState = BoatState.Coming;
            while (Vector3.Distance(transform.position, dockPoint.position) > 0.5f)
            {
                MoveBoat(dockPoint.position);
                yield return null;
            }

            // 3. VULLEN
            currentState = BoatState.Waiting;
            if (sfxSource != null && sfxClip != null) sfxSource.PlayOneShot(sfxClip);

            while (currentFilledSlots < totalSlots)
            {
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

                        yield return new WaitForSeconds(GetAdjustedTimePerItem());
                    }
                }
                yield return null;
            }

            // 4. VERTREK
            yield return new WaitForSeconds(1f);
            currentState = BoatState.Leaving;

            while (Vector3.Distance(transform.position, exitPoint.position) > 0.5f)
            {
                MoveBoat(exitPoint.position);
                yield return null;
            }

            
            currentRespawnTime = Mathf.Max(minRespawnTime, currentRespawnTime - timeReduction);

            
            isSellBoat = !isSellBoat;
            if (boats.Length >= 2)
            {
                boats[0].SetActive(isSellBoat);
                boats[1].SetActive(!isSellBoat);
            }
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
            {
                scoreUIScript.AddScore(scorePerItem);
            }
            else
            {
                Debug.LogWarning("BoatManager: Geen ScoreUI script gevonden in de Inspector!");
            }
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
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 2f);
    }
}