using PurrNet.Packing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputActionsData : IPackedAuto
{
    public enum InputType
    {
        View, Roll, Inventory, Build, Info,                 // ITM
        Cancel,                                             // UI
        Move, ToggleFreeview,                               // Moving
        FreeviewMove, FreeviewExamine, FreeviewExit,        // Freeview
        Yes, No,                                            // Confirmation
        UpAction, RightAction, DownAction, CombatRoll       // Combat
    }

    public InputType inputType;
    public Vector2 moveValue;
    public bool isPressed; // For Info (hold/release)
}
