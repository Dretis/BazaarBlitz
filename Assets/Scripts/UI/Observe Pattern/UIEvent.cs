using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class UIEvent
{
    public static Action<GameObject> ShowStorefrontUI = (store) =>
    {
        Debug.Log("ShowStorefrontUI Action happened.");
        store.SetActive(true);
    }
    ;
    public static Action OnChatBubbleUpdated;
    public static Action ChangeItemSprite;

}
