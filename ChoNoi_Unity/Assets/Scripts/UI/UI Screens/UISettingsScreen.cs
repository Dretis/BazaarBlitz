using LitMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class UISettingsScreen : UIScreen
{
    //public GameObject defaultSelectedObject;
    private CanvasGroup mainGroup;
    public bool inSidebar;
    public bool inSubmenu = false;

    public CanvasGroup settingsSideGroup;
    public Image submenuCover;
    //public List<Button> settingsOptions;

    // Start is called before the first frame update
    void Awake()
    {
        Initialize();

        if (hideOnAwake) HideImmediately();
    }

    public override void Initialize()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponent<Canvas>();

        mainGroup = GetComponent<CanvasGroup>();
    }

    public override void Show()
    {
        // Does something when it needs to appear

        if (currentMotion.IsPlaying()) currentMotion.Cancel();

        if (inSubmenu) 
        {
            inSidebar = true;
            ActivateSidebar();
        }
        else
        {
            inSidebar = true;
            ActivateSettingsScreen();
        }
        // currentMotion = do some animation here
    }

    public override void Hide()
    {
        // Does something when it needs to go away

        if (currentMotion.IsPlaying()) currentMotion.Complete();

        if (inSubmenu)
        {
            inSidebar = false;
            DeactivateSidebar();
        }
        else
        {
            inSidebar = false;

            mainGroup.alpha = 1.0f;
            mainGroup.interactable = false;
            mainGroup.blocksRaycasts = false;

            canvas.enabled = false;
            //StartCoroutine(DeactivateSettingsScreen());
        }
        // Prevent players from interacting with left-side menu
        //settingsSideGroup.interactable = false;
    }

    public void ActivateSettingsScreen()
    {
        canvas.enabled = true;

        mainGroup.alpha = 0.0f;
        mainGroup.interactable = true;
        mainGroup.blocksRaycasts = true;

        currentMotion = LMotion.Create(0f, 1f, 0.25f)
            .WithEase(Ease.OutQuad)
            .Bind(x => mainGroup.alpha = x);
    }

    public IEnumerator DeactivateSettingsScreen()
    {

        mainGroup.alpha = 1.0f;
        mainGroup.interactable = false;
        mainGroup.blocksRaycasts = false;

        currentMotion = LMotion.Create(1f, 0f, 0.25f)
            .WithEase(Ease.OutQuad)
            .Bind(x => mainGroup.alpha = x);

        yield return currentMotion.ToYieldInstruction();

        canvas.enabled = false;
    }

    public void ActivateSidebar()
    {
        inSubmenu = false;

        settingsSideGroup.alpha = 1f;
        settingsSideGroup.interactable = true;
        settingsSideGroup.blocksRaycasts = true;

        submenuCover.enabled = true;
    }

    public void DeactivateSidebar()
    {
        inSubmenu = true;

        settingsSideGroup.alpha = 0.5f;
        settingsSideGroup.interactable = false;
        settingsSideGroup.blocksRaycasts = false;

        submenuCover.enabled = false;
    }
}
