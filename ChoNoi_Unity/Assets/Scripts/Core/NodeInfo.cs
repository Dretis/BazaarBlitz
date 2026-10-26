using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New NodeInfo", menuName = "Scriptable Objects/NodeInfo")]
public class NodeInfo : ScriptableObject
{
    public Core.NodeType NodeType;

    public Color nodeHoverColor;
    public Sprite nodeSprite;

    public LocalizedString l_nodeName;
    public LocalizedString l_nodeAbout;
}
