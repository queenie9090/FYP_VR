using UnityEngine;

public class WorldSetting : MonoBehaviour
{
    [SerializeField, TextArea] private string gameStory;
    [SerializeField] private string gameWorld;

    public string GetPrompt()
    {
        return $"Game Story: {gameStory}\n" +
               $"Game World: {gameWorld}\n";
        // testing 123
        // testing hello world
    }
}

