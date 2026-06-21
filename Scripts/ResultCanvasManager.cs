using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class ResultCanvasManager : MonoBehaviour
{
    [SerializeField] private GameObject resultCanvas;

    [Header("Result Info")]
    [SerializeField] private Image characterCardImage;   // 결과창/BG/캐릭터카드칸 의 Image
    [SerializeField] private TMP_Text goldText;          // 결과창/BG/재화BG 의 골드 텍스트
    [SerializeField] private string lobbySceneName = "LobbyScene";

    [Header("Exit Fade")]
    [SerializeField] private CanvasGroup unicornExitFade;
    [SerializeField] private CanvasGroup dragonExitFade;
    [SerializeField] private CanvasGroup nineTailExitFade;

    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Particle Roots")]
    [SerializeField] private GameObject normalParticleRoot;
    [SerializeField] private GameObject unicornParticleRoot;
    [SerializeField] private GameObject dragonParticleRoot;
    [SerializeField] private GameObject nineTailParticleRoot;

    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem normalParticle;
    [SerializeField] private ParticleSystem unicornParticle;
    [SerializeField] private ParticleSystem dragonParticle;
    [SerializeField] private ParticleSystem nineTailParticle;

    public void OpenNormalResult()
    {
        resultCanvas.SetActive(true);

        ApplyResult();

        PlayParticle(normalParticleRoot, normalParticle);
    }

    public void OpenUnicornResult(GameObject currentCanvas)
    {
        unicornExitFade.alpha = 0f;

        unicornExitFade
            .DOFade(1f, fadeDuration)
            .OnComplete(() =>
            {
                OpenResult(
                    currentCanvas,
                    unicornParticleRoot,
                    unicornParticle);
            });
    }

    public void OpenDragonResult(GameObject currentCanvas)
    {
        dragonExitFade.alpha = 0f;

        dragonExitFade
            .DOFade(1f, fadeDuration)
            .OnComplete(() =>
            {
                OpenResult(
                    currentCanvas,
                    dragonParticleRoot,
                    dragonParticle);
            });
    }

    public void OpenNineTailResult(GameObject currentCanvas)
    {
        nineTailExitFade.alpha = 0f;

        nineTailExitFade
            .DOFade(1f, fadeDuration)
            .OnComplete(() =>
            {
                OpenResult(
                    currentCanvas,
                    nineTailParticleRoot,
                    nineTailParticle);
            });
    }

    private void OpenResult(
        GameObject currentCanvas,
        GameObject particleRoot,
        ParticleSystem particle)
    {
        if (currentCanvas != null)
            currentCanvas.SetActive(false);

        resultCanvas.SetActive(true);

        ApplyResult();

        PlayParticle(particleRoot, particle);
    }

    // 결과창이 열릴 때 방금 뽑은 캐릭터 이미지와 보유 골드를 갱신한다.
    private void ApplyResult()
    {
        CharacterData result = GachaRoller.LastResult;

        if (characterCardImage != null && result != null && result.Sprite != null)
        {
            characterCardImage.sprite  = result.Sprite;
            characterCardImage.enabled = true;
        }

        UpdateGoldText();
    }

    private void UpdateGoldText()
    {
        if (goldText == null)
            return;

        long gold = SaveManager.Instance?.Current?.currentGold ?? 0;
        goldText.text = gold.ToString("N0");
    }

    // 홈 버튼 → 로비 씬으로 이동
    public void GoHome()
    {
        SceneManager.LoadScene(lobbySceneName);
    }

    private void DisableAllParticles()
    {
        if (normalParticleRoot != null)
            normalParticleRoot.SetActive(false);

        if (unicornParticleRoot != null)
            unicornParticleRoot.SetActive(false);

        if (dragonParticleRoot != null)
            dragonParticleRoot.SetActive(false);

        if (nineTailParticleRoot != null)
            nineTailParticleRoot.SetActive(false);
    }

    private void PlayParticle(
        GameObject particleRoot,
        ParticleSystem particle)
    {
        if (particleRoot == null || particle == null)
            return;

        DisableAllParticles();

        particleRoot.SetActive(true);

        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particle.Clear();
        particle.Play();

        Debug.Log("��� ��ƼŬ : " + particle.name);
    }
}