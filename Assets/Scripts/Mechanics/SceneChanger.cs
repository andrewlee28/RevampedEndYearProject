using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
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
        Debug.Log("Button Clicked");
        SceneManager.LoadScene("SampleScene");
    }

    public void HowToPlay()
    {
        Debug.Log("Button Clicked");
        SceneManager.LoadScene("HowToPlay");
    }

    public void Homescreen()
    {
        SceneManager.LoadScene("Homescreen");
    }
}
