using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
#endif

/// <summary>
/// Creates Animator Controller from the Walking.fbx animation.
/// Menu: Tools > Create Player Animator
/// </summary>
public class PlayerAnimatorSetup
{
#if UNITY_EDITOR
    [MenuItem("Tools/Create Player Animator")]
    public static void CreateAnimator()
    {
        string controllerPath = "Assets/Resources/Animations/PlayerAnimator.controller";

        // Find the Walking FBX
        string fbxGUID = AssetDatabase.FindAssets("Walking t:Model")
            .FirstOrDefault();

        if (fbxGUID == null)
        {
            Debug.LogError("Walking.fbx not found in Assets/Resources/Animations/");
            return;
        }

        string fbxPath = AssetDatabase.GUIDToAssetPath(fbxGUID);
        Debug.Log($"Found Walking FBX at: {fbxPath}");

        // Load all sub-assets (animation clips) from the FBX
        Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        AnimationClip[] clips = subAssets.OfType<AnimationClip>().ToArray();

        Debug.Log($"Found {clips.Length} animation clips:");
        foreach (var c in clips)
            Debug.Log($"  - {c.name} (frames: {c.length * c.frameRate:F0})");

        if (clips.Length == 0)
        {
            Debug.LogError("No animation clips found in Walking.fbx. Make sure it has animations enabled in Import Settings.");
            return;
        }

        AnimationClip walkClip = clips[0]; // Use first clip (usually the walk)

        // Create Animator Controller
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // Add parameter
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

        // Get the default layer
        AnimatorControllerLayer layer0 = controller.layers[0];

        // Create states
        AnimatorState idleState = layer0.stateMachine.AddState("Idle", new Vector3(0, 0, 0));
        idleState.motion = walkClip;
        idleState.speed = 0f; // Freeze on first frame = idle pose

        AnimatorState walkState = layer0.stateMachine.AddState("Walk", new Vector3(250, 0, 0));
        walkState.motion = walkClip;
        walkState.speed = 1f;

        AnimatorState runState = layer0.stateMachine.AddState("Run", new Vector3(500, 0, 0));
        runState.motion = walkClip;
        runState.speed = 1.5f; // Faster = running

        // Set default
        layer0.stateMachine.defaultState = idleState;

        // Transitions
        AnimatorStateTransition t;

        // Idle -> Walk
        t = idleState.AddTransition(walkState);
        t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        t.hasExitTime = false;
        t.duration = 0.15f;

        // Walk -> Idle
        t = walkState.AddTransition(idleState);
        t.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        t.hasExitTime = false;
        t.duration = 0.15f;

        // Walk -> Run
        t = walkState.AddTransition(runState);
        t.AddCondition(AnimatorConditionMode.Greater, 0.6f, "Speed");
        t.hasExitTime = false;
        t.duration = 0.15f;

        // Run -> Walk
        t = runState.AddTransition(walkState);
        t.AddCondition(AnimatorConditionMode.Less, 0.6f, "Speed");
        t.hasExitTime = false;
        t.duration = 0.15f;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Animator Controller created at: {controllerPath}");
        EditorUtility.DisplayDialog("Done",
            $"Animator Controller created!\n\n" +
            $"Clip: {walkClip.name}\n" +
            $"States: Idle, Walk, Run\n" +
            $"Parameter: Speed (float)\n\n" +
            "The controller will be auto-assigned to the player on next play.",
            "OK");
    }
#endif
}
