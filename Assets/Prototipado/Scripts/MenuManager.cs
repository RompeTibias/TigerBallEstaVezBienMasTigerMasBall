using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    string actualScene;
    public GameObject pauseCanvas;

    public PelotaPrototipo inputManager;

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

    public void PlayGame()
    {
        SceneManager.LoadScene("Nivel1");
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void GoMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Resume()
    {
        pauseCanvas.SetActive(false);

        inputManager.mouseMovement.Enable();
        inputManager.mouseClick.Enable();
        inputManager.mouseRClick.Enable();
        inputManager.letterRClick.Enable();
    }
}
