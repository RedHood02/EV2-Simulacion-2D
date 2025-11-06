using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public TMPro.TMP_Text timerText;
    public float endGameTimer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(EndGame());
    }

    private void Update()
    {
        timerText.text = "Survive For: " + endGameTimer;
    }

    System.Collections.IEnumerator EndGame()
    {
        yield return new WaitForSeconds(endGameTimer);
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}