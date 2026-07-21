using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemLists
{
    public static HashSet<string> StealOnPassByItemNames = new HashSet<string> { "TestSteal" };
    public static HashSet<string> CombatOnPassByItemNames = new HashSet<string> { "TestCombat", "Rice Wine", "Cobra Whiskey" };
    public static HashSet<string> StopOnStoreOnPassBy = new HashSet<string> { "TestStore" };
    public static HashSet<string> TargetSelectionItemNames = new HashSet<string> { "Rafflesia", "Hoa Mai", "Banh Mi", "Banh Mi Sandwich"};
}
