using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenHandler : MonoBehaviour
{
    private void Start()
    {
        UnlockCursor();
    }
    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void Quit()
    {
        Debug.Log("Quitting Game");
        Application.Quit();
    }

    public void StartGame()
    {
        Debug.Log("StartGame was called");
        SceneManager.LoadScene("GameScene");
    }
}
