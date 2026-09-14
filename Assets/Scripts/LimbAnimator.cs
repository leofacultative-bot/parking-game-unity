using UnityEngine;

/// <summary>
/// Procedural walk animation using bone rotations.
/// Finds bones by partial name matching (works with any humanoid model).
/// No Animator or animation clips needed.
/// </summary>
public class LimbAnimator : MonoBehaviour
{
    // Bone references
    private Transform hips, spine, head;
    private Transform leftUpperArm, leftLowerArm;
    private Transform rightUpperArm, rightLowerArm;
    private Transform leftUpperLeg, leftLowerLeg;
    private Transform rightUpperLeg, rightLowerLeg;

    // Stored start rotations
    private Quaternion hipsStart, spineStart, headStart;
    private Quaternion lArmStart, lForeArmStart, rArmStart, rForeArmStart;
    private Quaternion lLegStart, lShinStart, rLegStart, rShinStart;

    private float walkCycle = 0f;
    private bool bonesFound = false;

    void Start()
    {
        FindBones();
        StoreStartRotations();
    }

    void FindBones()
    {
        // Search for bones by partial name (works with Mixamo, downloaded models, etc.)
        hips = FindBoneContaining("Hips");
        spine = FindBoneContaining("Spine");
        head = FindBoneContaining("Head");

        leftUpperArm = FindBoneContaining("LeftArm");
        leftLowerArm = FindBoneContaining("LeftForeArm");
        rightUpperArm = FindBoneContaining("RightArm");
        rightLowerArm = FindBoneContaining("RightForeArm");

        leftUpperLeg = FindBoneContaining("LeftUpLeg");
        leftLowerLeg = FindBoneContaining("LeftLeg");
        rightUpperLeg = FindBoneContaining("RightUpLeg");
        rightLowerLeg = FindBoneContaining("RightLeg");

        // Check what we found
        int found = 0;
        if (hips != null) found++;
        if (leftUpperArm != null) found++;
        if (rightUpperArm != null) found++;
        if (leftUpperLeg != null) found++;
        if (rightUpperLeg != null) found++;

        bonesFound = (found >= 3); // Need at least hips + 2 limbs

        if (bonesFound)
            Debug.Log($"LimbAnimator: Found {found} key bones. Hips={hips?.name}, LArm={leftUpperArm?.name}, RArm={rightUpperArm?.name}, LLeg={leftUpperLeg?.name}, RLeg={rightUpperLeg?.name}");
        else
            Debug.LogWarning($"LimbAnimator: Only found {found} bones. Skipping animation.");
    }

    void StoreStartRotations()
    {
        if (hips != null) hipsStart = hips.localRotation;
        if (spine != null) spineStart = spine.localRotation;
        if (head != null) headStart = head.localRotation;
        if (leftUpperArm != null) lArmStart = leftUpperArm.localRotation;
        if (leftLowerArm != null) lForeArmStart = leftLowerArm.localRotation;
        if (rightUpperArm != null) rArmStart = rightUpperArm.localRotation;
        if (rightLowerArm != null) rForeArmStart = rightLowerArm.localRotation;
        if (leftUpperLeg != null) lLegStart = leftUpperLeg.localRotation;
        if (leftLowerLeg != null) lShinStart = leftLowerLeg.localRotation;
        if (rightUpperLeg != null) rLegStart = rightUpperLeg.localRotation;
        if (rightLowerLeg != null) rShinStart = rightLowerLeg.localRotation;
    }

    void Update()
    {
        if (!bonesFound) return;

        PlayerController pc = GetComponent<PlayerController>();
        if (pc == null || pc.IsInCar) return;

        float speed = GetMoveSpeed();
        bool isMoving = speed > 0.1f;

        if (!isMoving)
        {
            ReturnToIdle();
            walkCycle = 0;
            return;
        }

        // Advance walk cycle
        float cycleSpeed = Mathf.Lerp(5f, 10f, speed / 8f);
        walkCycle += Time.deltaTime * cycleSpeed;
        if (walkCycle > Mathf.PI * 2f) walkCycle -= Mathf.PI * 2f;

        float intensity = Mathf.Clamp01(speed / 6f);
        Animate(intensity);
    }

    float GetMoveSpeed()
    {
        float speed = 0f;

        float v = Mathf.Abs(Input.GetAxis("Vertical"));
        float h = Mathf.Abs(Input.GetAxis("Horizontal"));
        speed = Mathf.Max(v, h);

        if (MobileJoystick.Instance != null)
        {
            float joySpeed = MobileJoystick.Instance.InputVector.magnitude;
            if (joySpeed > speed) speed = joySpeed;
        }

        return speed * 5f;
    }

    void Animate(float intensity)
    {
        float sin = Mathf.Sin(walkCycle);
        float cos = Mathf.Cos(walkCycle);

        // --- LEGS ---
        float legSwing = intensity * 45f;
        float kneeBend = intensity * 30f;

        if (leftUpperLeg != null)
            leftUpperLeg.localRotation = lLegStart * Quaternion.Euler(sin * legSwing, 0, 0);
        if (rightUpperLeg != null)
            rightUpperLeg.localRotation = rLegStart * Quaternion.Euler(-sin * legSwing, 0, 0);

        if (leftLowerLeg != null)
        {
            float lk = sin < 0 ? -Mathf.Abs(sin) * kneeBend : 0;
            leftLowerLeg.localRotation = lShinStart * Quaternion.Euler(lk, 0, 0);
        }
        if (rightLowerLeg != null)
        {
            float rk = -sin < 0 ? -Mathf.Abs(-sin) * kneeBend : 0;
            rightLowerLeg.localRotation = rShinStart * Quaternion.Euler(rk, 0, 0);
        }

        // --- ARMS (opposite to legs) ---
        float armSwing = intensity * 40f;
        float elbowBend = 15f + intensity * 25f;

        if (leftUpperArm != null)
            leftUpperArm.localRotation = lArmStart * Quaternion.Euler(-sin * armSwing, 0, 0);
        if (rightUpperArm != null)
            rightUpperArm.localRotation = rArmStart * Quaternion.Euler(sin * armSwing, 0, 0);

        if (leftLowerArm != null)
            leftLowerArm.localRotation = lForeArmStart * Quaternion.Euler(-elbowBend, 0, 0);
        if (rightLowerArm != null)
            rightLowerArm.localRotation = rForeArmStart * Quaternion.Euler(-elbowBend, 0, 0);

        // --- SPINE (subtle twist) ---
        float spineTwist = intensity * 3f;
        if (spine != null)
            spine.localRotation = spineStart * Quaternion.Euler(0, sin * spineTwist, sin * 2f);

        // --- HEAD (stays steady) ---
        if (head != null)
            head.localRotation = headStart;
    }

    void ReturnToIdle()
    {
        float lerp = Time.deltaTime * 5f;

        if (hips != null) hips.localRotation = Quaternion.Slerp(hips.localRotation, hipsStart, lerp);
        if (spine != null) spine.localRotation = Quaternion.Slerp(spine.localRotation, spineStart, lerp);
        if (head != null) head.localRotation = Quaternion.Slerp(head.localRotation, headStart, lerp);
        if (leftUpperArm != null) leftUpperArm.localRotation = Quaternion.Slerp(leftUpperArm.localRotation, lArmStart, lerp);
        if (leftLowerArm != null) leftLowerArm.localRotation = Quaternion.Slerp(leftLowerArm.localRotation, lForeArmStart, lerp);
        if (rightUpperArm != null) rightUpperArm.localRotation = Quaternion.Slerp(rightUpperArm.localRotation, rArmStart, lerp);
        if (rightLowerArm != null) rightLowerArm.localRotation = Quaternion.Slerp(rightLowerArm.localRotation, rForeArmStart, lerp);
        if (leftUpperLeg != null) leftUpperLeg.localRotation = Quaternion.Slerp(leftUpperLeg.localRotation, lLegStart, lerp);
        if (leftLowerLeg != null) leftLowerLeg.localRotation = Quaternion.Slerp(leftLowerLeg.localRotation, lShinStart, lerp);
        if (rightUpperLeg != null) rightUpperLeg.localRotation = Quaternion.Slerp(rightUpperLeg.localRotation, rLegStart, lerp);
        if (rightLowerLeg != null) rightLowerLeg.localRotation = Quaternion.Slerp(rightLowerLeg.localRotation, rShinStart, lerp);
    }

    Transform FindBoneContaining(string name)
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
}
