using UnityEngine;

/// <summary>
/// Simple mobile joystick - touches on left half of screen control movement.
/// No UI dependencies - uses raw touch input.
/// </summary>
public class MobileJoystick : MonoBehaviour
{
    public static MobileJoystick Instance;

    private Vector2 inputVector = Vector2.zero;
    private int touchIndex = -1;
    private Vector2 touchStartPos;

    public Vector2 InputVector => inputVector;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            // Find touch on left half of screen
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (touch.phase == TouchPhase.Began && touch.position.x < Screen.width * 0.5f)
                {
                    touchIndex = i;
                    touchStartPos = touch.position;
                }

                if (touchIndex >= 0 && i == touchIndex)
                {
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        Vector2 delta = touch.position - touchStartPos;
                        float maxDist = 100f;
                        inputVector = Vector2.ClampMagnitude(delta / maxDist, 1f);
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        inputVector = Vector2.zero;
                        touchIndex = -1;
                    }
                }
            }
        }
        else
        {
            inputVector = Vector2.zero;
            touchIndex = -1;
        }
    }

    // Draw joystick indicator on screen
    void OnGUI()
    {
        // Draw joystick background
        float size = 150f;
        float x = 50;
        float y = Screen.height - size - 50;

        GUI.color = new Color(1, 1, 1, 0.3f);
        GUI.DrawTexture(new Rect(x, y, size, size), Texture2D.whiteTexture);

        // Draw knob
        float knobSize = 50f;
        float knobX = x + (size - knobSize) / 2 + inputVector.x * (size - knobSize) / 2;
        float knobY = y + (size - knobSize) / 2 - inputVector.y * (size - knobSize) / 2;

        GUI.color = new Color(1, 1, 1, 0.6f);
        GUI.DrawTexture(new Rect(knobX, knobY, knobSize, knobSize), Texture2D.whiteTexture);

        GUI.color = Color.white;
    }
}
