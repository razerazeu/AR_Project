using UnityEngine;
using UnityEngine.UI;

public class QuitButton : MonoBehaviour
{
    public Text debugText;

    public void QuitApp()
    {
        Debug.Log("QuitApp was called");
        if (debugText != null)
            debugText.text = "QuitApp was called";

        Application.Quit();
    }
}
