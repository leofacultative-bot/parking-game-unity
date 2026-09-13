using UnityEngine;

/// <summary>
/// Simple day/night cycle. Moves the directional light around the scene.
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    public float dayDuration = 120f; // seconds for full day cycle
    public Light directionalLight;
    private float timeOfDay = 0.3f; // start at morning (0=midnight, 0.5=noon)

    void Start()
    {
        if (directionalLight == null)
            directionalLight = FindFirstObjectByType<Light>();
    }

    void Update()
    {
        timeOfDay += Time.deltaTime / dayDuration;
        if (timeOfDay >= 1f) timeOfDay -= 1f;

        // Rotate the light around the X axis (sun position)
        float sunAngle = (timeOfDay - 0.25f) * 360f; // 0.25 = noon at top
        if (directionalLight != null)
        {
            directionalLight.transform.rotation = Quaternion.Euler(sunAngle, -30f, 0f);

            // Intensity: bright at noon, dim at night
            float noonness = Mathf.Sin(timeOfDay * Mathf.PI * 2f - Mathf.PI * 0.5f);
            noonness = Mathf.Clamp01(noonness);
            directionalLight.intensity = Mathf.Lerp(0.2f, 1.4f, noonness);

            // Color: warm at sunrise/sunset, white at noon
            Color dawnColor = new Color(1f, 0.6f, 0.3f);
            Color noonColor = new Color(1f, 1f, 0.95f);
            Color nightColor = new Color(0.2f, 0.2f, 0.4f);

            if (noonness > 0.3f)
                directionalLight.color = Color.Lerp(dawnColor, noonColor, (noonness - 0.3f) / 0.7f);
            else
                directionalLight.color = Color.Lerp(nightColor, dawnColor, noonness / 0.3f);
        }

        // Camera background color matches sky
        Camera cam = Camera.main;
        if (cam != null)
        {
            float noonness2 = Mathf.Sin(timeOfDay * Mathf.PI * 2f - Mathf.PI * 0.5f);
            noonness2 = Mathf.Clamp01(noonness2);
            cam.backgroundColor = Color.Lerp(new Color(0.05f, 0.05f, 0.15f), new Color(0.5f, 0.7f, 1f), noonness2);
        }
    }
}
