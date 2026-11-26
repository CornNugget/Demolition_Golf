using UnityEngine;

public class LevelOrder : MonoBehaviour
{
    public static readonly string[] Scenes =
    {
        "TutorialScene",
        "GoalScene",
        "Tutorial2Scene",
        "Level1",
        "Level0",
        "hospital_scene",
    };

    // Returns the level after this one, or null if it's the last level (or not a level at all)
    public static string GetNext(string currentScene)
    {
        int index = System.Array.IndexOf(Scenes, currentScene);
        if (index < 0 || index >= Scenes.Length - 1) return null;
        return Scenes[index + 1];
    }    
}
