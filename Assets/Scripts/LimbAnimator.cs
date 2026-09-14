using UnityEngine;

/// <summary>
/// Procedural humanoid walk/run animation.
/// Swings arms, legs, sways hips, bounces body.
/// Works without any Animator Controller or animation clips.
/// </summary>
public class LimbAnimator : MonoBehaviour
{
    // Bone references
    private Transform spine, head;
    private Transform leftUpperArm, leftLowerArm;
    private Transform rightUpperArm, rightLowerArm;
    private Transform leftUpperLeg, leftLowerLeg;
    private Transform rightUpperLeg, rightLowerLeg;

    // Stored start rotations
    private Quaternion spineStart, headStart;
    private Quaternion leftUpperArmStart, leftLowerArmStart;
    private Quaternion rightUpperArmStart, rightLowerArmStart;
    private Quaternion leftUpperLegStart, leftLowerLegStart;
    private Quaternion rightUpperLegStart, rightLowerLegStart;

    private float walkCycle = 0f;
    private float prevSpeed = 0f;
    private PlayerController pc;

    void Start()
    {
        pc = GetComponent<PlayerController>();

        // Find bones by Mixamo naming convention
        spine = FindBone("mixamorig_Spine");
        head = FindBone("mixamorig_Head");

        leftUpperArm = FindBone("mixamorig_LeftArm");
        leftLowerArm = FindBone("mixamorig_LeftForeArm");
        rightUpperArm = FindBone("mixamorig_RightArm");
        rightLowerArm = FindBone("mixamorig_RightForeArm");

        leftUpperLeg = FindBone("mixamorig_LeftUpLeg");
        leftLowerLeg = FindBone("mixamorig_LeftLeg");
        rightUpperLeg = FindBone("mixamorig_RightUpLeg");
        rightLowerLeg = FindBone("mixamorig_RightLeg");

        // Log what we found
        Debug.Log($"LimbAnimator bones: spine={spine != null}, head={head != null}, " +
                  $"lArm={leftUpperArm != null}, rArm={rightUpperArm != null}, " +
                  $"lLeg={leftUpperLeg != null}, rLeg={rightUpperLeg != null}");

        // If Mixamo bones not found, dump all bone names to help debug
        if (spine == null && head == null)
        {
            Debug.Log("LimbAnimator: No Mixamo bones found. Dumping hierarchy:");
            DumpHierarchy(transform, 0);
        }

        // Store start rotations
        if (spine != null) spineStart = spine.localRotation;
        if (head != null) headStart = head.localRotation;
        if (leftUpperArm != null) leftUpperArmStart = leftUpperArm.localRotation;
        if (leftLowerArm != null) leftLowerArmStart = leftLowerArm.localRotation;
        if (rightUpperArm != null) rightUpperArmStart = rightUpperArm.localRotation;
        if (rightLowerArm != null) rightLowerArmStart = rightLowerArm.localRotation;
        if (leftUpperLeg != null) leftUpperLegStart = leftUpperLeg.localRotation;
        if (leftLowerLeg != null) leftLowerLegStart = leftLowerLeg.localRotation;
        if (rightUpperLeg != null) rightUpperLegStart = rightUpperLeg.localRotation;
        if (rightLowerLeg != null) rightLowerLegStart = rightLowerLeg.localRotation;
    }

    void Update()
    {
        if (pc == null || pc.IsInCar) return;

        // Get movement speed
        float speed = GetMoveSpeed();
        bool isMoving = speed > 0.1f;

        if (!isMoving)
        {
            ReturnToIdle();
            walkCycle = 0;
            prevSpeed = 0;
            return;
        }

        // Advance walk cycle
        float cycleSpeed = Mathf.Lerp(5f, 10f, speed / 8f);
        walkCycle += Time.deltaTime * cycleSpeed;
        if (walkCycle > Mathf.PI * 2f) walkCycle -= Mathf.PI * 2f;

        float intensity = Mathf.Clamp01(speed / 6f); // 0=idle, 1=full run

        AnimateLimbs(intensity);
    }

    float GetMoveSpeed()
    {
        float speed = 0f;

        // Keyboard
        float v = Mathf.Abs(Input.GetAxis("Vertical"));
        float h = Mathf.Abs(Input.GetAxis("Horizontal"));
        speed = Mathf.Max(v, h);

        // Joystick
        if (MobileJoystick.Instance != null)
        {
            float joySpeed = MobileJoystick.Instance.InputVector.magnitude;
            if (joySpeed > speed) speed = joySpeed;
        }

        return speed * 5f; // Scale up for animation speed
    }

    void AnimateLimbs(float intensity)
    {
        float sin = Mathf.Sin(walkCycle);
        float cos = Mathf.Cos(walkCycle);

        // --- LEGS ---
        float legSwing = intensity * 45f;
        float kneeBend = intensity * 30f;

        if (leftUpperLeg != null)
            leftUpperLeg.localRotation = leftUpperLegStart * Quaternion.Euler(sin * legSwing, 0, 0);
        if (rightUpperLeg != null)
            rightUpperLeg.localRotation = rightUpperLegStart * Quaternion.Euler(-sin * legSwing, 0, 0);

        // Knees bend backward when leg goes back
        if (leftLowerLeg != null)
        {
            float leftKnee = sin < 0 ? -Mathf.Abs(sin) * kneeBend : 0;
            leftLowerLeg.localRotation = leftLowerLegStart * Quaternion.Euler(leftKnee, 0, 0);
        }
        if (rightLowerLeg != null)
        {
            float rightKnee = -sin < 0 ? -Mathf.Abs(-sin) * kneeBend : 0;
            rightLowerLeg.localRotation = rightLowerLegStart * Quaternion.Euler(rightKnee, 0, 0);
        }

        // --- ARMS (opposite to legs) ---
        float armSwing = intensity * 40f;
        float elbowBend = 15f + intensity * 25f;

        if (leftUpperArm != null)
            leftUpperArm.localRotation = leftUpperArmStart * Quaternion.Euler(-sin * armSwing, 0, 0);
        if (rightUpperArm != null)
            rightUpperArm.localRotation = rightUpperArmStart * Quaternion.Euler(sin * armSwing, 0, 0);

        // Elbows always slightly bent, more when swinging
        if (leftLowerArm != null)
            leftLowerArm.localRotation = leftLowerArmStart * Quaternion.Euler(-elbowBend, 0, 0);
        if (rightLowerArm != null)
            rightLowerArm.localRotation = rightLowerArmStart * Quaternion.Euler(-elbowBend, 0, 0);

        // --- SPINE (subtle twist) ---
        float spineTwist = intensity * 3f;
        if (spine != null)
            spine.localRotation = spineStart * Quaternion.Euler(0, sin * spineTwist, sin * 2f);

        // --- HEAD (stays steady) ---
        if (head != null)
            head.localRotation = headStart;

        // --- BODY BOUNCE (on a child pivot, not the root) ---
        // Skip — CharacterController handles position
    }

    void ReturnToIdle()
    {
        float lerp = Time.deltaTime * 5f;

        if (spine != null) spine.localRotation = Quaternion.Slerp(spine.localRotation, spineStart, lerp);
        if (head != null) head.localRotation = Quaternion.Slerp(head.localRotation, headStart, lerp);
        if (leftUpperArm != null) leftUpperArm.localRotation = Quaternion.Slerp(leftUpperArm.localRotation, leftUpperArmStart, lerp);
        if (leftLowerArm != null) leftLowerArm.localRotation = Quaternion.Slerp(leftLowerArm.localRotation, leftLowerArmStart, lerp);
        if (rightUpperArm != null) rightUpperArm.localRotation = Quaternion.Slerp(rightUpperArm.localRotation, rightUpperArmStart, lerp);
        if (rightLowerArm != null) rightLowerArm.localRotation = Quaternion.Slerp(rightLowerArm.localRotation, rightLowerArmStart, lerp);
        if (leftUpperLeg != null) leftUpperLeg.localRotation = Quaternion.Slerp(leftUpperLeg.localRotation, leftUpperLegStart, lerp);
        if (leftLowerLeg != null) leftLowerLeg.localRotation = Quaternion.Slerp(leftLowerLeg.localRotation, leftLowerLegStart, lerp);
        if (rightUpperLeg != null) rightUpperLeg.localRotation = Quaternion.Slerp(rightUpperLeg.localRotation, rightUpperLegStart, lerp);
        if (rightLowerLeg != null) rightLowerLeg.localRotation = Quaternion.Slerp(rightLowerLeg.localRotation, rightLowerLegStart, lerp);

        // Skip position reset — CharacterController handles position
    }

    Transform FindBone(string name)
    {
        return FindBoneRecursive(transform, name);
    }

    Transform FindBoneRecursive(Transform parent, string name)
    {
        if (parent.name.Contains(name)) return parent;
        foreach (Transform child in parent)
        {
            Transform found = FindBoneRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }

    void DumpHierarchy(Transform t, int depth)
    {
        string indent = new string(' ', depth * 2);
        Debug.Log($"{indent}{t.name}");
        foreach (Transform child in t)
            DumpHierarchy(child, depth + 1);
    }
}
