using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
#endif

/// <summary>
/// Creates Animator Controller using the Walking.fbx animation clip.
/// Both models must be Humanoid (animationType=3) for retargeting to work.
/// Menu: Tools > Create Player Animator
/// </summary>
public class PlayerAnimatorSetup
{
#if UNITY_EDITOR
    [MenuItem("Tools/Create Player Animator")]
    public static void CreateAnimator()
    {
        string animDir = "Assets/Resources/Animations";

        // Find Walking.fbx animation clips (must be reimported as Humanoid)
        string fbxGUID = AssetDatabase.FindAssets("Walking t:Model")
            .FirstOrDefault(g => AssetDatabase.GUIDToAssetPath(g).Contains("Animations"));

        if (string.IsNullOrEmpty(fbxGUID))
        {
            Debug.LogError("Walking.fbx not found in Animations folder");
            return;
        }

        string fbxPath = AssetDatabase.GUIDToAssetPath(fbxGUID);
        Debug.Log($"Found: {fbxPath}");

        // Load all sub-assets
        Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        AnimationClip[] clips = subAssets.OfType<AnimationClip>().ToArray();
        Debug.Log($"Animation clips found: {clips.Length}");
        foreach (var c in clips)
            Debug.Log($"  Clip: {c.name}, length={c.length:F2}s, frames={c.frameRate}");

        if (clips.Length == 0)
        {
            Debug.LogError("No animation clips found. Make sure Walking.fbx is set to Humanoid (animationType=3) in its import settings.");
            return;
        }

        AnimationClip walkClip = clips[0];
        Debug.Log($"Using clip: {walkClip.name}");

        // Delete old procedural clips if they exist
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(animDir + "/WalkCycle.anim") != null)
            AssetDatabase.DeleteAsset(animDir + "/WalkCycle.anim");
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(animDir + "/Idle.anim") != null)
            AssetDatabase.DeleteAsset(animDir + "/Idle.anim");

        // Create Animator Controller
        string controllerPath = animDir + "/PlayerAnimator.controller";
        if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath) != null)
            AssetDatabase.DeleteAsset(controllerPath);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

        AnimatorControllerLayer layer0 = controller.layers[0];

        // Idle = walk clip at speed 0 (freeze on first frame)
        AnimatorState idleState = layer0.stateMachine.AddState("Idle", new Vector3(0, 0, 0));
        idleState.motion = walkClip;
        idleState.speed = 0f;

        // Walk = walk clip at normal speed
        AnimatorState walkState = layer0.stateMachine.AddState("Walk", new Vector3(250, 0, 0));
        walkState.motion = walkClip;
        walkState.speed = 1f;

        // Run = walk clip at faster speed
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

        Debug.Log("Done! Animator Controller created with real walking animation.");
    }
#endif
}
