using UnityEngine;
using TMPro;
using DG.Tweening;

// 일반 뽑기: NormalCost 골드 소모, 전체 희귀도 풀에서 추첨.
//   - 결과가 Normal 이면 결과창(resultCanvas) 직행
//   - 결과가 Normal 이 아니면 연출창(effectCanvas)로 진입 (UFOButtonEffect가 이어받음)
// 특수 뽑기: SpecialCost 골드 소모, Rare/Epic/Legend 풀에서 추첨 → 항상 연출창(effectCanvas)
// 추첨 결과는 GachaRoller.LastResult 에 저장되어 이후 연출 분기에 사용된다.
public class GachaButtonManager : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private GameObject gatchaCanvas;
    [SerializeField] private GameObject resultCanvas;
    

    [Header("Special Preview")]
    [SerializeField] private GachaTelescopePreview telescopePreview;
[SerializeField] private GameObject effectCanvas;

    [Header("Result")]
    [SerializeField] private ResultCanvasManager resultCanvasManager;

    [Header("Special Windows (다시뽑기 복귀용)")]
    [SerializeField] private GameObject unicornWindow;
    [SerializeField] private GameObject dragonWindow;
    [SerializeField] private GameObject nineTailWindow;

    [Header("Currency")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private int normalCost = 1000;
    [SerializeField] private int specialCost = 5000;

    [Header("Fade")]
    [SerializeField] private CanvasGroup darkImage;

    [SerializeField] private float fadeDuration = 0.3f;

    private void Start()    
    {
        SoundManager.Instance.Play(SoundManager.ESound.Bgm, "Sounds/BGM/Gatcha");   
    }
    private void OnEnable()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.OnLoaded += UpdateGoldText;

        UpdateGoldText();
    }

    private void OnDisable()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.OnLoaded -= UpdateGoldText;
    }

    // ── 버튼 진입점 ──────────────────────────────────────────────

    public void OnNormalGacha() => TryPull(normalCost, rareOrAbove: false);

    public void OnSpecialGacha() => TryPull(specialCost, rareOrAbove: true);

    // ── 추첨 & 분기 ──────────────────────────────────────────────

    private void TryPull(int cost, bool rareOrAbove)
    {
        var playData = SaveManager.Instance?.Current;
        if (playData == null)
        {
            Debug.LogWarning("[GachaButtonManager] SaveManager 데이터 없음 — 로비를 거쳐 진입해야 합니다.");
            return;
        }

        if (playData.currentGold < cost)
        {
            SoundManager.Instance.Play(SoundManager.ESound.Effect, "Sounds/SFX/UI/GoldShortage");
            Debug.LogWarning($"[GachaButtonManager] 골드 부족 (필요: {cost}, 보유: {playData.currentGold})");
            return;
        }

        CharacterData result = GachaRoller.Roll(rareOrAbove);
        if (result == null)
            return; // 풀이 비었거나 매니저 없음 — 골드 차감 없이 중단

        // 골드 차감 (획득 누계 totalEarnedGold 는 건드리지 않음)
        playData.currentGold -= cost;

        // 이미 보유한 캐릭터면 중복 → 골드 환급, 신규면 해금
        bool isDuplicate = playData.unlockedCharacters.Contains(result.id);
        if (isDuplicate)
            playData.currentGold += Define.GachaDuplicateRefund;
        else
            playData.unlockedCharacters.Add(result.id);

        SaveManager.Instance.SaveLocal();
        SaveManager.Instance.Save();

        GachaRoller.LastResult = result;
        UpdateGoldText();

        Debug.Log(isDuplicate
            ? $"[GachaButtonManager] 중복: {result.displayName} ({result.rarity}) / -{cost}G +{Define.GachaDuplicateRefund}G 환급"
            : $"[GachaButtonManager] 뽑기 결과: {result.displayName} ({result.rarity}) / -{cost}G");

        RouteToPresentation(result, isDuplicate);
    }

    private void RouteToPresentation(CharacterData result, bool isDuplicate)
    {
        // 중복 캐릭터는 희귀도와 무관하게 특수 연출을 생략하고 Normal 흐름으로 보여준다.
        if (isDuplicate || GachaRoller.IsNormal(result))
        {
            // Normal 결과창 직행, 일반 파티클 재생
            SoundManager.Instance.Play(SoundManager.ESound.Effect,
                isDuplicate ? "Sounds/SFX/Gatcha/DuplicateResult" : "Sounds/SFX/Gatcha/NormalResult");
            ChangeScreen(resultCanvas, () => resultCanvasManager?.OpenNormalResult());
        }
        else
            PlaySpecialPresentation();
    }

    // ── 다시뽑기: 가챠 메인 화면으로 복귀 ──────────────────────────

    public void ReturnToGacha()
    {
        if (resultCanvas != null)   resultCanvas.SetActive(false);
        if (effectCanvas != null)   effectCanvas.SetActive(false);
        if (unicornWindow != null)  unicornWindow.SetActive(false);
        if (dragonWindow != null)   dragonWindow.SetActive(false);
        if (nineTailWindow != null) nineTailWindow.SetActive(false);

        if (darkImage != null) darkImage.alpha = 0f;

        if (gatchaCanvas != null) gatchaCanvas.SetActive(true);

        UpdateGoldText();
    }

    // ── 화면 전환 ────────────────────────────────────────────────

    private void ChangeScreen(GameObject targetCanvas, TweenCallback onComplete)
    {
        darkImage.alpha = 0f;

        darkImage
            .DOFade(1f, fadeDuration)
            .OnComplete(() =>
            {
                targetCanvas.SetActive(true);
                gatchaCanvas.SetActive(false);
                onComplete?.Invoke();
            });
    }

    // ── 재화 표시 ────────────────────────────────────────────────

    private void UpdateGoldText()
    {
        if (goldText == null)
            return;

        long gold = SaveManager.Instance?.Current?.currentGold ?? 0;
        goldText.text = gold.ToString("N0");
    }


private void PlaySpecialPresentation()
    {
        if (telescopePreview != null)
        {
            telescopePreview.Play(OpenEffectCanvasFromPreview);
            return;
        }

        // 프리뷰가 연결되지 않은 경우 기존 흐름으로 안전하게 폴백한다.
        ChangeScreen(effectCanvas, null);
    }

private void OpenEffectCanvasFromPreview()
    {
        if (effectCanvas != null)
            effectCanvas.SetActive(true);

        if (gatchaCanvas != null)
            gatchaCanvas.SetActive(false);

        if (darkImage != null)
            darkImage.alpha = 0f;
    }

}
