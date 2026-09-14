using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class SceneSetup : MonoBehaviour
{
    bool initialized = false;

    void Awake()
    {
        if (initialized) return;
        if (GameObject.Find("Ground") != null) return; // already set up
        initialized = true;
        SetupScene();
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Rebuild Scene %&r")]
    static void RebuildScene()
    {
        // Delete everything except camera and light
        foreach (GameObject go in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.name == "Main Camera" || go.name == "Directional Light" ||
                go.name == "EventSystem" || go.name == "SceneSetup" ||
                go.GetComponent<Camera>() != null || go.GetComponent<Light>() != null)
                continue;
            DestroyImmediate(go);
        }

        // Add SceneSetup if missing
        if (FindFirstObjectByType<SceneSetup>() == null)
        {
            GameObject go = new GameObject("SceneSetup");
            go.AddComponent<SceneSetup>();
        }
        else
        {
            SceneSetup setup = FindFirstObjectByType<SceneSetup>();
            setup.initialized = false;
            setup.SetupScene();
        }

        Debug.Log("Scene rebuilt with Audi R8!");
    }
#endif

    void SetupScene()
    {
        // Ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0, 0, 0);
        ground.transform.localScale = new Vector3(50, 0.1f, 50);
        ground.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f);

        // Parking spot (yellow)
        GameObject spot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        spot.name = "ParkingSpot";
        spot.transform.position = new Vector3(0, 0.06f, 5);
        spot.transform.localScale = new Vector3(4, 0.05f, 6);
        spot.GetComponent<Renderer>().material.color = new Color(1f, 0.9f, 0f);

        // Car - try to load real model, fall back to primitive
        GameObject car = LoadCarModel();
        if (car == null)
        {
            car = CreatePrimitiveCar();
        }

        // Additional parked cars
        CreateParkedCar(new Vector3(8, 0.6f, -8), new Color(0.2f, 0.4f, 0.8f)); // blue
        CreateParkedCar(new Vector3(-8, 0.6f, -8), new Color(0.2f, 0.7f, 0.2f)); // green
        CreateParkedCar(new Vector3(8, 0.6f, 8), new Color(0.9f, 0.8f, 0.1f)); // yellow
        CreateParkedCar(new Vector3(-8, 0.6f, 8), new Color(0.6f, 0.1f, 0.6f)); // purple

        // Walls around parking lot
        float wallH = 1.5f;
        float wallT = 0.3f;
        float halfSize = 25f;
        Color wallColor = new Color(0.6f, 0.6f, 0.6f);

        CreateWall("WallNorth", new Vector3(0, wallH/2, halfSize), new Vector3(halfSize*2, wallH, wallT), wallColor);
        CreateWall("WallSouth", new Vector3(0, wallH/2, -halfSize), new Vector3(halfSize*2, wallH, wallT), wallColor);
        CreateWall("WallEast", new Vector3(halfSize, wallH/2, 0), new Vector3(wallT, wallH, halfSize*2), wallColor);
        CreateWall("WallWest", new Vector3(-halfSize, wallH/2, 0), new Vector3(wallT, wallH, halfSize*2), wallColor);

        // Setup Camera
        Camera cam = Camera.main;
        cam.transform.position = new Vector3(0, 4f, -18);
        cam.transform.rotation = Quaternion.Euler(20, 0, 0);
        cam.backgroundColor = new Color(0.5f, 0.7f, 1f);

        // Create Player character
        GameObject player = CreatePlayer(new Vector3(0, 0, -3));

        // Add GameManager - camera follows player by default
        GameManager gm = gameObject.AddComponent<GameManager>();
        gm.car = player; // camera follows player
        gm.mainCamera = cam;

        // Mobile Joystick
        gameObject.AddComponent<MobileJoystick>();

        // Mobile UI Buttons (Jump, Sprint, Enter, Exit)
        gameObject.AddComponent<MobileUI>();

        // HUD (health bar, speedometer)
        gameObject.AddComponent<HUD>();

        // Light
        Light light = FindFirstObjectByType<Light>();
        if (light != null)
        {
            light.transform.position = new Vector3(10, 15, 0);
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
            light.intensity = 1.2f;
        }

        // Day/Night Cycle
        gameObject.AddComponent<DayNightCycle>();
    }

    GameObject CreatePlayer(Vector3 pos)
    {
        // Try loading Ripley model
        GameObject ripleyPrefab = Resources.Load<GameObject>("Models/Ripley/Warrior Idle");
        GameObject player;

        if (ripleyPrefab != null)
        {
            player = Instantiate(ripleyPrefab, pos, Quaternion.identity);
            player.name = "Player";
            player.transform.localScale = new Vector3(3f, 3f, 3f);
        }
        else
        {
            player = CreatePerson("Player", pos);
        }

        // Add PlayerController
        PlayerController pc = player.AddComponent<PlayerController>();

        // Set up Animator with walking animation
        Animator anim = player.GetComponent<Animator>();
        if (anim == null)
            anim = player.AddComponent<Animator>();
        RuntimeAnimatorController runtimeController = Resources.Load<RuntimeAnimatorController>("Animations/PlayerAnimator");
        if (runtimeController != null)
        {
            anim.runtimeAnimatorController = runtimeController;
            Debug.Log("Assigned walking animation to player");
        }
        else
        {
            Debug.LogWarning("PlayerAnimator.controller not found — run Tools > Create Player Animator in Unity Editor");
        }

        return player;
    }

    GameObject LoadCarModel()
    {
        // Try loading Audi R8 first, then Kenney cars
        string[] carNames = { "Cars/AudiR8/r8", "Cars/sedan", "Cars/race", "Cars/suv", "Cars/truck" };

        foreach (string carName in carNames)
        {
            GameObject carPrefab = Resources.Load<GameObject>(carName);
            if (carPrefab != null)
            {
                GameObject car = Instantiate(carPrefab, new Vector3(0, 0.6f, -8), Quaternion.identity);
                car.name = "Car";

                // Scale to fit character (~1.8m tall), car roof should be waist height
                if (carName.Contains("AudiR8"))
                    car.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                else
                    car.transform.localScale = new Vector3(2f, 2f, 2f);

                // Add Rigidbody
                Rigidbody rb = car.AddComponent<Rigidbody>();
                rb.mass = 1200f;
                rb.linearDamping = 0.5f;
                rb.angularDamping = 2f;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                rb.centerOfMass = new Vector3(0, -0.3f, 0);

                // BoxCollider sized for car at 0.5 scale (~2.2m long, ~0.95m wide, ~0.6m tall)
                BoxCollider col = car.AddComponent<BoxCollider>();
                col.center = new Vector3(0, 0.3f, 0);
                col.size = new Vector3(0.95f, 0.6f, 2.2f);

                // Add CarController
                car.AddComponent<CarController>();

                // Headlights, taillights, damage
                CarFeatures features = car.AddComponent<CarFeatures>();
                features.maxHealth = 100f;

                Debug.Log($"Loaded car model: {carName}");
                return car;
            }
        }

        Debug.LogWarning("No car model found in Resources/Cars/, using primitive car");
        return null;
    }

    GameObject CreatePrimitiveCar()
    {
        // Car body
        GameObject car = GameObject.CreatePrimitive(PrimitiveType.Cube);
        car.name = "Car";
        car.transform.position = new Vector3(0, 0.6f, -8);
        car.transform.localScale = new Vector3(2, 1, 4);
        car.GetComponent<Renderer>().material.color = new Color(0.8f, 0.1f, 0.1f);

        // Car roof
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Roof";
        roof.transform.parent = car.transform;
        roof.transform.localPosition = new Vector3(0, 0.6f, -0.2f);
        roof.transform.localScale = new Vector3(0.9f, 0.5f, 0.5f);
        roof.GetComponent<Renderer>().material.color = new Color(0.7f, 0.1f, 0.1f);

        // Wheels
        Vector3[] wheelPos = {
            new Vector3(-0.9f, -0.3f, 1.2f),
            new Vector3(0.9f, -0.3f, 1.2f),
            new Vector3(-0.9f, -0.3f, -1.2f),
            new Vector3(0.9f, -0.3f, -1.2f)
        };
        foreach (Vector3 p in wheelPos)
        {
            GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = "Wheel";
            wheel.transform.parent = car.transform;
            wheel.transform.localPosition = p;
            wheel.transform.localScale = new Vector3(0.4f, 0.2f, 0.4f);
            wheel.GetComponent<Renderer>().material.color = Color.black;
            wheel.transform.Rotate(0, 0, 90);
        }

        // Add Rigidbody
        Rigidbody rb = car.AddComponent<Rigidbody>();
        rb.mass = 1200f;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 2f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        // Add CarController
        car.AddComponent<CarController>();

        // Headlights, taillights, damage
        CarFeatures features = car.AddComponent<CarFeatures>();
        features.maxHealth = 100f;

        return car;
    }

    GameObject CreatePerson(string name, Vector3 pos)
    {
        GameObject person = new GameObject(name);
        person.transform.position = pos;

        Color skinColor = new Color(0.9f, 0.75f, 0.6f);
        Color shirtColor = new Color(0.2f, 0.4f, 0.8f);
        Color pantsColor = new Color(0.2f, 0.2f, 0.3f);
        Color shoeColor = new Color(0.15f, 0.1f, 0.1f);

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.parent = person.transform;
        head.transform.localPosition = new Vector3(0, 1.7f, 0);
        head.transform.localScale = new Vector3(0.35f, 0.4f, 0.35f);
        head.GetComponent<Renderer>().material.color = skinColor;

        // Body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.parent = person.transform;
        body.transform.localPosition = new Vector3(0, 1.1f, 0);
        body.transform.localScale = new Vector3(0.5f, 0.7f, 0.3f);
        body.GetComponent<Renderer>().material.color = shirtColor;

        // Arms
        GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftArm.name = "LeftArm";
        leftArm.transform.parent = person.transform;
        leftArm.transform.localPosition = new Vector3(-0.4f, 1.1f, 0);
        leftArm.transform.localScale = new Vector3(0.15f, 0.6f, 0.15f);
        leftArm.GetComponent<Renderer>().material.color = shirtColor;

        GameObject rightArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightArm.name = "RightArm";
        rightArm.transform.parent = person.transform;
        rightArm.transform.localPosition = new Vector3(0.4f, 1.1f, 0);
        rightArm.transform.localScale = new Vector3(0.15f, 0.6f, 0.15f);
        rightArm.GetComponent<Renderer>().material.color = shirtColor;

        // Legs
        GameObject leftLeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftLeg.name = "LeftLeg";
        leftLeg.transform.parent = person.transform;
        leftLeg.transform.localPosition = new Vector3(-0.12f, 0.4f, 0);
        leftLeg.transform.localScale = new Vector3(0.18f, 0.6f, 0.18f);
        leftLeg.GetComponent<Renderer>().material.color = pantsColor;

        GameObject rightLeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightLeg.name = "RightLeg";
        rightLeg.transform.parent = person.transform;
        rightLeg.transform.localPosition = new Vector3(0.12f, 0.4f, 0);
        rightLeg.transform.localScale = new Vector3(0.18f, 0.6f, 0.18f);
        rightLeg.GetComponent<Renderer>().material.color = pantsColor;

        // Shoes
        GameObject leftShoe = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftShoe.name = "LeftShoe";
        leftShoe.transform.parent = person.transform;
        leftShoe.transform.localPosition = new Vector3(-0.12f, 0.08f, 0.05f);
        leftShoe.transform.localScale = new Vector3(0.2f, 0.1f, 0.25f);
        leftShoe.GetComponent<Renderer>().material.color = shoeColor;

        GameObject rightShoe = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightShoe.name = "RightShoe";
        rightShoe.transform.parent = person.transform;
        rightShoe.transform.localPosition = new Vector3(0.12f, 0.08f, 0.05f);
        rightShoe.transform.localScale = new Vector3(0.2f, 0.1f, 0.25f);
        rightShoe.GetComponent<Renderer>().material.color = shoeColor;

        // Hair
        GameObject hair = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hair.name = "Hair";
        hair.transform.parent = person.transform;
        hair.transform.localPosition = new Vector3(0, 1.95f, 0);
        hair.transform.localScale = new Vector3(0.38f, 0.12f, 0.38f);
        hair.GetComponent<Renderer>().material.color = new Color(0.15f, 0.1f, 0.05f);

        return person;
    }

    void CreateWall(string name, Vector3 pos, Vector3 scale, Color color)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().material.color = color;
    }

    void CreateParkedCar(Vector3 pos, Color color)
    {
        // Car body
        GameObject car = GameObject.CreatePrimitive(PrimitiveType.Cube);
        car.name = "ParkedCar";
        car.transform.position = pos;
        car.transform.localScale = new Vector3(2, 1, 4);
        car.GetComponent<Renderer>().material.color = color;

        // Roof
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Roof";
        roof.transform.parent = car.transform;
        roof.transform.localPosition = new Vector3(0, 0.6f, -0.2f);
        roof.transform.localScale = new Vector3(0.9f, 0.5f, 0.5f);
        roof.GetComponent<Renderer>().material.color = color * 0.85f;

        // Wheels
        Vector3[] wheelPos = {
            new Vector3(-0.9f, -0.3f, 1.2f),
            new Vector3(0.9f, -0.3f, 1.2f),
            new Vector3(-0.9f, -0.3f, -1.2f),
            new Vector3(0.9f, -0.3f, -1.2f)
        };
        foreach (Vector3 p in wheelPos)
        {
            GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = "Wheel";
            wheel.transform.parent = car.transform;
            wheel.transform.localPosition = p;
            wheel.transform.localScale = new Vector3(0.4f, 0.2f, 0.4f);
            wheel.GetComponent<Renderer>().material.color = Color.black;
            wheel.transform.Rotate(0, 0, 90);
        }

        // Rigidbody + collider
        Rigidbody rb = car.AddComponent<Rigidbody>();
        rb.mass = 1200f;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 2f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        BoxCollider col = car.AddComponent<BoxCollider>();
        col.center = new Vector3(0, 0.3f, 0);
        col.size = new Vector3(1.8f, 1f, 4f);

        car.AddComponent<CarController>();
    }

    void FixTPose(GameObject person)
    {
        // Only used for models without animations
        // Mixamo model bone names - rotate arms down from T-pose
        string[] leftArmBones = { "mixamorig_LeftArm", "mixamorig_LeftForeArm", "mixamorig_LeftHand" };
        string[] rightArmBones = { "mixamorig_RightArm", "mixamorig_RightForeArm", "mixamorig_RightHand" };

        foreach (string boneName in leftArmBones)
        {
            Transform bone = FindChildRecursive(person.transform, boneName);
            if (bone != null) bone.Rotate(0, 0, 80);
        }

        foreach (string boneName in rightArmBones)
        {
            Transform bone = FindChildRecursive(person.transform, boneName);
            if (bone != null) bone.Rotate(0, 0, -80);
        }
    }

    Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            Transform found = FindChildRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
