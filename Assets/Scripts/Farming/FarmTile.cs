using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class FarmTile : MonoBehaviour
{
    public event EventHandler WaterSwitched;

    [SerializeField] private Material[] materialStates;
    [SerializeField] private float plowingTime;
    [SerializeField] private float wateringTime;
    [SerializeField] private float harvestingTime;
    [SerializeField] private Renderer renderer;
    [SerializeField] private Transform plantSpawnPoint;
    [SerializeField] private GameObject CropObject;

    public PlantedCrop PlantedCrop { get; private set; }
    public bool IsPlowed { get; private set; }
    public bool IsWatered { get; private set; }
    public bool IsPlanted { get; private set; }
    public bool IsReadyToHarvest { get; set; }

    public bool HasBomb { get; set; }
    public GameObject BombObject;

    private float _timer;
    public float WateringTimer = 10f;

    private void Start()
    {
        IsPlowed = false;
        IsWatered = false;
        IsReadyToHarvest = false;
    }

    public void Update()
    {
        UpdateWaterTimer();
    }

    private void UpdateWaterTimer()
    {
        if (!IsWatered)
            return;

        _timer += Time.deltaTime;
        if (_timer > WateringTimer)
        {
            DryOutPlot();
        }
    }

    public void PlowPlot()
    {
        if (IsPlowed) return;
        IsPlowed = true;
        renderer.material = materialStates[1];
        if (HasBomb)
        {
            StartCoroutine(BombObject.GetComponent<Mine>().TriggerMine());
        }
    }

    public void WaterPlot()
    {
        if (IsWatered || !IsPlowed) return;
        IsWatered = true;
        renderer.material = materialStates[2];
        _timer = 0f;
    }

    private void DryOutPlot()
    {
        IsWatered = false;
        renderer.material = materialStates[1];
    }

    public void ResetPlot()
    {
        IsPlowed = false;
        IsWatered = false;
        IsPlanted = false;
        IsReadyToHarvest = false;
        renderer.material = materialStates[0];
    }

    public void PlantPlot(CropSO cropData)
    {
        if (!IsPlowed || PlantedCrop != null)
        {
            Debug.LogWarning("Cannot plant on this tile");
            return;
        }

        Vector3 spawnPosition = plantSpawnPoint != null ? plantSpawnPoint.position : transform.position;
        GameObject plantedCropObject = Instantiate(CropObject, plantSpawnPoint.position, Quaternion.identity);
        plantedCropObject.transform.SetParent(transform, true);
        plantedCropObject.name = $"PlantedCrop_{cropData.CropName}";

        PlantedCrop = plantedCropObject.GetComponent<PlantedCrop>();
        PlantedCrop.ParentFarmTile = this;
        PlantedCrop.CropData = cropData;
        PlantedCrop.VisualRoot = plantedCropObject.transform;
        IsPlanted = true;
    }
    
    public void Spawnbomb()
    {
        var bomb = Instantiate(BombObject, transform.position - transform.up / 10, Quaternion.identity);
        bomb.GetComponent<Mine>().ParentTile = this;
    }
}
