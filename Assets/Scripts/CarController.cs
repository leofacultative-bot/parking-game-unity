using UnityEngine;

/// <summary>
/// Car controller using force + torque (inspired by GTA clone patterns).
/// </summary>
public class CarController : MonoBehaviour
{
    [Header("Movement")]
    public float motorForce = 2000f;
    public float steerAngle = 40f;
    public float brakeForce = 2000f;
    public float maxSpeed = 25f;

    private Rigidbody rb;
    private float inputAccel;
    private float inputSteer;
    private bool inputBrake;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.mass = 1200f;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 3f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.centerOfMass = new Vector3(0, -1.5f, 0);
    }

    void Update()
    {
        ReadInput();
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        CheckGrounded();
        ApplyMotor();
        ApplySteering();
        ApplyBrake();
        LimitSpeed();
    }

    void ReadInput()
    {
        // Only read input if player is in car
        if (PlayerController.Instance != null && !PlayerController.Instance.IsInCar)
        {
            inputAccel = 0;
            inputSteer = 0;
            inputBrake = false;
            return;
        }

        inputAccel = Input.GetAxis("Vertical");
        inputSteer = Input.GetAxis("Horizontal");
        inputBrake = Input.GetKey(KeyCode.Space);

        // Mobile brake
        if (MobileUI.Instance != null && MobileUI.Instance.IsBrakeHeld)
            inputBrake = true;

        // Mobile joystick controls car when player is in it
        if (MobileJoystick.Instance != null && MobileJoystick.Instance.InputVector.magnitude > 0.1f)
        {
            inputSteer = MobileJoystick.Instance.InputVector.x;
            inputAccel = MobileJoystick.Instance.InputVector.y;
        }
    }

    void CheckGrounded()
    {
        // Cast from 4 corners of car
        Vector3[] offsets = {
            new Vector3(-0.8f, 0, 0.8f),
            new Vector3(0.8f, 0, 0.8f),
            new Vector3(-0.8f, 0, -0.8f),
            new Vector3(0.8f, 0, -0.8f),
        };

        int groundedCount = 0;
        foreach (Vector3 offset in offsets)
        {
            Vector3 rayStart = transform.TransformPoint(offset) + Vector3.up * 0.5f;
            if (Physics.Raycast(rayStart, Vector3.down, 1.5f))
            {
                groundedCount++;
            }
        }

        isGrounded = groundedCount >= 2;
    }

    void ApplyMotor()
    {
        if (!isGrounded) return;
        if (Mathf.Abs(inputAccel) < 0.1f) return;

        // Check if going backward and player wants forward (brake first)
        float currentSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        if (currentSpeed < -0.5f && inputAccel > 0)
        {
            rb.AddForce(-rb.linearVelocity.normalized * brakeForce, ForceMode.Acceleration);
            return;
        }

        Vector3 force = transform.forward * inputAccel * motorForce;
        rb.AddForce(force, ForceMode.Acceleration);
    }

    void ApplySteering()
    {
        if (!isGrounded) return;
        if (Mathf.Abs(inputSteer) < 0.05f) return;

        float speed = rb.linearVelocity.magnitude;
        if (speed < 0.5f) return;

        // GTA-style torque steering
        float speedFactor = Mathf.Clamp01(1f - speed / maxSpeed);
        float currentSteer = inputSteer * steerAngle * Mathf.Lerp(0.3f, 1f, speedFactor);

        Quaternion steerRotation = Quaternion.Euler(0, currentSteer * Time.fixedDeltaTime * speed, 0);
        rb.MoveRotation(rb.rotation * steerRotation);
    }

    void ApplyBrake()
    {
        if (!inputBrake && Mathf.Abs(inputAccel) > 0.1f) return;

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > 0.1f)
        {
            rb.AddForce(-horizontalVelocity.normalized * brakeForce, ForceMode.Acceleration);
        }
    }

    void LimitSpeed()
    {
        float speed = rb.linearVelocity.magnitude;
        if (speed > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
}
