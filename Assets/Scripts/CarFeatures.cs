using UnityEngine;

/// <summary>
/// Car features: headlights, taillights, brake lights, damage system.
/// Attach to any car object.
/// </summary>
public class CarFeatures : MonoBehaviour
{
    [Header("Lights")]
    public bool hasLights = true;
    private Light headLightL, headLightR;
    private Renderer tailLightL, tailLightR;
    private bool lightsOn = false;

    [Header("Damage")]
    public float maxHealth = 100f;
    public float currentHealth;
    private ParticleSystem smokeFX;
    private ParticleSystem fireFX;
    private bool isExploded = false;
    private float explodeTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;

        if (hasLights)
            CreateLights();
    }

    void Update()
    {
        if (isExploded) return;

        // Toggle lights
        if (Input.GetKeyDown(KeyCode.L) || (MobileUI.Instance != null && MobileUI.Instance.LightsPressed))
        {
            lightsOn = !lightsOn;
            SetLights(lightsOn);
        }

        // Brake lights when decelerating
        UpdateBrakeLights();

        // Damage system
        UpdateDamage();
    }

    void CreateLights()
    {
        // Headlights (white, forward-facing)
        headLightL = CreateLight("HeadLight_L", new Vector3(-0.6f, 0.5f, 2f), Color.white, 2f);
        headLightR = CreateLight("HeadLight_R", new Vector3(0.6f, 0.5f, 2f), Color.white, 2f);

        // Taillights (red, backward-facing) — using small cubes
        tailLightL = CreateTailLight("TailLight_L", new Vector3(-0.6f, 0.5f, -2.2f));
        tailLightR = CreateTailLight("TailLight_R", new Vector3(0.6f, 0.5f, -2.2f));

        SetLights(false);
    }

    Light CreateLight(string name, Vector3 localPos, Color color, float range)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;

        Light light = go.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = color;
        light.range = range;
        light.spotAngle = 60f;
        light.intensity = 3f;
        light.enabled = false;

        return light;
    }

    Renderer CreateTailLight(string name, Vector3 localPos)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(transform);
        go.transform.localPosition = localPos;
        go.transform.localScale = new Vector3(0.15f, 0.1f, 0.05f);
        Destroy(go.GetComponent<Collider>());

        Renderer rend = go.GetComponent<Renderer>();
        rend.material.color = new Color(0.3f, 0f, 0f); // dim red when off
        return rend;
    }

    void SetLights(bool on)
    {
        lightsOn = on;
        if (headLightL != null) headLightL.enabled = on;
        if (headLightR != null) headLightR.enabled = on;

        if (tailLightL != null)
            tailLightL.material.color = on ? Color.red : new Color(0.3f, 0f, 0f);
        if (tailLightR != null)
            tailLightR.material.color = on ? Color.red : new Color(0.3f, 0f, 0f);
    }

    void UpdateBrakeLights()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        // Brake lights brighten when car is braking
        float speed = rb.linearVelocity.magnitude;
        bool braking = (Input.GetKey(KeyCode.Space) || (MobileUI.Instance != null && MobileUI.Instance.IsBrakeHeld)) && speed > 0.5f;
        Color brakeColor = braking ? new Color(1f, 0.2f, 0.2f) : (lightsOn ? Color.red : new Color(0.3f, 0f, 0f));

        if (tailLightL != null) tailLightL.material.color = brakeColor;
        if (tailLightR != null) tailLightR.material.color = brakeColor;
    }

    public void TakeDamage(float amount)
    {
        if (isExploded) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= maxHealth * 0.33f && smokeFX == null)
        {
            CreateSmokeFX();
        }

        if (currentHealth <= maxHealth * 0.1f && fireFX == null)
        {
            CreateFireFX();
            explodeTimer = 5f; // Explode in 5 seconds
        }
    }

    void UpdateDamage()
    {
        // Fire countdown to explosion
        if (fireFX != null && explodeTimer > 0)
        {
            explodeTimer -= Time.deltaTime;
            if (explodeTimer <= 0)
            {
                Explode();
            }
        }
    }

    void CreateSmokeFX()
    {
        GameObject go = new GameObject("SmokeFX");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.up * 0.5f;

        smokeFX = go.AddComponent<ParticleSystem>();
        var main = smokeFX.main;
        main.startLifetime = 2f;
        main.startSpeed = 1f;
        main.startSize = 0.5f;
        main.startColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
        main.maxParticles = 30;
        main.loop = true;

        var emission = smokeFX.emission;
        emission.rateOverTime = 10;

        var shape = smokeFX.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        shape.radius = 0.2f;
    }

    void CreateFireFX()
    {
        GameObject go = new GameObject("FireFX");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.up * 0.8f;

        fireFX = go.AddComponent<ParticleSystem>();
        var main = fireFX.main;
        main.startLifetime = 0.8f;
        main.startSpeed = 2f;
        main.startSize = 0.4f;
        main.startColor = Color.orange;
        main.maxParticles = 50;
        main.loop = true;

        var emission = fireFX.emission;
        emission.rateOverTime = 25;

        var shape = fireFX.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 20f;
        shape.radius = 0.3f;
    }

    void Explode()
    {
        if (isExploded) return;
        isExploded = true;

        // Push car up
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(Vector3.up * 5000f, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 3000f, ForceMode.Impulse);
        }

        // Big explosion particle effect
        GameObject expGO = new GameObject("Explosion");
        expGO.transform.position = transform.position + Vector3.up;
        ParticleSystem exp = expGO.AddComponent<ParticleSystem>();
        var main = exp.main;
        main.startLifetime = 1f;
        main.startSpeed = 8f;
        main.startSize = 2f;
        main.startColor = Color.red;
        main.maxParticles = 100;
        main.loop = false;
        main.playOnAwake = true;

        var emission = exp.emission;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 50) });

        Destroy(expGO, 3f);

        // Destroy the car after a delay
        Destroy(gameObject, 2f);
    }
}
