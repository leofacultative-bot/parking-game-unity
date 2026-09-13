using UnityEditor;
using UnityEditor.SceneManagement;

public class OpenSceneOnLoad
{
    [InitializeOnLoadMethod]
    static void OpenParkingLot()
    {
        string scenePath = "Assets/Scenes/ParkingLot.unity";
        if (System.IO.File.Exists(scenePath))
        {
            if (EditorSceneManager.GetActiveScene().name != "ParkingLot")
            {
                EditorSceneManager.OpenScene(scenePath);
            }
        }
    }
}
