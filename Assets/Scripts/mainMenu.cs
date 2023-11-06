using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip btnPlay;
    [Header("Level")]
    public SceneInfo scene;

    public void climb()
    {
        scene.level = 0;
        source.PlayOneShot(btnPlay);
        StartCoroutine(wait());
    }
    IEnumerator wait()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Tutorial");
    }
    public void menu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {

        Application.Quit();
    }
}
