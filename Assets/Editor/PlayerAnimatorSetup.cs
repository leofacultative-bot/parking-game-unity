using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
#endif

/// <summary>
/// Creates a proper walking animation and Animator Controller for the player.
/// Menu: Tools > Create Player Animator
/// </summary>
public class PlayerAnimatorSetup
{
#if UNITY_EDITOR
    [MenuItem("Tools/Create Player Animator")]
    public static void CreateAnimator()
    {
        string animDir = "Assets/Resources/Animations";

        // === 1. Create a walking animation clip ===
        AnimationClip walkClip = new AnimationClip();
        walkClip.name = "WalkCycle";
        walkClip.frameRate = 30;

        // Simple walk cycle: leg forward/back, arm swing
        // Left leg forward
        AddCurve(walkClip, "mixamorig:Hips/mixamorig:LeftUpLeg", "localEulerAngles.z", 
            new Keyframe[] { new Keyframe(0, 15), new Keyframe(0.5f, -15), new Keyframe(1, 15) });
        // Right leg forward (opposite)
        AddCurve(walkClip, "mixamorig:Hips/mixamorig:RightUpLeg", "localEulerAngles.z", 
            new Keyframe[] { new Keyframe(0, -15), new Keyframe(0.5f, 15), new Keyframe(1, -15) });
        // Left arm swing (opposite to left leg)
        AddCurve(walkClip, "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:LeftShoulder/mixamorig:LeftArm", "localEulerAngles.z", 
            new Keyframe[] { new Keyframe(0, -12), new Keyframe(0.5f, 12), new Keyframe(1, -12) });
        // Right arm swing
        AddCurve(walkClip, "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:RightShoulder/mixamorig:RightArm", "localEulerAngles.z", 
            new Keyframe[] { new Keyframe(0, 12), new Keyframe(0.5f, -12), new Keyframe(1, 12) });
        // Hip bounce
        AddCurve(walkClip, "mixamorig:Hips", "localPosition.y", 
            new Keyframe[] { new Keyframe(0, 0), new Keyframe(0.25f, 0.02f), new Keyframe(0.5f, 0), new Keyframe(0.75f, 0.02f), new Keyframe(1, 0) });
        // Spine twist
        AddCurve(walkClip, "mixamorig:Hips/mixamorig:Spine", "localEulerAngles.y", 
            new Keyframe[] { new Keyframe(0, 3), new Keyframe(0.5f, -3), new Keyframe(1, 3) });

        AssetDatabase.CreateAsset(walkClip, animDir + "/WalkCycle.anim");

        // === 2. Create idle animation (first frame of walk) ===
        AnimationClip idleClip = new AnimationClip();
        idleClip.name = "Idle";
        idleClip.frameRate = 30;
        // Just keep hips at rest
        AddCurve(idleClip, "mixamorig:Hips", "localPosition.y", 
            new Keyframe[] { new Keyframe(0, 0) });
        AssetDatabase.CreateAsset(idleClip, animDir + "/Idle.anim");

        // === 3. Create Animator Controller ===
        string controllerPath = animDir + "/PlayerAnimator.controller";
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

        AnimatorControllerLayer layer0 = controller.layers[0];

        AnimatorState idleState = layer0.stateMachine.AddState("Idle", new Vector3(0, 0, 0));
        idleState.motion = idleClip;

        AnimatorState walkState = layer0.stateMachine.AddState("Walk", new Vector3(250, 0, 0));
        walkState.motion = walkClip;
        walkState.speed = 1f;

        AnimatorState runState = layer0.stateMachine.AddState("Run", new Vector3(500, 0, 0));
        runState.motion = walkClip;
        runState.speed = 1.6f;

        layer0.stateMachine.defaultState = idleState;

        // Transitions
        AnimatorStateTransition t;
        t = idleState.AddTransition(walkState);
        t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        t.hasExitTime = false;
        t.duration = 0.15f;

        t = walkState.AddTransition(idleState);
        t.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        t.hasExitTime = false;
        t.duration = 0.15f;

        t = walkState.AddTransition(runState);
        t.AddCondition(AnimatorConditionMode.Greater, 0.6f, "Speed");
        t.hasExitTime = false;
        t.duration = 0.15f;

        t = runState.AddTransition(walkState);
        t.AddCondition(AnimatorConditionMode.Less, 0.6f, "Speed");
        t.hasExitTime = false;
        t.duration = 0.15f;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Player Animator created: WalkCycle.anim, Idle.anim, PlayerAnimator.controller");
        EditorUtility.DisplayDialog("Done",
            "Created:\n- WalkCycle.anim (procedural walk)\n- Idle.anim\n- PlayerAnimator.controller\n\n" +
            "The player will automatically use these.", "OK");
    }

    static void AddCurve(AnimationClip clip, string path, string property, Keyframe[] keys)
    {
        // Map property names to binding types
        string[] parts = property.Split('.');
        string prop = parts[parts.Length - 1];

        EditorCurveBinding binding = new EditorCurveBinding();
        binding.path = path;
        binding.type = typeof(Transform);

        if (prop == "localPosition.y")
            binding.propertyName = "m_LocalPosition.y";
        else if (prop == "localEulerAngles.z")
            binding.propertyName = "m_LocalEulerAngles.z";
        else if (prop == "localEulerAngles.y")
            binding.propertyName = "m_LocalEulerAngles.y";
        else if (prop == "localEulerAngles.x")
            binding.propertyName = "m_LocalEulerAngles.x";
        else
            binding.propertyName = prop;

        AnimationCurve curve = new AnimationCurve(keys);
        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }
#endif
}
