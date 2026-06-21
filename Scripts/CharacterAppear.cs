using UnityEngine;
using DG.Tweening;

public class CharacterAppear : MonoBehaviour
{
    [SerializeField] private RectTransform character;

    [Header("Appear")]
    [SerializeField] private float startScale = 0.05f;
    [SerializeField] private float popScale = 1.18f;
    [SerializeField] private float popDuration = 0.28f;
    [SerializeField] private float settleDuration = 0.12f;

    [Header("Appear Particles")]
    [SerializeField] private ParticleSystem[] appearParticles;

    private Vector2 originalPos;
    private Sequence appearSequence;

    private void Awake()
    {
        originalPos = character.anchoredPosition;
    }

    public void PlayAppear()
    {
        StopFloating();

        character.localScale = Vector3.one * startScale;
        character.localRotation = Quaternion.identity;
        character.anchoredPosition = originalPos;

        appearSequence = DOTween.Sequence()
            .SetTarget(character)
            .Append(
                character.DOScale(popScale, popDuration)
                    .SetEase(Ease.OutBack)
            )
            .Append(
                character.DOScale(1f, settleDuration)
                    .SetEase(Ease.OutQuad)
            )
            .OnComplete(PlayAppearParticles);
    }

    private void PlayAppearParticles()
    {
        if (appearParticles == null || appearParticles.Length == 0)
        {
            return;
        }

        foreach (ParticleSystem particle in appearParticles)
        {
            if (particle == null)
            {
                continue;
            }

            particle.gameObject.SetActive(true);
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.Clear();
            particle.Play();
        }
    }

    public void StopFloating()
    {
        appearSequence?.Kill();
        appearSequence = null;

        if (character != null)
        {
            character.DOKill();
            character.anchoredPosition = originalPos;
            character.localRotation = Quaternion.identity;
        }
    }
}
