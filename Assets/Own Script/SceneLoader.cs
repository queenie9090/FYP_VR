using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneToLoad;

    public void LoadScene(string name)
    { 
        SceneManager.LoadScene(name);
    }
}
