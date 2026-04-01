using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum InteractionMode
{
    Idle,
    Plowing,
    Planting,
    Watering,
    Harvesting
}   

public class FarmInteraction : MonoBehaviour
{
    [SerializeField] private Slider _progressSlider;
    [SerializeField] private float holdInterval = 0.5f;
    [SerializeField] private PlayerMovement movement;

    public InteractionMode Mode = InteractionMode.Idle;

    private InputAction _action;
    private ItemHolder _itemHolder;
    private PlayerConfiguration _configuration;

    private FarmTile _tile;
    [SerializeField] private float _holdTimer;
    private bool held;

    private bool ISPlaying;

    private void Start()
    {
        _itemHolder = GetComponent<ItemHolder>();
        _progressSlider.gameObject.SetActive(false);
    }

    //detecting tiles
    private void FixedUpdate()
    {
        Ray ray = new Ray(transform.position - transform.up, -transform.up);
        Debug.DrawRay(transform.position - transform.up, -transform.up, Color.red);
        if (Physics.Raycast(ray, out RaycastHit hit, .1f))
        {
            if (hit.transform.tag == "Tile")
            {
                if (_tile == null)
                {
                    Debug.Log("new tile");
                    _tile = hit.transform.gameObject.GetComponent<FarmTile>();
                    return;
                }
                else if (hit.transform.gameObject.GetComponent<FarmTile>() != _tile)
                {
                    _tile = hit.transform.gameObject.GetComponent<FarmTile>();
                    Debug.Log("replaced tile"); 
                }
            }
        }
        else _tile = null;
    }

    void Update()
    {
        if (_action == null) return;

        _action.started += _ => held = true;
        _action.canceled += _ => held = false;

        movement.IsPerformingAction = held;

        //determine what action is used
        if (_tile == null)
        {
            _progressSlider.gameObject.SetActive(false);
            return;
        }

        if (!held) 
        {
            _holdTimer = 0;
            _progressSlider.gameObject.SetActive(false);
            return;
        }

        _progressSlider.gameObject.SetActive(true);

        switch (Mode)
        {
            case InteractionMode.Idle:
                break;
            case InteractionMode.Plowing:
                Plowing();
                break;
            case InteractionMode.Planting:
                Planting();
                break;
            case InteractionMode.Watering:
                Watering();
                break;
            case InteractionMode.Harvesting:
                Harvesting();
                break;
        }
    }

    [SerializeField] private Animator _animator;

    void TriggerAnimation()
    {
        if (ISPlaying) return;
        _animator.SetTrigger("UseTool");
    }
    void Plowing()
    {
        if (_tile.IsPlowed) return;
        TriggerAnimation();
        ISPlaying = true;

        _progressSlider.value = _holdTimer / holdInterval;
        if (held && _holdTimer >= holdInterval)
        {
            Debug.Log("p1");
            _tile.PlowPlot();
            _holdTimer = 0;
            ISPlaying = false;
            return;
        }
        _holdTimer += Time.deltaTime;
    }

    void Planting()
    {
        if (_tile.IsPlanted) return;
        TriggerAnimation();
        ISPlaying = true;

        _progressSlider.value = _holdTimer / holdInterval;
        if (held && _holdTimer >= holdInterval)
        {
            SeedIdentifier seedIdentifier = _itemHolder.GetHeldSeedIdentifier();
            if (seedIdentifier != null)
            {
                CropSO cropData = CropManager.Instance.GetCropByName(seedIdentifier.CropName);
                if (cropData != null)
                {
                    _tile.PlantPlot(cropData);
                    Debug.Log($"Planted {seedIdentifier.CropName}");
                }
            }
            _holdTimer = 0;
            ISPlaying = false;
            return;
        }
        _holdTimer += Time.deltaTime;
    }

    void Watering()
    {
        if (_tile.IsWatered) return;
        TriggerAnimation();
        ISPlaying = true;

        _progressSlider.value = _holdTimer / holdInterval;
        if (held && _holdTimer >= holdInterval)
        {
            Debug.Log("w1");
            _tile.WaterPlot();
            _holdTimer = 0;
            ISPlaying = false;
            return;
        }
        _holdTimer += Time.deltaTime;
    }

    void Harvesting()
    {
        if (!_tile.IsReadyToHarvest) return;
        TriggerAnimation();
        ISPlaying = true;

        _progressSlider.value = _holdTimer / holdInterval;
        if (held && _holdTimer >= holdInterval)
        {
            Debug.Log("h1");
            _tile.ResetPlot();
            PlantedCrop crop = _tile.PlantedCrop;
            if(crop != null)
                crop.RequestHarvest();
            _holdTimer = 0;
            ISPlaying = false;
            return;
        }
        _holdTimer += Time.deltaTime;
    }

    public void InitializePlayer(PlayerConfiguration pc)
    {
        _configuration = pc;
        _configuration.Input.onActionTriggered += Input_onActionTriggered1;
    }

    private void Input_onActionTriggered1(InputAction.CallbackContext obj)
    {
        {
            if (obj.action.name == "Interact" && _action == null)
            {
                _action = obj.action;
            }
        }
    }
}
