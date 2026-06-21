using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GachaTelescopePreview : MonoBehaviour
{
    [Header("UFO Flyby")]
    [SerializeField] private RectTransform ufo;
    [SerializeField] private CanvasGroup ufoCanvasGroup;
    [SerializeField] private Vector2 ufoStartPos = new(-520f, 120f);
    [SerializeField] private Vector2 ufoEndPos = new(520f, 210f);
    [SerializeField] private float ufoFlyDuration = 0.55f;

    [Header("Telescope Iris")]
    [SerializeField] private Image irisOverlay;
    [SerializeField] private Material irisMaterial;
    [SerializeField] private Vector2 irisCenter = new(0.5f, 0.43f);
    [SerializeField] private float irisStartRadius = 0.9f;
    [SerializeField] private float irisEndRadius = -0.05f;
    [SerializeField] private float irisCloseDuration = 0.65f;
    [SerializeField] private float irisSoftness = 0.035f;

    [Header("Timing")]
    [SerializeField] private float delayBeforeIris = 0.12f;
    [SerializeField] private float holdBlackDuration = 0.5f;

    private Sequence sequence;
    private Material runtimeIrisMaterial;
    private static readonly int CenterId = Shader.PropertyToID("_Center");
    private static readonly int RadiusId = Shader.PropertyToID("_Radius");
    private static readonly int SoftnessId = Shader.PropertyToID("_Softness");

    private void Awake()
    {
        PrepareRuntimeMaterial();
        ResetPreview();
    }

    private void OnDisable()
    {
        sequence?.Kill();
        ResetPreview();
    }

    public void Play(Action onComplete)
    {
        PrepareRuntimeMaterial();
        sequence?.Kill();
        ResetPreview();

        gameObject.SetActive(true);
        if (ufo != null)
            ufo.gameObject.SetActive(true);
        if (irisOverlay != null)
            irisOverlay.gameObject.SetActive(true);

        sequence = DOTween.Sequence();

        if (ufo != null)
        {
            sequence.Append(ufo.DOAnchorPos(ufoEndPos, ufoFlyDuration).SetEase(Ease.InOutSine));
            sequence.Join(ufo.DOScale(0.62f, ufoFlyDuration).SetEase(Ease.InSine));
        }

        if (ufoCanvasGroup != null)
        {
            sequence.Join(ufoCanvasGroup.DOFade(1f, 0.12f));
            sequence.Append(ufoCanvasGroup.DOFade(0f, 0.12f));
        }

        sequence.AppendInterval(delayBeforeIris);
        sequence.Append(DOTween.To(SetIrisRadius, irisStartRadius, irisEndRadius, irisCloseDuration).SetEase(Ease.InCubic));
        sequence.AppendInterval(holdBlackDuration);
        sequence.OnComplete(() =>
        {
            onComplete?.Invoke();
            ResetPreview();
        });
    }

    private void PrepareRuntimeMaterial()
    {
        if (irisOverlay == null || irisMaterial == null)
            return;

        if (runtimeIrisMaterial == null)
            runtimeIrisMaterial = Instantiate(irisMaterial);

        irisOverlay.material = runtimeIrisMaterial;
        runtimeIrisMaterial.SetVector(CenterId, irisCenter);
        runtimeIrisMaterial.SetFloat(SoftnessId, irisSoftness);
    }

    private void ResetPreview()
    {
        if (ufo != null)
        {
            ufo.DOKill();
            ufo.anchoredPosition = ufoStartPos;
            ufo.localScale = Vector3.one * 0.36f;
            ufo.localRotation = Quaternion.Euler(0f, 0f, -8f);
            ufo.gameObject.SetActive(false);
        }

        if (ufoCanvasGroup != null)
        {
            ufoCanvasGroup.DOKill();
            ufoCanvasGroup.alpha = 0f;
        }

        if (runtimeIrisMaterial != null)
            SetIrisRadius(irisStartRadius);

        if (irisOverlay != null)
            irisOverlay.gameObject.SetActive(false);
    }

    private void SetIrisRadius(float radius)
    {
        if (runtimeIrisMaterial != null)
            runtimeIrisMaterial.SetFloat(RadiusId, radius);
    }
}
