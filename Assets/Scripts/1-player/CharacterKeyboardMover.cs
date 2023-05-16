using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


/**
 * This component moves a player controlled with a CharacterController using the keyboard.
 */
[RequireComponent(typeof(CharacterController))]
public class CharacterKeyboardMover: MonoBehaviour {
    // params
    [Tooltip("Speed of player keyboard-movement, in meters/second")]
    [SerializeField] float speed = 3.5f;
    [SerializeField] float runSpeed = 3.5f;
    [SerializeField] float gravity = 9.81f;
    [SerializeField] float jumpStrength = 10f;
    [SerializeField] float chestPlayerDistanceDelta = 3f;
    [SerializeField] float openChestSpeed = 30f;
    [SerializeField] float openChestRotationX = 308f;

    // buttons
    [SerializeField] InputAction moveAction;
    [SerializeField] InputAction jumpButton = new InputAction(type: InputActionType.Button);
    [SerializeField] InputAction runButton = new InputAction(type: InputActionType.Button);
    [SerializeField] InputAction changeGunA = new InputAction(type: InputActionType.Button);
    [SerializeField] InputAction changeGunB = new InputAction(type: InputActionType.Button);
    [SerializeField] InputAction openChestButton = new InputAction(type: InputActionType.Button);

    // materials
    [SerializeField] Material gunAMaterial;
    [SerializeField] Material gunBMaterial;

    // game objects
    [SerializeField] GameObject gunObject;
    [SerializeField] GameObject chestRoofObject;

    // character controller
    private CharacterController cc;

    // extra
    bool openingChest = false;

    private void OnEnable() {
        moveAction.Enable();
        jumpButton.Enable();
        runButton.Enable();
        changeGunA.Enable();
        changeGunB.Enable();
        openChestButton.Enable();
    }

    private void OnDisable() {
        moveAction.Disable();
        jumpButton.Disable();
        runButton.Disable();
        changeGunA.Disable();
        changeGunB.Disable();
        openChestButton.Disable();
    }

    void OnValidate() {
        // Provide default bindings for the input actions.
        // Based on answer by DMGregory: https://gamedev.stackexchange.com/a/205345/18261
        if (moveAction == null)
            moveAction = new InputAction(type: InputActionType.Button);
        if (moveAction.bindings.Count == 0)
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
    }

    void Start() {
        cc = GetComponent<CharacterController>();
    }

    Vector3 velocity = new Vector3(0,0,0);

    void Update()  {
        if (cc.isGrounded) {
            OpenChestIfNeeded();
            SwitchGunIfNeeded();
            JumpIfNeeded();
            RunIfNeeded();
            Vector3 movement = moveAction.ReadValue<Vector2>(); // Implicitly convert Vector2 to Vector3, setting z=0.
            velocity.x = movement.x * speed;
            velocity.z = movement.y * speed;
        } else {
            velocity.y -= gravity*Time.deltaTime;
        }

        // Move in the direction you look:
        velocity = transform.TransformDirection(velocity);

        cc.Move(velocity * Time.deltaTime);
    }

    private void OpenChestIfNeeded() {
        float chestPlayerDistance = Vector3.Distance(transform.position, chestRoofObject.transform.position);
        bool isPlayerCloseToChest = chestPlayerDistance < chestPlayerDistanceDelta;
        bool canOpenChest = isPlayerCloseToChest && cc.isGrounded && openChestButton.WasPressedThisFrame();
        if (canOpenChest)
        {
            openingChest = true;
        }
        if (openingChest)
        {
            Vector3 rotateVelocity = new Vector3(-openChestSpeed, 0, 0);
            chestRoofObject.transform.Rotate(rotateVelocity * Time.deltaTime);
        }
        bool isChestDoneOpened = chestRoofObject.transform.eulerAngles.x < openChestRotationX;
        if (isChestDoneOpened)
        {
            openingChest = false;
        }
    }

    private void SwitchGunIfNeeded()
    {
        if (changeGunA.WasReleasedThisFrame())
        {
            gunObject.GetComponent<MeshRenderer>().material = gunAMaterial;
        }
        if (changeGunB.WasReleasedThisFrame())
        {
            gunObject.GetComponent<MeshRenderer>().material = gunBMaterial;
        }
    }

    private void JumpIfNeeded()
    {
        if (jumpButton.WasPressedThisFrame())
        {
            velocity.y += jumpStrength;
        }
    }

    private void RunIfNeeded()
    {
        if (runButton.WasPressedThisFrame())
        {
            speed += runSpeed;
        }
        else if (runButton.WasReleasedThisFrame())
        {
            speed -= runSpeed;
        }
    }
}
