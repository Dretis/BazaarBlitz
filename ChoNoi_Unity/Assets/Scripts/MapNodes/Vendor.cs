using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Vendor : ScriptableObject
{
    [Header("Vendor Stuff")]
    public string vendorName;
    public List<ItemStats> itemsForSale; // Items it can sell
    public List<int> salePrice;

    [Header("Vendor Dialogue")]
    [TextArea(2, 10)]
    public string enterDialogue;

    [TextArea(2, 10)]
    public string leaveDialogue;

    [TextArea(2, 10)]
    public string purchaseDialogue;
}
