using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UFOButtonEffect : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private GameObject effectCanvas;
    [SerializeField] private UFOScopeIntro scopeIntro;

    [Header("UFO")]
    [SerializeField] private RectTransform ufo;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Particles")]
    [SerializeField] private ParticleSystem[] particleEffects;

    [Header("Transition")]
    [SerializeField] private CanvasGroup transitionCanvas;

    [Header("Special Windows")]
    [SerializeField] private GameObject dragonSpecialWindow;
    [SerializeField] private GameObject unicornSpecialWindow;
    [SerializeField] private GameObject nineTailSpecialWindow;

    [Header("Animation")]
    [SerializeField] private float pressDuration = 0.08f;
    [SerializeField] private float flyDuration = 0.45f;
    [SerializeField] private float moveUp = 120f;
    [SerializeField] private float moveBackScale = 0.15f;

    private bool isPlaying;

    private int selectedParticleIndex = -1;
    private ParticleSystem selectedParticle;

    private Vector2 ufoStartPos;
    private Vector3 ufoStartScale;
    private Quaternion ufoStartRot;
    private Button button;

    private void Awake()
    {
        // 최초 활성화 시 1회만: UFO 초기 트랜스폼을 기억해 둔다 (다시뽑기 재사용 복원용).
        ufoStartPos   = ufo.anchoredPosition;
        ufoStartScale = ufo.localScale;
        ufoStartRot   = ufo.localRotation;
        button        = GetComponent<Button>();
    }

    // 연출창이 열릴 때마다 호출 — 이전 연출의 잔여 상태를 초기화해 재사용을 보장한다.
    private void OnEnable()
    {
        isPlaying             = false;
        selectedParticleIndex = -1;
        selectedParticle      = null;

        ufo.DOKill();
        if (canvasGroup != null)     canvasGroup.DOKill();
        if (transitionCanvas != null) transitionCanvas.DOKill();

        ufo.gameObject.SetActive(true);
        ufo.anchoredPosition = ufoStartPos;
        ufo.localScale       = ufoStartScale;
        ufo.localRotation    = ufoStartRot;

        if (button != null)           button.interactable = true;
        if (canvasGroup != null)      canvasGroup.alpha = 1f;
        if (transitionCanvas != null) transitionCanvas.alpha = 0f;
        if (dragonSpecialWindow != null)   dragonSpecialWindow.SetActive(false);
        if (unicornSpecialWindow != null)  unicornSpecialWindow.SetActive(false);
        if (nineTailSpecialWindow != null) nineTailSpecialWindow.SetActive(false);

        if (scopeIntro == null)
            scopeIntro = FindScopeIntro();

        scopeIntro?.Play();

    }

    public void OnClickUFO()
    {
        if (isPlaying)
            return;

        isPlaying = true;

        SoundManager.Instance.Play(SoundManager.ESound.Effect, "Sounds/SFX/Gatcha/UfoFlyAway");

        Button btn = GetComponent<Button>();

        if (btn != null)
            btn.interactable = false;

        Vector2 startPos = ufo.anchoredPosition;

        Sequence seq = DOTween.Sequence();

        // 1. ����
        seq.Append(
            ufo.DOScale(
                new Vector3(1.15f, 0.85f, 1f),
                pressDuration)
            .SetEase(Ease.OutQuad)
        );

        // 2. �ݵ�
        seq.Append(
            ufo.DOScale(
                Vector3.one,
                pressDuration)
            .SetEase(Ease.OutBack)
        );

        seq.AppendInterval(0.03f);

        // 3. UFO ���ư�
        seq.Append(
            ufo.DOScale(moveBackScale, flyDuration)
            .SetEase(Ease.InBack)
        );

        seq.Join(
            ufo.DOAnchorPos(
                startPos + new Vector2(60f, moveUp),
                flyDuration)
            .SetEase(Ease.InQuad)
        );

        float randomTilt = Random.Range(-20f, 20f);

        seq.Join(
            ufo.DORotate(
                new Vector3(0f, 0f, randomTilt),
                flyDuration)
            .SetEase(Ease.OutQuad)
        );

        seq.Join(
            canvasGroup.DOFade(
                0f,
                flyDuration)
        );

        // UFO �ִϸ��̼� ����
        seq.OnComplete(() =>
        {
            ufo.gameObject.SetActive(false);
            PlaySelectedParticle();
        });
    }

    private void PlaySelectedParticle()
    {
        if (particleEffects == null || particleEffects.Length == 0)
            return;

        // 가챠 추첨 결과에 맞는 연출 인덱스를 선택한다.
        // 결과가 없으면(연출창에 직접 진입한 경우) 기존처럼 랜덤으로 폴백.
        selectedParticleIndex = ResolveParticleIndex();

        selectedParticle = particleEffects[selectedParticleIndex];

        PlaceParticleAtCenter(selectedParticle);

        selectedParticle.gameObject.SetActive(true);

        selectedParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        selectedParticle.Clear();
        selectedParticle.Play();

        float duration =
            selectedParticle.main.duration +
            selectedParticle.main.startLifetime.constantMax;

        DOVirtual.DelayedCall(duration, StartTransition);
    }

    // 결과 캐릭터 타입 → 연출 인덱스 ([0]dragon / [1]unicorn / [2]구미호(Fox)).
    // 결과가 없으면 랜덤 폴백.
    private int ResolveParticleIndex()
    {
        CharacterData result = GachaRoller.LastResult;

        if (result == null)
            return Random.Range(0, particleEffects.Length);

        // CharacterData.type 은 "Fox"(구미호) 를 쓰지만, 연출/창 명칭은 "NineTail" 로 혼용한다 — 둘 다 매핑.
        int index = result.type switch
        {
            "Dragon"             => 0,
            "Unicorn"            => 1,
            "Fox" or "NineTail"  => 2,
            _                    => -1,
        };

        if (index < 0 || index >= particleEffects.Length)
        {
            Debug.LogWarning($"[UFOButtonEffect] '{result.type}' 에 매핑된 특수 연출이 없습니다 — 인덱스 0으로 폴백.");
            return 0;
        }

        return index;
    }

    private void StartTransition()
    {
        transitionCanvas.alpha = 0f;

        transitionCanvas
            .DOFade(1f, 0.5f)
            .SetEase(Ease.Linear)
            .OnComplete(OpenSpecialWindow);
    }

    private void OpenSpecialWindow()
    {
        dragonSpecialWindow.SetActive(false);
        unicornSpecialWindow.SetActive(false);
        nineTailSpecialWindow.SetActive(false);

        switch (selectedParticleIndex)
        {
            case 0:
                dragonSpecialWindow.SetActive(true);
                break;

            case 1:
                unicornSpecialWindow.SetActive(true);
                break;

            case 2:
                nineTailSpecialWindow.SetActive(true);
                break;
        }

        // 특수 결과창(dragon/unicorn/nineTail)이 뜨는 순간 결과음 재생
        var sfx = Resources.Load<AudioClip>("Sounds/SFX/Gatcha/SpecialResult");
        SoundManager.Instance.Play(SoundManager.ESound.Effect, "Sounds/SFX/Gatcha/SpecialResult");

        // 결과음 강조용 BGM 덕킹 → 결과음 길이만큼 지난 뒤 자동 복원 (자체 완결)
        SoundManager.Instance.SetVolume(SoundManager.ESound.Bgm, SoundSettings.BgmVolume * 0.3f);
        float duckTime = sfx != null ? sfx.length : 1.5f;
        DOVirtual.DelayedCall(duckTime, () =>
            SoundManager.Instance.SetVolume(SoundManager.ESound.Bgm, SoundSettings.BgmVolume));

        // ����â(Canvas) ��ü ����
        effectCanvas.SetActive(false);

        isPlaying = false;
    }


    private UFOScopeIntro FindScopeIntro()
    {
        UFOScopeIntro[] intros = Resources.FindObjectsOfTypeAll<UFOScopeIntro>();
        for (int i = 0; i < intros.Length; i++)
        {
            if (intros[i].gameObject.scene.IsValid())
                return intros[i];
        }

        return null;
    }


private void PlaceParticleAtCenter(ParticleSystem particle)
    {
        if (particle == null)
            return;

        RectTransform particleRect = particle.transform as RectTransform;
        RectTransform ufoParent = ufo.parent as RectTransform;

        if (particleRect != null && ufoParent != null && particleRect.parent == ufoParent)
        {
            particleRect.anchoredPosition = ufoStartPos;
            particleRect.localRotation = Quaternion.identity;
            return;
        }

        if (ufoParent != null)
        {
            particle.transform.position = ufoParent.TransformPoint(ufoStartPos);
            particle.transform.rotation = Quaternion.identity;
            return;
        }

        particle.transform.position = ufo.position;
        particle.transform.rotation = Quaternion.identity;
    }
}