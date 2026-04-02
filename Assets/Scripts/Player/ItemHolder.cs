using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Audio.GeneratorInstance;

public class ItemHolder : MonoBehaviour
{
    [SerializeField] private Transform holdPoint;
    [SerializeField] private BoxCollider collider;
    [SerializeField] private LayerMask toolLayer;
    [SerializeField] private FarmInteraction FarmInteraction;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioSource sfxSource;
    private InputAction _action;

    private PlayerConfiguration _configuration;
    private GameObject _tool;
    private bool IsHoldingItem;
    private SeedIdentifier _heldSeedIdentifier;
    private float _defaultPitch;


    private void Start()
    {
        _defaultPitch = sfxSource.pitch;
    }
    private void Update()
    {
        if (_action == null) return;
        if (_action.WasPressedThisFrame())
        {
            Interact();
        }
        if(_tool == null)
        {
            IsHoldingItem = false;
            FarmInteraction.Mode = InteractionMode.Idle;
        }
    }


    private void Interact()
    {
        //get item
        if (!IsHoldingItem)
        {
            Collider[] overlaps = Physics.OverlapBox
            (
                collider.bounds.center,
                collider.transform.localScale,
                transform.rotation,
                toolLayer
            );

            if (overlaps.Length == 0)
            {
                FarmInteraction.Mode = InteractionMode.Idle;
                return;
            }
            sfxSource.pitch = _defaultPitch;
            _tool = overlaps[0].gameObject;
            _tool.transform.parent = holdPoint;
            _tool.transform.position = holdPoint.position;
            _tool.transform.rotation = holdPoint.rotation;
            _tool.GetComponent<Rigidbody>().isKinematic = true;
            IsHoldingItem = true;
            sfxSource.PlayOneShot(pickupSound);
            string type = overlaps[0].tag;

            switch (type)
            {
                case "Plow":
                    FarmInteraction.Mode = InteractionMode.Plowing;
                    break;
                case "Seed":
                    FarmInteraction.Mode = InteractionMode.Planting;
                    _heldSeedIdentifier = _tool.GetComponent<SeedIdentifier>();
                    break;
                case "Water":
                    FarmInteraction.Mode = InteractionMode.Watering;
                    break;
                case "Sickle":
                    FarmInteraction.Mode = InteractionMode.Harvesting;
                    break;
            }
        }

        //yeet item
        else
        {
            _tool.transform.parent = null;
            _tool.GetComponent<Rigidbody>().isKinematic = false;
            _tool.GetComponent<Rigidbody>().AddForce(transform.forward + transform.up * 5, ForceMode.Impulse);
            _tool = null;
            _heldSeedIdentifier = null;
            FarmInteraction.Mode = InteractionMode.Idle;
            sfxSource.pitch = sfxSource.pitch - .3f;
            sfxSource.PlayOneShot(pickupSound);
            IsHoldingItem = false;
        }
    }

    public SeedIdentifier GetHeldSeedIdentifier()
    {
        return _heldSeedIdentifier;
    }

    public void InitializePlayer(PlayerConfiguration pc)
    {
        _configuration = pc;
        _configuration.Input.onActionTriggered += Input_onActionTriggered1;
    }

    private void Input_onActionTriggered1(InputAction.CallbackContext obj)
    {
        {
            if (obj.action.name == "Attack" && _action == null)
            {
                _action = obj.action;
            }
        }
    }
}
