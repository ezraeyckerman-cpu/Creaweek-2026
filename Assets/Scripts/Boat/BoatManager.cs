using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoatManager : MonoBehaviour
{
    [System.Serializable]
    public class CropVisuals
    {
        public string cropName;
        public GameObject cropPrefab;
    }

    public GameObject[] boats;

    [Header("Movement")]
    public Transform startPoint;
    public Transform dockPoint;
    public Transform exitPoint;
    public float speed = 5f;

    [Header("Scalable Cargo Visuals")]
    public List<CropVisuals> allCropVisuals;

    [Header("Rewards")]
    public int goldPerItem = 15;
    public int scorePerItem = 100;

    private Dictionary<string, GameObject> cropPrefabDict = new Dictionary<string, GameObject>();
    private List<string> possibleCrops = new List<string>();

    [Header("Settings")]
    public Transform playerBackpack;
    public int totalSlots = 6;
    public float timePerItem = 0.8f;
    public float boatRespawnTime = 10f;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip sfxClip;

    [HideInInspector] public int PlayersInZone = 0;
    private bool playerInZone = false;
    public string currentRequiredCrop;
    public int currentFilledSlots = 0;

    // Boot Type: Wisselt tussen Goud en Score
    private bool isSellBoat = true;
    private int currentCrate = 0;

    private enum BoatState { Coming, Waiting, Leaving, Gone }
    [SerializeField] private BoatState currentState = BoatState.Gone;

    private float _amountPerCrate;

    void Start()
    {
        InitializeCropDictionary();
        StartCoroutine(BoatRoutine());
        _amountPerCrate = totalSlots / 6;
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

    public void AddPlayerToZone()
    {
        PlayersInZone++;
        playerInZone = true;
    }

    public void RemovePlayerFromZone()
    {
        PlayersInZone = Mathf.Max(0, PlayersInZone - 1);
        playerInZone = PlayersInZone > 0;
    }

    private float GetAdjustedTimePerItem()
    {
        if (PlayersInZone <= 1) return timePerItem;
        return timePerItem / PlayersInZone;
    }

    IEnumerator BoatRoutine()
    {
        while (true)
        {
            // 1. WACHTEN & RESET
            currentState = BoatState.Gone;
            transform.position = new Vector3(0, -100, 0);
            yield return new WaitForSeconds(boatRespawnTime);

            if (possibleCrops.Count == 0) yield break;

            // LANDMINES GENEREREN (Via jouw TileManager)
            if (TileManager.Instance != null)
                TileManager.Instance.GenerateBombs();

            currentRequiredCrop = possibleCrops[Random.Range(0, possibleCrops.Count)];
            currentFilledSlots = 0;

            string typeLabel = isSellBoat ? "GOLD BOAT" : "SCORE BOAT";
            Debug.Log($"NIEUWE BOOT: {typeLabel}. Wil {totalSlots}x {currentRequiredCrop}!");

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
            sfxSource.PlayOneShot(sfxClip);
            while (currentFilledSlots < totalSlots)
            {
                if (playerInZone && CropManager.Instance != null)
                {
                    if (CropManager.Instance.TryRemoveHarvestedCrop(currentRequiredCrop, 1))
                    {
                        SpawnFlyingItem();
                        GiveReward(); // Geef goud OF score
                        currentFilledSlots++;

                        //crate spawning
                        while (currentFilledSlots >= _amountPerCrate * currentCrate && currentCrate != 6)
                        {
                            Debug.Log(currentCrate);
                            gameObject.transform.GetChild(currentCrate).gameObject.SetActive(true);
                            currentCrate++;
                            yield return null;
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

            // Wissel type voor de volgende boot
            isSellBoat = !isSellBoat;

            boats[0].active = isSellBoat;
            boats[1].active = !isSellBoat;
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
            // Voeg hier je Score-logica toe
            Debug.Log($"+{scorePerItem} Score!");
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