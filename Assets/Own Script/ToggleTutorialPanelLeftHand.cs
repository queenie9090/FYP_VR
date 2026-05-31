using UnityEngine;

public class ToggleTutorialPanelLeftHand : MonoBehaviour
{
    public void ToggleTutorialPanel()
    {
        if (this.gameObject != null)
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }
}
