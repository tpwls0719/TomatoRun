using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("아이템 프리팹")]
    public GameObject waterDropPrefab;

    [Header("스폰 설정")]
    public int count = 30; // 풀링 수

    private GameObject[] waterDrops;
    private int currentIndex = 0;
    private Vector2 poolPosition = new Vector2(0, 25f); // 풀링 대기 위치

    public static ItemSpawner Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        waterDrops = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            waterDrops[i] = Instantiate(waterDropPrefab, poolPosition, Quaternion.identity);
            waterDrops[i].SetActive(false);
        }
    }

    public void SpawnItemsOnPlatform(GameObject platform)
    {
        Collider2D platformCol = platform.GetComponent<Collider2D>();
        if (platformCol == null) return;

        float platformWidth = platformCol.bounds.size.x;

        // 자식 Visual 오브젝트 기준으로 Collider 가져오기
        GameObject sample = waterDrops[0];
        sample.SetActive(true); // bounds 계산 위해 잠시 활성화
        Collider2D itemCol = sample.GetComponentInChildren<Collider2D>();
        float itemWidth = itemCol ? itemCol.bounds.size.x : 1f;
        float itemHeight = itemCol ? itemCol.bounds.size.y : 1f;
        sample.SetActive(false);

        float spacing = itemWidth * 0.1f;

        int maxItems = Mathf.FloorToInt((platformWidth + spacing) / (itemWidth + spacing));
        maxItems = Mathf.Max(maxItems, 1);

        float totalWidth = maxItems * itemWidth + (maxItems - 1) * spacing;
        float startX = -totalWidth / 2f + itemWidth / 2f;

        for (int i = 0; i < maxItems; i++)
        {
            GameObject drop = waterDrops[currentIndex];
            drop.SetActive(false);
            drop.SetActive(true);

            drop.transform.SetParent(platform.transform, false); // local 기준 고정

            float x = startX + i * (itemWidth + spacing);
            float y = (platformCol.bounds.size.y / 2f) + (itemHeight / 2f) + 0.5f;

            drop.transform.localPosition = new Vector3(x, y, 0);

            currentIndex = (currentIndex + 1) % count;
        }

        Debug.Log($"[스폰 완료] {maxItems}개 물방울 생성됨");
    }
}
