using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CanvasScript : MonoBehaviour {

    public Button palyAgain;

    public void playAgainPress()
    {
        SceneManager.LoadScene("PlayScrean");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    
    public void backPress()
    {
        GameObject[] tab = GameObject.FindGameObjectsWithTag("InfoObject");
        Destroy(tab[0]);
        SceneManager.LoadScene("MainMenu");
    }
}
