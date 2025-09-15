using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HyperlinkManager : MonoBehaviour
{
    public void OpenURL(string url)
    {
        if (url == null) { Debug.Log("Empty URL"); return; }

        Application.OpenURL(url);
    }
}
