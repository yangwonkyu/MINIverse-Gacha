using UnityEngine;
using DG.Tweening;

public class BackgroundMove : MonoBehaviour
{
    [SerializeField] private RectTransform upperBackground;
    [SerializeField] private RectTransform lowerBackground;

    [SerializeField] private float duration = 0.8f;

    [Header("Character")]
    [SerializeField] private CharacterAppear characterAppear;

    // 닫힌(초기) 배경 위치 — 연출창이 다시뽑기로 재사용될 때마다 여기서 다시 갈라지도록 기억해 둔다.
    private Vector2 upperClosedPos;
    private Vector2 lowerClosedPos;
    private bool capturedClosed;

    private void Awake()
    {
        if (upperBackground != null) upperClosedPos = upperBackground.anchoredPosition;
        if (lowerBackground != null) lowerClosedPos = lowerBackground.anchoredPosition;
        capturedClosed = true;
    }

    public void PlayAnimation()
    {
        // 이전 재생의 잔여 트윈/위치를 초기화 — 창을 재사용해도 배경 갈라짐이 항상 닫힘 상태에서 다시 재생된다.
        upperBackground.DOKill();
        lowerBackground.DOKill();
        if (capturedClosed)
        {
            upperBackground.anchoredPosition = upperClosedPos;
            lowerBackground.anchoredPosition = lowerClosedPos;
        }

        Sequence seq = DOTween.Sequence();

        seq.Join(
            upperBackground
                .DOAnchorPosY(-450f, duration)
                .SetEase(Ease.OutCubic)
        );

        seq.Join(
            lowerBackground
                .DOAnchorPosY(450f, duration)
                .SetEase(Ease.OutCubic)
        );

        // ����� ���� ������ �� ĳ���� ����
        seq.Insert(duration * 0.65f,
            DOVirtual.DelayedCall(0f, () =>
            {
                characterAppear.PlayAppear();
            })
        );
    }
}