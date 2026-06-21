using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UnicornSpecialIntro : MonoBehaviour
{
    [Header("Transition")]
    [SerializeField] private CanvasGroup blackCanvas;

    [Header("Dissolve")]
    [SerializeField] private Image symbolImage;
    [SerializeField] private string dissolveProperty = "_Dissolve";
    [SerializeField] private float dissolveDuration = 1.2f;
    [SerializeField] private float afterDissolveDelay = 0.15f;

    [Header("Legacy Intro Objects")]
    [SerializeField] private RectTransform starRoot;
    [SerializeField] private CanvasGroup starCanvasGroup;
    [SerializeField] private ParticleSystem particleSystem;
    [SerializeField] private CanvasGroup dialogueGroup;

    [Header("Background")]
    [SerializeField] private BackgroundMove backgroundMove;

    private Material baseSymbolMaterial;
    private Material runtimeSymbolMaterial;

    private void OnEnable()
    {
        CacheSymbolImage();
        PrepareRuntimeMaterial();
        KillTweens();
        HideLegacyObjects();
        ResetIntroObjects();

        PlayIntro();
    }

    private void OnDisable()
    {
        KillTweens();
        ReleaseRuntimeMaterial();
    }

    private void PlayIntro()
    {
        Sequence seq = DOTween.Sequence();

        if (runtimeSymbolMaterial != null)
        {
            seq.Append(
                DOTween.To(
                        () => runtimeSymbolMaterial.GetFloat(dissolveProperty),
                        value => runtimeSymbolMaterial.SetFloat(dissolveProperty, value),
                        1f,
                        dissolveDuration)
                    .SetEase(Ease.InOutSine)
                    .SetTarget(runtimeSymbolMaterial)
            );
        }

        seq.AppendCallback(HideSymbol);
        seq.AppendInterval(afterDissolveDelay);

        seq.AppendCallback(() =>
        {
            backgroundMove.PlayAnimation();
        });

        seq.Join(
            blackCanvas.DOFade(0f, 0.6f)
        );
    }

    private void CacheSymbolImage()
    {
        if (symbolImage != null)
        {
            return;
        }

        Image[] images = GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i].name == "심볼")
            {
                symbolImage = images[i];
                break;
            }
        }
    }

    private void PrepareRuntimeMaterial()
    {
        ReleaseRuntimeMaterial();

        if (symbolImage == null || symbolImage.material == null)
        {
            return;
        }

        if (baseSymbolMaterial == null)
        {
            baseSymbolMaterial = symbolImage.material;
        }

        runtimeSymbolMaterial = new Material(baseSymbolMaterial);
        runtimeSymbolMaterial.SetFloat(dissolveProperty, 0f);
        symbolImage.material = runtimeSymbolMaterial;
    }

    private void ResetIntroObjects()
    {
        if (blackCanvas != null)
        {
            blackCanvas.alpha = 1f;
            blackCanvas.gameObject.SetActive(true);
        }

        if (symbolImage != null)
        {
            symbolImage.gameObject.SetActive(true);
            Color color = symbolImage.color;
            color.a = 1f;
            symbolImage.color = color;
        }
    }

    private void HideLegacyObjects()
    {
        if (starRoot != null)
        {
            starRoot.gameObject.SetActive(false);
        }

        if (starCanvasGroup != null)
        {
            starCanvasGroup.alpha = 0f;
        }

        if (dialogueGroup != null)
        {
            dialogueGroup.alpha = 0f;
            dialogueGroup.gameObject.SetActive(false);
        }

        if (particleSystem != null)
        {
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystem.Clear();
        }
    }

    private void HideSymbol()
    {
        if (symbolImage != null)
        {
            symbolImage.gameObject.SetActive(false);
        }
    }

    private void KillTweens()
    {
        if (blackCanvas != null)
        {
            DOTween.Kill(blackCanvas);
        }

        if (runtimeSymbolMaterial != null)
        {
            DOTween.Kill(runtimeSymbolMaterial);
        }
    }

    private void ReleaseRuntimeMaterial()
    {
        if (runtimeSymbolMaterial == null)
        {
            return;
        }

        if (symbolImage != null)
        {
            symbolImage.material = baseSymbolMaterial;
        }

        Destroy(runtimeSymbolMaterial);
        runtimeSymbolMaterial = null;
    }
}
