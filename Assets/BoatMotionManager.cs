using LitMotion;
using LitMotion.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatMotionManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer p_Boat;
    [SerializeField] private PlayerPaletteLoader boatPaletteLoader;
    public Transform p_BoatTransform;

    private MotionHandle pBoatMotion;

    [Header("Boat Motion Variables")]
    //[SerializeField] SerializableMotionSettings<float, PunchOptions> boatMotionSettings;
    //  The vibration will fluctuate within the range of startValue ± strength.
    [SerializeField] private float startValue;
    [SerializeField] private float strength = 1f; //vibration strength
    [SerializeField] private float duration = 1f; //vibration strength
    [SerializeField] private int frequency = 4; //vibration strength

    [Header("Listen On Event Channels")]
    public VoidEventChannelSO m_ReturnToMainMenu;

    private void OnEnable()
    {
        m_ReturnToMainMenu.OnEventRaised += OnReturnToMainMenu;
    }

    private void OnDisable()
    {
        m_ReturnToMainMenu.OnEventRaised -= OnReturnToMainMenu;
    }

    // Start is called before the first frame update
    void Start()
    {
        p_BoatTransform = p_Boat.transform;

        DoPBoatMotion();
    }

    private void DoPBoatMotion()
    {
        if(pBoatMotion.IsActive()) pBoatMotion.Cancel();

        pBoatMotion = LMotion.Punch.Create(startValue, strength, duration)
                        .WithLoops(-1, LoopType.Flip)
                        .WithFrequency(frequency)
                        .WithDampingRatio(1f)
                        //.WithEase(Ease.InOutSine)
                        .BindToLocalPositionY(p_BoatTransform);
    }
    /*
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            DoPBoatMotion();
        }
    }
    */

    public PlayerPaletteLoader GetBoatPaletteLoader()
    {
        return boatPaletteLoader;
    }

    public void StopPBoatMotion()
    {
        if (pBoatMotion.IsActive()) pBoatMotion.Cancel();
    }

    private void OnReturnToMainMenu()
    {
        StopPBoatMotion();
    }
}
