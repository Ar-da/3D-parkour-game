using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartGame()
    {
        SceneManager.LoadScene("LevelPrototype");
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("StartScreen");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
