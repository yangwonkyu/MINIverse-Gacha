using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UFOScopeIntro : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform ufo;
    [SerializeField] private CanvasGroup ufoCanvasGroup;
    [SerializeField] private CanvasGroup blackCanvas;
    [SerializeField] private Material scopeMaterial;
        [SerializeField] private GameObject clickText;
    [SerializeField] private Image ufoImage;

    [Header("Hint Outline")]
    [SerializeField] private Material unicornOutlineMaterial;
    [SerializeField] private Material dragonOutlineMaterial;
    [SerializeField] private Material nineTailOutlineMaterial;

    [Header("Move Area")]
    [SerializeField] private float moveX = 900f;
    
    [SerializeField] private float horizontalEdgePadding = 190f;
    [SerializeField] private float verticalEdgePadding = 150f;
[SerializeField] private float moveY = 700f;

    [Header("Settings")]
    [SerializeField] private int moveCount = 4;
    [SerializeField] private float moveDuration = 0.42f;
    [SerializeField] private float stayDuration = 0.3f;
    [SerializeField] private float scopeFollowSpeed = 3.5f;

    [Header("Disappear")]
    [SerializeField] private float scopeLostDelay = 0.18f;
    [SerializeField] private float warpScale = 1.25f;

    [Header("Floating")]
    [SerializeField] private float floatHeight = 18f;
    [SerializeField] private float floatDuration = 1.6f;

    private readonly int CenterID = Shader.PropertyToID("_Center");
    private readonly int RadiusID = Shader.PropertyToID("_Radius");

    private Vector2 originPos;
    private Vector3 originScale;
    private Vector2 currentCenter;
    private Material originalUFOMaterial;

    private Tween floatTween;



    public void Play()
    {
        CacheUFOImage();
        ResetUFOHintMaterial();

        StopAllCoroutines();

        DOTween.Kill(ufo);

        floatTween?.Kill();

        StartCoroutine(CoPlay());
    }

    private IEnumerator CoPlay()
    {
        originPos = ufo.anchoredPosition;
        originScale = ufo.localScale;

        clickText.SetActive(false);

        blackCanvas.alpha = 1f;
        blackCanvas.gameObject.SetActive(true);

        ufoCanvasGroup.alpha = 1f;
        ufo.localScale = originScale * 0.45f;

        scopeMaterial.SetFloat(RadiusID, 0.12f);

        Vector3 viewport =
            Camera.main.WorldToViewportPoint(ufo.position);

        currentCenter = new Vector2(viewport.x, viewport.y);

        SetScopeCenter(currentCenter);

        yield return new WaitForSeconds(0.5f);

        yield return UFOTracking();

        yield return UFODisappear();

        yield return ScopeSearch();

        yield return CloseScope();

        blackCanvas.gameObject.SetActive(false);

        yield return ReturnUFO();
    }

    private IEnumerator UFOTracking()
    {
        for (int i = 0; i < moveCount; i++)
        {
            Vector2 targetPos = GetRandomVisiblePosition();

            bool reachedTarget = false;

            ufo.DOAnchorPos(targetPos, moveDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() => reachedTarget = true);

            while (!reachedTarget)
            {
                UpdateScopeCenter();
                yield return null;
            }

            yield return new WaitForSeconds(stayDuration);
        }
    }

    private IEnumerator UFODisappear()
    {
        yield return new WaitForSeconds(scopeLostDelay);

        Sequence seq = DOTween.Sequence();

        seq.Append(
            ufo.DOScale(
                originScale * warpScale,
                0.06f)
        );

        seq.Join(
            ufoCanvasGroup.DOFade(
                0f,
                0.08f)
        );

        yield return seq.WaitForCompletion();

        yield return new WaitForSeconds(0.15f);
    }

    private IEnumerator ScopeSearch()
    {
        yield return new WaitForSeconds(0.1f);

        Vector2 center = currentCenter;

        Sequence seq = DOTween.Sequence();

        seq.Append(
            DOTween.To(
                () => center.x,
                x =>
                {
                    center.x = x;
                    SetScopeCenter(center);
                },
                Mathf.Clamp(center.x - 0.25f, 0.08f, 0.92f),
                0.22f)
        );

        seq.AppendInterval(0.1f);

        seq.Append(
            DOTween.To(
                () => center.x,
                x =>
                {
                    center.x = x;
                    SetScopeCenter(center);
                },
                Mathf.Clamp(center.x + 0.5f, 0.08f, 0.92f),
                0.3f)
        );

        seq.AppendInterval(0.1f);

        seq.Append(
            DOTween.To(
                () => center.x,
                x =>
                {
                    center.x = x;
                    SetScopeCenter(center);
                },
                Mathf.Clamp(center.x - 0.5f, 0.08f, 0.92f),
                0.3f)
        );

        seq.AppendInterval(0.1f);

        seq.Append(
            DOTween.To(
                () => center.x,
                x =>
                {
                    center.x = x;
                    SetScopeCenter(center);
                },
                Mathf.Clamp(center.x + 0.25f, 0.08f, 0.92f),
                0.22f)
        );

        yield return seq.WaitForCompletion();
    }

    private IEnumerator CloseScope()
    {
        float radius = scopeMaterial.GetFloat(RadiusID);

        yield return DOTween.To(
                () => radius,
                x =>
                {
                    radius = x;
                    scopeMaterial.SetFloat(RadiusID, radius);
                },
                0f,
                0.45f)
            .WaitForCompletion();
    }

    private IEnumerator ReturnUFO()
    {
                ApplyHintOutline();

ufo.localScale = originScale;
        ufoCanvasGroup.alpha = 1f;

        Vector2 startPos = GetVisibleReturnStartPosition();

        ufo.anchoredPosition = startPos;

        Sequence seq = DOTween.Sequence();

        seq.Append(
            ufo.DOAnchorPos(originPos, 0.65f)
                .SetEase(Ease.OutBack)
        );

        seq.Join(
            ufo.DORotate(
                new Vector3(0f, 0f, 720f),
                0.65f,
                RotateMode.FastBeyond360)
        );

        yield return seq.WaitForCompletion();

        yield return ufo
            .DOAnchorPosY(originPos.y + 25f, 0.18f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.OutQuad)
            .WaitForCompletion();

        clickText.SetActive(true);

        StartFloating();
    }

        private void CacheUFOImage()
    {
        if (ufoImage == null && ufo != null)
            ufoImage = ufo.GetComponent<Image>();

        if (ufoImage != null && originalUFOMaterial == null)
            originalUFOMaterial = ufoImage.material;
    }

    private void ApplyHintOutline()
    {
        CacheUFOImage();

        Material hintMaterial = ResolveHintMaterial();
        if (ufoImage != null && hintMaterial != null)
            ufoImage.material = hintMaterial;
    }

    private Material ResolveHintMaterial()
    {
        CharacterData result = GachaRoller.LastResult;
        if (result == null)
            return null;

        return result.type switch
        {
            "Dragon" => dragonOutlineMaterial,
            "Unicorn" => unicornOutlineMaterial,
            "Fox" or "NineTail" => nineTailOutlineMaterial,
            _ => null,
        };
    }

    private void ResetUFOHintMaterial()
    {
        CacheUFOImage();

        if (ufoImage != null)
            ufoImage.material = originalUFOMaterial;
    }

private void StartFloating()
    {
        floatTween?.Kill();

        floatTween = ufo
            .DOAnchorPosY(originPos.y + floatHeight, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void SetScopeCenter(Vector2 center)
    {
        center.x = Mathf.Clamp(center.x, 0.08f, 0.92f);
        center.y = Mathf.Clamp(center.y, 0.08f, 0.92f);

        currentCenter = center;

        scopeMaterial.SetVector(
            CenterID,
            currentCenter);
    }

    private void UpdateScopeCenter()
    {
        Vector3 viewport =
            Camera.main.WorldToViewportPoint(ufo.position);

        Vector2 targetCenter =
            new Vector2(viewport.x, viewport.y);

        currentCenter = Vector2.Lerp(
            currentCenter,
            targetCenter,
            Time.deltaTime * scopeFollowSpeed);

        SetScopeCenter(currentCenter);
    }


private Vector2 GetVisibleReturnStartPosition()
    {
        RectTransform parentRect = ufo.parent as RectTransform;
        if (parentRect == null || parentRect.rect.height < 100f)
            return originPos + Vector2.up * 360f;

        Rect rect = parentRect.rect;
        float halfUfoHeight = ufo.rect.height * 0.5f;
        float topY = rect.height * 0.5f - halfUfoHeight - verticalEdgePadding;
        float startY = Mathf.Min(originPos.y + 420f, topY);

        return new Vector2(originPos.x, Mathf.Max(originPos.y + 180f, startY));
    }


private Vector2 GetRandomVisiblePosition()
    {
        RectTransform parentRect = ufo.parent as RectTransform;
        if (parentRect == null || parentRect.rect.width < 100f || parentRect.rect.height < 100f)
        {
            return new Vector2(
                Random.Range(-moveX, moveX),
                Random.Range(-moveY, moveY));
        }

        Rect rect = parentRect.rect;
        Vector2 halfSize = ufo.rect.size * 0.5f;

        float maxX = Mathf.Max(0f, rect.width * 0.5f - halfSize.x - horizontalEdgePadding);
        float maxY = Mathf.Max(0f, rect.height * 0.5f - halfSize.y - verticalEdgePadding);

        maxX = Mathf.Min(maxX, moveX);
        maxY = Mathf.Min(maxY, moveY);

        return new Vector2(
            Random.Range(-maxX, maxX),
            Random.Range(-maxY, maxY));
    }
}