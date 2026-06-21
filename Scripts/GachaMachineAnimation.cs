using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class GachaMachineAnimation : MonoBehaviour
{
    [System.Serializable]
    private class OrbitItem
    {
        public RectTransform item;
        public float angle;
        public float radius;
        public float speed;
        public float wobbleOffset;
    }

    [Header("�ӽ�")]
    [SerializeField] private RectTransform machine;
    [SerializeField] private float shakeAngle = 5f;
    [SerializeField] private float shakeDuration = 0.8f;

    [Header("��ǰ �̵� ����")]
    [SerializeField] private List<RectTransform> moveAreas;

    [Header("���� �̵� �ݰ�")]
    [SerializeField] private float minRadius = 30f;
    [SerializeField] private float maxRadius = 100f;

    [Header("���� �ӵ�")]
    [SerializeField] private float minOrbitSpeed = 20f;
    [SerializeField] private float maxOrbitSpeed = 50f;

    [Header("�ݰ� �ⷷ��")]
    [SerializeField] private float wobbleAmount = 10f;
    [SerializeField] private float wobbleSpeed = 2f;

    private readonly List<OrbitItem> orbitItems = new();

    /// <summary>
    /// ��í �ӽ� ��鸲 �� ��ǰ ���� �ʱ�ȭ
    /// </summary>
    private void Start()
    {        if (machine != null)
            machine.localRotation = Quaternion.identity;

        InitializeItems();
    }

    /// <summary>
    /// �ӽ��� �¿�� ���ϴ�.
    /// </summary>
    private void StartMachineShake()
    {
        machine
            .DORotate(new Vector3(0, 0, shakeAngle), shakeDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    /// <summary>
    /// ��ǰ���� ���� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    private void InitializeItems()
    {
        orbitItems.Clear();

        foreach (RectTransform area in moveAreas)
        {
            foreach (RectTransform item in area)
            {
                OrbitItem orbitItem = new OrbitItem
                {
                    item = item,
                    angle = Random.Range(0f, 360f),
                    radius = Random.Range(minRadius, maxRadius),
                    speed = Random.Range(minOrbitSpeed, maxOrbitSpeed),
                    wobbleOffset = Random.Range(0f, 100f)
                };

                orbitItems.Add(orbitItem);
            }
        }
    }

    /// <summary>
    /// ��ǰ ���� ó��
    /// </summary>
    private void Update()
    {
        foreach (OrbitItem orbit in orbitItems)
        {
            orbit.angle += orbit.speed * Time.deltaTime;

            float currentRadius =
                orbit.radius +
                Mathf.Sin(Time.time * wobbleSpeed + orbit.wobbleOffset)
                * wobbleAmount;

            float rad = orbit.angle * Mathf.Deg2Rad;

            Vector2 pos = new Vector2(
                Mathf.Cos(rad) * currentRadius,
                Mathf.Sin(rad) * currentRadius
            );

            orbit.item.anchoredPosition = pos;

            orbit.item.localRotation =
                Quaternion.Euler(
                    0,
                    0,
                    Mathf.Sin(Time.time * 3f + orbit.wobbleOffset) * 10f
                );
        }
    }

    private void OnDisable()
    {
        DOTween.Kill(machine);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        foreach (RectTransform area in moveAreas)
        {
            if (area == null) continue;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(area.position, maxRadius);
        }
    }
#endif
}