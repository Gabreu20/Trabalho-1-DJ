using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] AudioSource source;

    [SerializeField] AudioClip open, close;

    bool opened = false;

    private void Start()
    {
        source = GetComponent<AudioSource>();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!opened)
            {
                source.PlayOneShot(open);
                opened = true;
                canvas.SetActive(opened);
            }
            else
            {
                source.PlayOneShot(close);
                opened = false;
                canvas.SetActive(false);
            }
        }
    }

    public void closePanel()
    {
        source.PlayOneShot(close);
        opened = false;
        canvas.SetActive(false);   
    }
    public void Quit()
    {
        SceneManager.LoadScene("MainMenu");
    }

}
