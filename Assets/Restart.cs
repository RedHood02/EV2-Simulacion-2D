using UnityEngine;

public class Restart : MonoBehaviour
{
    public KeyCode restartKey;
    void Update()
    {
        if (Input.GetKeyDown(restartKey)) UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}