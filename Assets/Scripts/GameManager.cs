using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject car;
    public Camera mainCamera;

    private Vector3 cameraOffset = new Vector3(0, 4f, -10f);
    private float cameraSmooth = 5f;

    void Start()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        // Camera is handled by PlayerController (on foot) and CarController (in car).
        // GameManager does NOT move the camera.
    }

    public void ResetCar()
    {
        if (car != null)
        {
            car.transform.position = new Vector3(0, 1f, -8);
            car.transform.rotation = Quaternion.identity;
            Rigidbody rb = car.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}
