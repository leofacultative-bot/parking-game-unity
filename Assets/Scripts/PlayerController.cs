using UnityEngine;

/// <summary>
/// Player character controller — walk, run, jump, sprint with joystick or keyboard.
/// Camera follows player. Enter/exit car with E key or mobile button.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Movement")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float turnSpeed = 5f;
    public float jumpForce = 8f;
    public float gravity = 20f;

    private CharacterController controller;
    private Camera mainCamera;
    private Animator animator;
    private float cameraSmooth = 20f;
    private bool isInCar = false;
    private GameObject currentCar;
    private Vector3 moveDirection = Vector3.zero;
    private bool isSprinting = false;
    private bool wantsToJump = false;
    private bool wantsToEnterCar = false;
    private bool wantsToExitCar = false;

    public bool IsInCar => isInCar;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();

        controller = GetComponent<CharacterController>();
        if (controller == null)
            controller = gameObject.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.center = new Vector3(0, 0.9f, 0);
        controller.radius = 0.3f;
        controller.slopeLimit = 45f;
        controller.stepOffset = 0.3f;

        if (mainCamera != null)
        {
            // Camera behind the player (mesh faces -Z due to 170° child rotation,
            // so -transform.forward puts camera behind the visible mesh)
            Vector3 behindPlayer = -transform.forward * 6f + Vector3.up * 3.5f;
            mainCamera.transform.position = transform.position + behindPlayer;
            mainCamera.transform.LookAt(transform.position + Vector3.up * 1.2f);
        }
    }

    void Update()
    {
        if (isInCar)
        {
            if (wantsToExitCar || Input.GetKeyDown(KeyCode.E))
                ExitCar();
            return;
        }

        ReadInput();
        HandleMovement();
        HandleCarEntry();
    }

    void LateUpdate()
    {
        UpdateCamera();
    }

    void ReadInput()
    {
        // Keyboard
        if (Input.GetKeyDown(KeyCode.Space)) wantsToJump = true;
        isSprinting = Input.GetKey(KeyCode.LeftShift);
        if (Input.GetKeyDown(KeyCode.E)) wantsToEnterCar = true;

        // Mobile buttons set these via MobileUI
        if (MobileUI.Instance != null)
        {
            isSprinting = MobileUI.Instance.IsSprintHeld;
            if (MobileUI.Instance.JumpPressed) wantsToJump = true;
            if (MobileUI.Instance.EnterPressed) wantsToEnterCar = true;
            if (MobileUI.Instance.ExitPressed) wantsToExitCar = true;
        }
    }

    void HandleMovement()
    {
        float h = 0, v = 0;

        // Keyboard
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        // Mobile joystick
        if (MobileJoystick.Instance != null && MobileJoystick.Instance.InputVector.magnitude > 0.1f)
        {
            h = MobileJoystick.Instance.InputVector.x;
            v = MobileJoystick.Instance.InputVector.y;
        }

        Vector2 input = new Vector2(h, v).normalized;
        float inputMagnitude = input.magnitude;

        if (controller.isGrounded)
        {
            if (inputMagnitude > 0.1f)
            {
                // Move relative to camera direction
                Vector3 camForward = mainCamera.transform.forward;
                camForward.y = 0;
                camForward.Normalize();
                Vector3 camRight = mainCamera.transform.right;
                camRight.y = 0;
                camRight.Normalize();

                moveDirection = (camForward * input.y + camRight * input.x).normalized;

                // Smooth rotation toward movement direction
                Quaternion targetRot = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);

                // Speed
                float speed = isSprinting ? sprintSpeed : walkSpeed;
                moveDirection *= speed;
            }
            else
            {
                moveDirection = Vector3.zero;
            }

            // Jump
            if (wantsToJump)
            {
                moveDirection.y = jumpForce;
                wantsToJump = false;
            }
        }

        // Apply gravity (always, even when inputMagnitude is 0)
        moveDirection.y -= gravity * Time.deltaTime;

        // Move — inputMagnitude only affects horizontal, gravity always applies
        Vector3 horizontalMove = new Vector3(moveDirection.x, 0, moveDirection.z) * inputMagnitude;
        horizontalMove.y = moveDirection.y; // gravity component is independent
        controller.Move(horizontalMove * Time.deltaTime);

        // Drive Animator
        if (animator != null)
        {
            animator.SetFloat("Speed", inputMagnitude * (isSprinting ? 1f : 0.5f));
        }

        // Reset flags
        wantsToJump = false;
        wantsToEnterCar = false;
        wantsToExitCar = false;
    }

    void HandleCarEntry()
    {
        if (!wantsToEnterCar) return;

        CarController[] cars = FindObjectsByType<CarController>(FindObjectsSortMode.None);
        foreach (CarController car in cars)
        {
            if (Vector3.Distance(transform.position, car.transform.position) < 4f)
            {
                EnterCar(car.gameObject);
                break;
            }
        }
    }

    public void EnterCar(GameObject car)
    {
        isInCar = true;
        currentCar = car;
        gameObject.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.car = car;
    }

    public void ExitCar()
    {
        isInCar = false;

        if (currentCar != null)
        {
            transform.position = currentCar.transform.position + currentCar.transform.right * 3f;
            transform.position += Vector3.up * 0.5f;
            transform.rotation = currentCar.transform.rotation;
        }

        gameObject.SetActive(true);

        if (GameManager.Instance != null)
            GameManager.Instance.car = gameObject;

        currentCar = null;
        wantsToExitCar = false;
    }

    void UpdateCamera()
    {
        if (mainCamera == null) return;

        // Camera behind the player (see Start() comment about -Z mesh facing)
        Vector3 behindPlayer = -transform.forward * 6f + Vector3.up * 3.5f;
        Vector3 targetPos = transform.position + behindPlayer;
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, cameraSmooth * Time.deltaTime);

        Vector3 lookTarget = transform.position + Vector3.up * 1.2f;
        mainCamera.transform.LookAt(lookTarget);
    }
}
