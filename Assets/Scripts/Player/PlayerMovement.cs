using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player variables")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Renderer renderer;
    [SerializeField] private float stunTime;

    [Header("movement variables")]
    [SerializeField] private float playerSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private ParticleSystem _movementParticles;
    [SerializeField] private Animator _animator;

    private PlayerConfiguration _configuration;
    //private InputAction _movementAction;
    private Vector2 _movementInput;

    private Vector3 _velocity;

    public bool Bombed;
    private bool HasStunned;

    public bool IsPerformingAction;
    private ParticleSystem.EmissionModule _emission;

    void Start()
    {
        //_movementAction = playerInput.currentActionMap.FindAction("Move");
        _emission = _movementParticles.emission;
    }

    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (Bombed && !HasStunned)
        {
            StartCoroutine(StunTime());
        }

        if (_movementInput == Vector2.zero)
        {
            _emission.enabled = false; // when stopped
            _animator.SetBool("IsWalking", false);
        }
        
        Vector3 movement = new Vector3(_movementInput.x, 0f, _movementInput.y) * playerSpeed;
        if (IsPerformingAction) movement = movement / 2;

        if(movement.sqrMagnitude > 0.001f)
        {
            _animator.SetBool("IsWalking", true);
            Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
            if (IsPerformingAction) transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed / 3 * Time.deltaTime);
            else transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);            
        }

        if (!controller.isGrounded)
        {
            _velocity.y += Physics.gravity.y * Time.deltaTime;
        }
        else
        {
            Bombed = false;
            _velocity.y = 0f;
        }

        Vector3 XZVelocity = new Vector3(transform.forward.x, 0, transform.forward.z) * movement.magnitude;
        if (HasStunned && Bombed) XZVelocity = _velocity;
        if (HasStunned) XZVelocity = Vector3.zero;
        _velocity = new Vector3(XZVelocity.x, _velocity.y, XZVelocity.z);
        _emission.enabled = true;  // when moving       

        //_movementParticles.Play();
        controller.Move(_velocity * Time.deltaTime);
    }
        
    public void InitializePlayer(PlayerConfiguration pc)
    {
        _configuration = pc;
        renderer.material = pc.PlayerMaterial;
        _configuration.Input.onActionTriggered += Input_onActionTriggered1;
    }

    private void Input_onActionTriggered1(InputAction.CallbackContext obj)
    {
        if (obj.action != _configuration.Input.currentActionMap.FindAction("Move")) return;
        _movementInput = obj.ReadValue<Vector2>();
    }

    public void AddVelocity(Vector3 velocity)
    {
        _velocity += velocity;
    }

    IEnumerator StunTime()
    {
        HasStunned = true;
        yield return new WaitForSeconds(stunTime);
        HasStunned = false;
    }
}
