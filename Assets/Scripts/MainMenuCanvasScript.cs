using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuCanvasScript : MonoBehaviour {

    public GameObject infoObject;
    

    public void playerTextPress()
    {
        infoObject.GetComponent<InfoObjectScript>().isAi = false;
        SceneManager.LoadScene("PlayScrean");
    }

    public void computerTexPress()
    {
        infoObject.GetComponent<InfoObjectScript>().isAi = true;
        SceneManager.LoadScene("PlayScrean");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
