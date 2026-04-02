using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

public partial class PlantedCrop : MonoBehaviour
{
    public FarmTile ParentFarmTile;
    public CropSO CropData;
    public Transform VisualRoot;

    public float GrowthMultiplier = 1f;

    private float _timeSpentGrowing;
    private List<GameObject> _cropObjects;

    private CropFSM _cropFSM;


    [Space(10), Header("Particles effects")]
    [SerializeField] private VisualEffect _poofEffect;
    [SerializeField] private VisualEffect _starEffect;
    [SerializeField] private VisualEffect _twinkleEffect;

    private void Start()
    {
        InstantiateCropPool();
        _cropFSM = new CropFSM(this);

        if (ParentFarmTile == null)
            ParentFarmTile = GetComponentInParent<FarmTile>();

        SubscribeToFarmTile();
        
    }

    private void Update()
    {
        _cropFSM.Update(Time.deltaTime);             
    }

    private void OnDestroy()
    {
        if (ParentFarmTile != null)
        {
            UnsubscribeFromFarmTileEvents();
        }
    }

    private void SubscribeToFarmTile()
    {
        ParentFarmTile.WaterSwitched += ParentFarmTile_WaterSwitched;
    }   

    private void UnsubscribeFromFarmTileEvents()
    {
        ParentFarmTile.WaterSwitched -= ParentFarmTile_WaterSwitched;
    }

    private void InstantiateCropPool()
    {
        _cropObjects = new List<GameObject>(CropData.GameObjects.Count);
        foreach(GameObject obj in CropData.GameObjects)
        {
            GameObject crop = Instantiate(obj, VisualRoot);
            crop.SetActive(false);
            _cropObjects.Add(crop);
        }
    }

    public void TransitionToStage(int stage)
    {
        if(stage > 0)
            _cropObjects[stage - 1].SetActive(false);
        _cropObjects[stage].SetActive(true);
    }

    public void RequestHarvest()
    {
        _cropFSM.CurrentState.HarvestCrop();
    }

    private void Harvest()
    {
        int cropyield = UnityEngine.Random.Range(CropData.CropYieldMin, CropData.CropYieldMax);
        CropManager.Instance.AddHarvestedCrop(CropData.name, cropyield);
        CropManager.Instance.GetHarvestedCropAmount(CropData.name);
        Destroy(this.gameObject);
    }

    private void ParentFarmTile_WaterSwitched(object sender, EventArgs e)
    {
        if (GrowthMultiplier == 1f)
        {
            GrowthMultiplier = 0.5f;
        }

        if (GrowthMultiplier == 0.5f)
        {
            GrowthMultiplier = 1f;
        }
        
    }

    private void PlayGrownParticles(bool playStars)
    {
        _poofEffect.Play();
        if(playStars)
        {
            _twinkleEffect.Play();
            _starEffect.Play();
        }
    }
}
