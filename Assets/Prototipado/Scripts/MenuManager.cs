using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    string actualScene;

    public GameObject pauseCanvas;

    public GameObject tutorialCanvas;

    public PelotaPrototipo inputManager;

    string buttonName;

    public void Start()
    {
        if (tutorialCanvas != null)
        {
            inputManager.mouseMovement.Disable();
            inputManager.mouseClick.Disable();
            inputManager.mouseRClick.Disable();
            inputManager.letterRClick.Disable();
        }        
    }

    public void NextLevel()
    {
        actualScene = SceneManager.GetActiveScene().name;

        switch (actualScene)
        {
            case "Nivel1":
                SceneManager.LoadScene("Nivel2");
                break;
            case "Nivel2":
                SceneManager.LoadScene("Nivel3");
                break;
            default:
                Debug.Log("Escena no encontrada");
                break;
        }
    }

    public void SelectLevel(GameObject button)
    {
        buttonName = button.name;

        switch (buttonName)
        {
            case "ButtonLvl1":
                SceneManager.LoadScene("Nivel1");
                break;
            case "ButtonLvl2":
                SceneManager.LoadScene("Nivel2");
                break;
            case "ButtonLvl3":
                SceneManager.LoadScene("Nivel3");
                break;
            default:
                Debug.Log("No button found");
                break;
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Nivel1");
    }

    public void LevelsMenu()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void GoMenu()
    {
        SceneManager.LoadScene("MainMenu");
        inputManager.mouseMovement.Enable();
        inputManager.mouseClick.Enable();
        inputManager.mouseRClick.Enable();
        inputManager.letterRClick.Enable();
    }

    public void Resume()
    {
        pauseCanvas.SetActive(false);

        inputManager.mouseMovement.Enable();
        inputManager.mouseClick.Enable();
        inputManager.mouseRClick.Enable();
        inputManager.letterRClick.Enable();
    }
    public void CloseTutorial()
    {
        tutorialCanvas.SetActive(false);
        inputManager.mouseMovement.Enable();
        inputManager.mouseClick.Enable();
        inputManager.mouseRClick.Enable();
        inputManager.letterRClick.Enable();
    }
}
