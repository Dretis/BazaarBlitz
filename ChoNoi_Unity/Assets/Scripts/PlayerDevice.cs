using Core;
using UnityEngine;

public class PlayerDevice : MonoBehaviour
{
    [Header("Input")]
    public PlayerInputController playerInputController;
    [SerializeField] private EventSystemEnabler _eventSystemEnabler;

    [Header("UI Managers")]
    [SerializeField] private UILevelUpManager _uiLevelUpManager;
    [SerializeField] private UIBlessingsManager _uiBlessingsManager;
    [SerializeField] private UIPromptManager _uiPromptManager;
    [SerializeField] private UIInventoryManager _uiInventoryManager;
    [SerializeField] private UICombatSelectorManager _uiCombatSelectorManager;
    [SerializeField] private UIRenovateSelectorManager _uiRenovateSelectorManager;

    private PlayerState _playerState;
    private GameplayTest _gameplayTest;

    public void Initialize(PlayerState ps, GameplayTest gt)
    {
        _playerState = ps;
        _gameplayTest = gt;

        playerInputController.Initialize(ps, gt);
        _eventSystemEnabler.Initialize(ps, gt);

        _uiLevelUpManager.Initialize(ps, gt);
        _uiBlessingsManager.Initialize(ps, gt);
        _uiPromptManager.Initialize(ps, gt);
        _uiInventoryManager.Initialize(ps, gt);
        _uiCombatSelectorManager.Initialize(ps, gt);
        _uiRenovateSelectorManager.Initialize(ps, gt);
    }
}
