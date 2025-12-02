using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIScreenManager : MonoBehaviour
{
    public static UIScreenManager instance;

    [SerializeField] private UIScreen currentScreen;
    [SerializeField] private List<UIScreen> screenStack;

    [SerializeField] private List<GameObject> priorSelectedObjects;
    // Start is called before the first frame update

    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("ERROR! More than 1 UIScreenManager found.");
            Destroy(this.gameObject);
        }

        instance = this;
    }

    void Start()
    {
        if(currentScreen != null) screenStack.Add(currentScreen);
    }

    public void GoToScreen(UIScreen screen)
    {
        //priorSelectedObject = EventSystem.current.SetSelectedGameObject(items[0]);
        priorSelectedObjects.Add(EventSystem.current.currentSelectedGameObject);
        EventSystem.current.SetSelectedGameObject(screen.defaultSelectedObject);

        currentScreen.Hide();

        currentScreen = screen;
        screenStack.Add(screen);

        currentScreen.Show();
    }

    public void GoBack()
    {
        if( screenStack.Count == 1)
        {
            Debug.Log("[!] This is the furthest you can go!");
            return;
        }
        currentScreen.Hide();

        EventSystem.current.SetSelectedGameObject(priorSelectedObjects[^1]);
        priorSelectedObjects.RemoveAt(priorSelectedObjects.Count - 1);

        screenStack.RemoveAt(screenStack.Count - 1);
        currentScreen = screenStack[^1];

        currentScreen.Show();
    }
}
