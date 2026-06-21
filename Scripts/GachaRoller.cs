using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 가챠 씬 전용 골드 기반 캐릭터 추첨기.
/// CharacterManager 로스터(acquisitionType == "Gacha")에서 RarityConfig 가중치 비율을 유지한 채 1명을 뽑는다.
/// 희귀도별 확률 = RarityConfig 설정 비율 (같은 희귀도 인원수로 정규화하여 비율을 보존).
/// 결과는 LastResult에 저장되어 GachaButtonManager / UFOButtonEffect가 연출 분기에 사용한다.
/// </summary>
public static class GachaRoller
{
    private const string GachaAcquisition = "Gacha";

    /// <summary>가장 최근 추첨 결과. 연출(UFO/특수창)에서 어떤 캐릭터인지 판별하는 데 사용.</summary>
    public static CharacterData LastResult { get; set; }

    /// <summary>유니콘 캐릭터 여부 (특수 연출 분기용).</summary>
    public static bool IsUnicorn(CharacterData data) =>
        data != null && data.type == "Unicorn";

    /// <summary>Normal 희귀도 여부 (결과창 직행 분기용).</summary>
    public static bool IsNormal(CharacterData data) =>
        data != null && data.rarity == "Normal";

    /// <summary>
    /// 추첨을 실행한다.
    /// </summary>
    /// <param name="rareOrAbove">true면 Rare/Epic/Legend 풀(특수 뽑기), false면 전체 희귀도 풀(일반 뽑기).</param>
    /// <returns>뽑힌 캐릭터. 매니저가 없거나 풀이 비어 있으면 null.</returns>
    public static CharacterData Roll(bool rareOrAbove)
    {
        var cm = CharacterManager.Instance;
        if (cm == null)
        {
            Debug.LogWarning("[GachaRoller] CharacterManager.Instance 없음 — 로비를 거쳐 진입해야 합니다.");
            return null;
        }

        IEnumerable<CharacterData> query =
            cm.All.Where(c => c.acquisitionType == GachaAcquisition);

        if (rareOrAbove)
            query = query.Where(c => c.rarity is "Rare" or "Epic" or "Legend");

        var pool = query.ToList();
        if (pool.Count == 0)
        {
            Debug.LogWarning($"[GachaRoller] 뽑을 수 있는 캐릭터가 없습니다. (rareOrAbove: {rareOrAbove})");
            return null;
        }

        return WeightedRandom(pool);
    }

    // 같은 희귀도 아이템 수로 나눠 정규화: 희귀도별 확률 = 가중치 설정값(%)과 일치
    private static CharacterData WeightedRandom(List<CharacterData> pool)
    {
        var rarityCounts = new Dictionary<string, int>();
        foreach (var e in pool)
        {
            rarityCounts.TryGetValue(e.rarity, out int c);
            rarityCounts[e.rarity] = c + 1;
        }

        float total = pool.Sum(e => RarityConfig.GetGachaWeight(e.rarity) / rarityCounts[e.rarity]);
        float roll = Random.Range(0f, total);
        float cumulative = 0f;

        foreach (var entry in pool)
        {
            cumulative += RarityConfig.GetGachaWeight(entry.rarity) / rarityCounts[entry.rarity];
            if (roll < cumulative) return entry;
        }

        return pool[^1];
    }
}
