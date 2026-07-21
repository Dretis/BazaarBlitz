using UnityEngine;
using UnityEditor;
using UnityEngine.TextCore.Text;

[CustomEditor(typeof(ItemStats))]
public class ItemStatsSpritePreviewer : Editor
{

    ItemStats itemStats;

    private void OnEnable()
    {
        //target is by default available
        //because it inherite Editor
        itemStats = target as ItemStats;
    }


    public override void OnInspectorGUI()
    {
        //Draw whatever we already have in SO definition
        base.OnInspectorGUI();

        //Guard clause
        if (itemStats.itemSprite == null)
            return;

        //Convert the SO Sprite (see SO script) to Texture
        //Texture2D texture = AssetPreview.GetAssetPreview(itemStats.itemSprite);

        itemStats.itemSprite = EditorGUILayout.ObjectField(itemStats.itemSprite, typeof(Sprite), true,
        GUILayout.Height(128), GUILayout.Width(128)) as Sprite;

        //Create empty space 150x150 (you may need to tweak it to scale better your sprite
        //This allows us to place the image JUST UNDER our default inspector
        //GUILayout.Label("", GUILayout.Height(150), GUILayout.Width(150));

        //Draws the texture where we have defined our Label (empty space)
        //GUI.DrawTexture(GUILayoutUtility.GetLastRect(), texture);
    }
}