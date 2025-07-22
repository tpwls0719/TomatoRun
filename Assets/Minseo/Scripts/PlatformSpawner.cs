using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [Header("플랫폼 프리팹")]
    public GameObject longPlatformPrefab;   // 긴 플랫폼 프리팹
    public GameObject shortPlatformPrefab;  // 짧은 플랫폼 프리팹
    [Range(0f, 1f)]
    public float longPlatformChance = 0.5f; // 긴 플랫폼 선택 확률

    public int count = 6;   // 생성할 발판의 개수

    public float fixedSpawnInterval = 2.0f;  // 고정 발판 생성 간격 (사용 안함)
    public float platformDistance = 4f;      // 플랫폼 간 거리 (일정하게 유지)
    public float spawnTriggerX = 8f;         // 플랫폼 끝이 이 X위치에 오면 새 플랫폼 생성
    private float nextSpawnTime;             // 다음 스폰 시간 (첫 플랫폼용)
    private float lastPlatformRightEdge = 0f;

    public float yMin = -5.0f;
    public float yMax = -1.0f;
    private float xPos = 12f;
    private GameObject[] platforms;
    private int currentIndex = 0;
    private Vector2 poolPosition = new Vector2(0, 25f);

    // 비활성화된 플랫폼을 찾는 메서드
    private int FindInactivePlatform()
    {
        for (int i = 0; i < platforms.Length; i++)
        {
            if (platforms[i] != null && !platforms[i].activeSelf)
            {
                return i;
            }
        }
        return -1;
    }

    void Start()
    {
        platforms = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            GameObject selectedPrefab = (Random.value < longPlatformChance) ? longPlatformPrefab : shortPlatformPrefab;
            platforms[i] = Instantiate(selectedPrefab, poolPosition, Quaternion.identity);
        }

        nextSpawnTime = Time.time + 0.5f;
    }

    void Update()
    {
        // if (GameManager.instance.isGameOver) return;

        if (Time.time >= nextSpawnTime)
        {
            float yPos = Random.Range(yMin, yMax);
            float newXPos = xPos;
            int platformIndex = FindInactivePlatform();
            GameObject spawnedPlatform = null;

            if (platformIndex != -1 && platforms[platformIndex] != null)
            {
                platforms[platformIndex].SetActive(true);
                platforms[platformIndex].transform.position = new Vector2(newXPos, yPos);
                spawnedPlatform = platforms[platformIndex];
            }
            else
            {
                if (platforms[currentIndex] != null)
                {
                    // 플랫폼만 위치 이동 (SetActive 제거로 자식 아이템 보호)
                    platforms[currentIndex].transform.position = new Vector2(newXPos, yPos);
                    platforms[currentIndex].SetActive(true);  // 마지막에만 활성화
                    spawnedPlatform = platforms[currentIndex];
                }
                currentIndex = (currentIndex + 1) % count;
            }

            nextSpawnTime = Time.time + fixedSpawnInterval;

            if (spawnedPlatform != null && ItemSpawner.Instance != null)
            {
                Collider2D platformCol = spawnedPlatform.GetComponent<Collider2D>();
                Vector2 platformPos = spawnedPlatform.transform.position;
                Vector2 platformSize = platformCol ? platformCol.bounds.size : Vector2.one;

                ItemSpawner.Instance.SpawnItemsOnPlatform(platformPos, platformSize, spawnedPlatform);
            }
        }
    }
}
