using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

public class HyperlinkManager : MonoBehaviour
{
    public void OpenURL(string url)
    {
        if (url == null) { Debug.Log("Empty URL"); return; }

        Application.OpenURL(url);
    }

    public void OpenScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
