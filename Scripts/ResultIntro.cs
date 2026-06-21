using UnityEngine;
using DG.Tweening;

public class ResultIntro : MonoBehaviour
{
    [SerializeField] private CanvasGroup introCanvasGroup;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 1f;

    private void OnEnable()
    {
        introCanvasGroup.DOKill();

        introCanvasGroup.alpha = 1f;

        introCanvasGroup
            .DOFade(0f, fadeDuration)
            .SetEase(Ease.InOutSine);
    }
}