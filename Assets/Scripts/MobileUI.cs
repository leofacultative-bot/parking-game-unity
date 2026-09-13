using UnityEngine;

/// <summary>
/// On-screen mobile buttons: Jump, Sprint, Enter Car, Exit Car.
/// Uses IMGUI (no EventSystem needed).
/// </summary>
public class MobileUI : MonoBehaviour
{
    public static MobileUI Instance;

    public bool IsSprintHeld { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool EnterPressed { get; private set; }
    public bool ExitPressed { get; private set; }
    public bool LightsPressed { get; private set; }
    public bool IsBrakeHeld { get; private set; }

    private bool sprintHeld = false;

    void Awake()
    {
        Instance = this;
    }

    void OnGUI()
    {
        // Only show on mobile (or in editor for testing)
        if (Input.touchCount == 0 && !Application.isEditor) return;

        float scale = Screen.dpi / 160f;
        if (scale < 1) scale = 1;
        float btnW = 120 * scale;
        float btnH = 60 * scale;
        float pad = 20 * scale;
        float rightX = Screen.width - btnW - pad;

        GUIStyle bigBtn = new GUIStyle(GUI.skin.button);
        bigBtn.fontSize = (int)(18 * scale);
        bigBtn.fontStyle = FontStyle.Bold;

        // ---- RIGHT SIDE BUTTONS ----

        // Exit Car button (top right) - only show when in car
        PlayerController pc = PlayerController.Instance;
        if (pc != null && pc.IsInCar)
        {
            if (GUI.Button(new Rect(rightX, pad, btnW, btnH), "EXIT", bigBtn))
            {
                ExitPressed = true;
            }
        }
        else
        {
            // Enter Car button (top right) - only show when near car
            GameObject nearestCar = FindNearestCar();
            if (nearestCar != null && Vector3.Distance(transform.position, nearestCar.transform.position) < 5f)
            {
                if (GUI.Button(new Rect(rightX, pad, btnW, btnH), "ENTER", bigBtn))
                {
                    EnterPressed = true;
                }
            }
        }

        // Jump button (bottom right)
        Rect jumpRect = new Rect(rightX, Screen.height - pad - btnH, btnW, btnH);
        if (GUI.Button(jumpRect, "JUMP", bigBtn))
        {
            JumpPressed = true;
        }

        // Sprint button (above jump)
        Rect sprintRect = new Rect(rightX, Screen.height - pad - btnH * 2 - pad, btnW, btnH);
        GUIStyle sprintStyle = new GUIStyle(bigBtn);
        sprintStyle.normal.textColor = sprintHeld ? Color.yellow : Color.white;
        if (GUI.RepeatButton(sprintRect, "SPRINT", sprintStyle))
        {
            sprintHeld = true;
        }
        else
        {
            sprintHeld = false;
        }

        // ---- LEFT SIDE ----

        // Brake button (bottom left, big red)
        GUIStyle brakeBtn = new GUIStyle(bigBtn);
        brakeBtn.normal.textColor = Color.red;
        if (GUI.RepeatButton(new Rect(pad, Screen.height - pad - btnH, btnW, btnH), "BRAKE", brakeBtn))
        {
            IsBrakeHeld = true;
        }
        else
        {
            IsBrakeHeld = false;
        }

        // Lights button (top left)
        if (GUI.Button(new Rect(pad, pad, btnW, btnH), "LIGHTS", bigBtn))
        {
            LightsPressed = true;
        }
    }

    void LateUpdate()
    {
        // Reset one-shot buttons after a frame
        JumpPressed = false;
        EnterPressed = false;
        ExitPressed = false;
        LightsPressed = false;
    }

    GameObject FindNearestCar()
    {
        CarController[] cars = FindObjectsByType<CarController>(FindObjectsSortMode.None);
        GameObject nearest = null;
        float minDist = 5f;

        foreach (CarController car in cars)
        {
            float dist = Vector3.Distance(transform.position, car.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = car.gameObject;
            }
        }

        return nearest;
    }

    Rigidbody nearestCarRb
    {
        get
        {
            CarController[] cars = FindObjectsByType<CarController>(FindObjectsSortMode.None);
            foreach (CarController car in cars)
            {
                if (Vector3.Distance(transform.position, car.transform.position) < 5f)
                    return car.GetComponent<Rigidbody>();
            }
            return null;
        }
    }
}
