using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

/// <summary>
/// Editor script that creates an Animator Controller with walk/idle/run animations.
/// Run from menu: Tools > Create Player Animator
/// </summary>
public class PlayerAnimatorSetup
{
#if UNITY_EDITOR
    [MenuItem("Tools/Create Player Animator")]
    public static void CreateAnimator()
    {
        // Create Animator Controller
        string path = "Assets/Resources/Models/Ripley/PlayerAnimator.controller";
        
        // Ensure directory exists
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Models/Ripley"))
        {
            Debug.LogError("Ripley folder not found");
            return;
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        // Add parameters
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);

        // Create states

        // Create idle clip
        AnimationClip idleClip = new AnimationClip();
        idleClip.name = "Idle";
        idleClip.frameRate = 30;

        // Create walk clip
        AnimationClip walkClip = new AnimationClip();
        walkClip.name = "Walk";
        walkClip.frameRate = 30;

        // Create run clip
        AnimationClip runClip = new AnimationClip();
        runClip.name = "Run";
        runClip.frameRate = 30;

        // Save clips as assets
        AssetDatabase.CreateAsset(idleClip, "Assets/Resources/Models/Ripley/Idle.anim");
        AssetDatabase.CreateAsset(walkClip, "Assets/Resources/Models/Ripley/Walk.anim");
        AssetDatabase.CreateAsset(runClip, "Assets/Resources/Models/Ripley/Run.anim");

        // Add states to state machine
        AnimatorControllerLayer layer0 = controller.layers[0];
        AnimatorState idleState = layer0.stateMachine.AddState("Idle", new Vector3(0, 0, 0));
        idleState.motion = idleClip;
        idleState.speed = 1f;

        AnimatorState walkState = layer0.stateMachine.AddState("Walk", new Vector3(250, 0, 0));
        walkState.motion = walkClip;
        walkState.speed = 1f;

        AnimatorState runState = layer0.stateMachine.AddState("Run", new Vector3(500, 0, 0));
        runState.motion = runClip;
        runState.speed = 1f;

        // Set default state
        layer0.stateMachine.defaultState = idleState;

        // Transitions
        AnimatorStateTransition idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.15f;

        AnimatorStateTransition walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.15f;

        AnimatorStateTransition walkToRun = walkState.AddTransition(runState);
        walkToRun.AddCondition(AnimatorConditionMode.Greater, 0.6f, "Speed");
        walkToRun.hasExitTime = false;
        walkToRun.duration = 0.15f;

        AnimatorStateTransition runToWalk = runState.AddTransition(walkState);
        runToWalk.AddCondition(AnimatorConditionMode.Less, 0.6f, "Speed");
        runToWalk.hasExitTime = false;
        runToWalk.duration = 0.15f;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Player Animator Controller created at: " + path);
        EditorUtility.DisplayDialog("Done", "Animator Controller created!\n\nAssign it to the player's Animator component.\nStates: Idle, Walk, Run\nParameter: Speed (float)", "OK");
    }
#endif
}
