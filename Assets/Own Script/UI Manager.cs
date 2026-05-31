using UnityEditor;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject startGameMenu;
    public GameObject tutorialMenu;
    public GameObject settingsMenu;
    public GameObject aboutUsMenu;
    public GameObject scenarioCustomizationMenu;
    //public GameObject exitGameMenu;

    public void OpenPanel(GameObject panelToOpen)
    {
        mainMenu.SetActive(false);
        startGameMenu.SetActive(false);
        tutorialMenu.SetActive(false);
        settingsMenu.SetActive(false);
        aboutUsMenu.SetActive(false);
        scenarioCustomizationMenu.SetActive(false);
        //exitGameMenu.SetActive(false);

        panelToOpen.SetActive(true);
    }

    public void ClosePanel() 
    {
        mainMenu.SetActive(true);
        startGameMenu.SetActive(false);
        tutorialMenu.SetActive(false);
        settingsMenu.SetActive(false);
        aboutUsMenu.SetActive(false);
        scenarioCustomizationMenu.SetActive(false);
        //exitGameMenu.SetActive(false);
    }

    public void CloseScenarioCustomizationPanel(GameObject SpecificPanel)
    {
        settingsMenu.SetActive(true);
        SpecificPanel.SetActive(false);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
    }
}
