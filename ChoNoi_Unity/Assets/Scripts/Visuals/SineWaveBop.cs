using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SineWaveBop : MonoBehaviour
{
    [SerializeField] private float yDistance;
    [SerializeField] private float duration;
    private bool wavePatternOn = false;
    private void OnEnable()
    {
        if (!wavePatternOn)
        {
            var decideWavePattern = Random.Range(1, 3);
            switch (decideWavePattern)
            {
                case 1:
                    transform.DOMoveY(transform.position.y + yDistance, duration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
                    wavePatternOn = true;
                    break;
                case 2:
                    transform.DOMoveY(transform.position.y - yDistance, duration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
                    wavePatternOn = true;
                    break;
                default:
                    transform.DOMoveY(transform.position.y + yDistance, duration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
                    wavePatternOn = true;
                    break;
            }
        }
    }
}
