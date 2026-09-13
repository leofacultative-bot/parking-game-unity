using UnityEngine;

/// <summary>
/// HUD overlay: player health bar and car speedometer.
/// </summary>
public class HUD : MonoBehaviour
{
    public static HUD Instance;
    private float playerHealth = 100f;
    private float maxPlayerHealth = 100f;

    void Awake()
    {
        Instance = this;
    }

    public void TakeDamage(float amount)
    {
        playerHealth -= amount;
        playerHealth = Mathf.Clamp(playerHealth, 0, maxPlayerHealth);
    }

    public void Heal(float amount)
    {
        playerHealth += amount;
        playerHealth = Mathf.Clamp(playerHealth, 0, maxPlayerHealth);
    }

    void OnGUI()
    {
        if (Input.touchCount == 0 && !Application.isEditor) return;

        float scale = Screen.dpi / 160f;
        if (scale < 1) scale = 1;
        float pad = 15 * scale;

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = (int)(16 * scale);
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = Color.white;

        // ---- HEALTH BAR (top left) ----
        float barW = 200 * scale;
        float barH = 25 * scale;
        float barX = pad;
        float barY = pad + 50 * scale; // below lights button

        // Background
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(barX, barY, barW, barH), Texture2D.whiteTexture);

        // Fill based on health
        float healthPercent = playerHealth / maxPlayerHealth;
        GUI.color = healthPercent > 0.5f ? Color.green : (healthPercent > 0.25f ? Color.yellow : Color.red);
        GUI.DrawTexture(new Rect(barX + 2, barY + 2, (barW - 4) * healthPercent, barH - 4), Texture2D.whiteTexture);

        GUI.color = Color.white;
        GUI.Label(new Rect(barX, barY - 20 * scale, barW, 20 * scale), "HEALTH", labelStyle);

        // ---- SPEEDOMETER (bottom center) ----
        PlayerController pc = PlayerController.Instance;
        if (pc != null && pc.IsInCar)
        {
            Rigidbody rb = FindCarRigidbody();
            if (rb != null)
            {
                float speed = rb.linearVelocity.magnitude * 3.6f; // m/s to km/h
                string speedText = $"{speed:F0} km/h";

                GUIStyle speedStyle = new GUIStyle(GUI.skin.label);
                speedStyle.fontSize = (int)(28 * scale);
                speedStyle.fontStyle = FontStyle.Bold;
                speedStyle.alignment = TextAnchor.MiddleCenter;
                speedStyle.normal.textColor = Color.white;

                float speedW = 200 * scale;
                float speedH = 50 * scale;
                GUI.Label(new Rect((Screen.width - speedW) / 2, Screen.height - speedH - pad - 70 * scale, speedW, speedH), speedText, speedStyle);
            }
        }

        // ---- DAMAGE NUMBERS (floating) ----
        GUI.color = Color.white;
    }

    Rigidbody FindCarRigidbody()
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
