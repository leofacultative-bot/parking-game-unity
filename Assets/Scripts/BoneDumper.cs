using UnityEngine;

/// <summary>
/// Debug script: dumps all child transform names of this object to the log.
/// Attach to the player, press Play, check Console.
/// </summary>
public class BoneDumper : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== BONE HIERARCHY DUMP ===");
        DumpBone(transform, 0);
        Debug.Log("=== END DUMP ===");
    }

    void DumpBone(Transform t, int depth)
    {
        string indent = new string(' ', depth * 2);
        Debug.Log($"{indent}[{depth}] {t.name}");
        foreach (Transform child in t)
            DumpBone(child, depth + 1);
    }
}
