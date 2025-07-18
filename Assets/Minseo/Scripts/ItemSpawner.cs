using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("아이템 프리팹")]
    public GameObject waterDropPrefab;
    public GameObject pillPrefab;

    [Header("스폰 설정")]
    public int count = 30; // 풀링 수
    public float pillSpawnChance = 0.15f; // 알약 스폰 확률 (15%)

    private GameObject[] waterDrops;
    private GameObject[] pills;
    private int currentIndex = 0;
    private int currentPillIndex = 0;
    private Vector2 poolPosition = new Vector2(0, 25f); // 풀링 대기 위치

    public static ItemSpawner Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Debug.Log($"[ItemSpawner] 초기화 시작 - waterDropPrefab: {waterDropPrefab}, pillPrefab: {pillPrefab}");
        
        // 물방울 풀 생성
        if (waterDropPrefab == null)
        {
            Debug.LogError("[ItemSpawner] waterDropPrefab이 null입니다! Inspector에서 설정해주세요.");
            return;
        }
        
        waterDrops = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            waterDrops[i] = Instantiate(waterDropPrefab, poolPosition, Quaternion.identity);
            waterDrops[i].SetActive(false);
        }

        // 알약 풀 생성
        if (pillPrefab != null)
        {
            pills = new GameObject[count / 3]; // 물방울보다 적게
            for (int i = 0; i < pills.Length; i++)
            {
                pills[i] = Instantiate(pillPrefab, poolPosition, Quaternion.identity);
                pills[i].SetActive(false);
            }
            Debug.Log($"[ItemSpawner] 알약 풀 생성됨: {pills.Length}개");
        }
        
        Debug.Log($"[ItemSpawner] 초기화 완료 - 물방울: {waterDrops.Length}개, 알약: {(pills?.Length ?? 0)}개");
    }

    public void SpawnItemsOnPlatform(GameObject platform)
    {
        Debug.Log($"[ItemSpawner] 스폰 요청됨 - 플랫폼: {platform.name}");
        
        if (waterDrops == null || waterDrops.Length == 0)
        {
            Debug.LogError("[ItemSpawner] waterDrops 배열이 null이거나 비어있습니다!");
            return;
        }
        
        Collider2D platformCol = platform.GetComponent<Collider2D>();
        if (platformCol == null) 
        {
            Debug.LogError($"[ItemSpawner] 플랫폼 {platform.name}에 Collider2D가 없습니다!");
            return;
        }

        float platformWidth = platformCol.bounds.size.x;
        Debug.Log($"[ItemSpawner] 플랫폼 크기: {platformWidth}");

        // 플랫폼에 있는 장애물 찾기
        Transform obstacle = FindObstacleOnPlatform(platform);
        
        // 샘플 아이템 크기 측정
        GameObject sample = waterDrops[0];
        if (sample == null)
        {
            Debug.LogError("[ItemSpawner] 첫 번째 물방울이 null입니다!");
            return;
        }
        
        sample.SetActive(true);
        Collider2D itemCol = sample.GetComponent<Collider2D>();
        float itemWidth = itemCol ? itemCol.bounds.size.x : 1f;
        float itemHeight = itemCol ? itemCol.bounds.size.y : 1f;
        sample.SetActive(false);
        
        Debug.Log($"[ItemSpawner] 아이템 크기: {itemWidth} x {itemHeight}");

        // 장애물이 있으면 좌우로 분할해서 배치, 없으면 전체에 배치
        if (obstacle != null)
        {
            Debug.Log($"[ItemSpawner] 장애물 발견: {obstacle.name}");
            SpawnItemsAroundObstacle(platform, obstacle, platformWidth, itemWidth, itemHeight);
        }
        else
        {
            Debug.Log("[ItemSpawner] 장애물 없음 - 전체 배치");
            SpawnItemsOnFullPlatform(platform, platformWidth, itemWidth, itemHeight);
        }

        Debug.Log($"[스폰 완료] 플랫폼에 아이템 배치됨 (장애물 {(obstacle != null ? "있음" : "없음")})");
    }

    Transform FindObstacleOnPlatform(GameObject platform)
    {
        // 플랫폼의 자식 중에서 장애물 찾기
        for (int i = 0; i < platform.transform.childCount; i++)
        {
            Transform child = platform.transform.GetChild(i);
            if (child.name.Contains("Obstacle") || child.CompareTag("Obstacle"))
            {
                return child;
            }
        }
        return null;
    }

    void SpawnItemsOnFullPlatform(GameObject platform, float platformWidth, float itemWidth, float itemHeight)
    {
        float spacing = itemWidth * 0.1f;
        int maxItems = Mathf.FloorToInt(platformWidth / (itemWidth + spacing));
        maxItems = Mathf.Max(maxItems, 1);

        SpawnItemsInRow(platform, maxItems, 0f, itemWidth, itemHeight, spacing);
    }

    void SpawnItemsAroundObstacle(GameObject platform, Transform obstacle, float platformWidth, float itemWidth, float itemHeight)
    {
        Collider2D obstacleCol = obstacle.GetComponent<Collider2D>();
        if (obstacleCol == null) return;

        float obstacleWidth = obstacleCol.bounds.size.x;
        float obstacleLocalX = obstacle.localPosition.x;
        
        float spacing = itemWidth * 0.1f;
        
        // 좌측 영역
        float leftAreaWidth = (platformWidth / 2f) + obstacleLocalX - (obstacleWidth / 2f) - spacing;
        if (leftAreaWidth > itemWidth)
        {
            int leftItems = Mathf.FloorToInt(leftAreaWidth / (itemWidth + spacing));
            if (leftItems > 0)
            {
                float leftCenterX = -(platformWidth / 4f) + (obstacleLocalX - obstacleWidth / 2f) / 2f;
                SpawnItemsInRow(platform, leftItems, leftCenterX, itemWidth, itemHeight, spacing);
            }
        }

        // 우측 영역
        float rightAreaWidth = (platformWidth / 2f) - obstacleLocalX - (obstacleWidth / 2f) - spacing;
        if (rightAreaWidth > itemWidth)
        {
            int rightItems = Mathf.FloorToInt(rightAreaWidth / (itemWidth + spacing));
            if (rightItems > 0)
            {
                float rightCenterX = (platformWidth / 4f) + (obstacleLocalX + obstacleWidth / 2f) / 2f;
                SpawnItemsInRow(platform, rightItems, rightCenterX, itemWidth, itemHeight, spacing);
            }
        }
    }

    void SpawnItemsInRow(GameObject platform, int itemCount, float centerX, float itemWidth, float itemHeight, float spacing)
    {
        Debug.Log($"[ItemSpawner] 행 스폰 시작 - 아이템 수: {itemCount}, 중심X: {centerX}");
        
        Collider2D platformCol = platform.GetComponent<Collider2D>();
        
        for (int i = 0; i < itemCount; i++)
        {
            // 확률적으로 물방울 또는 알약 선택
            bool spawnPill = (Random.Range(0f, 1f) < pillSpawnChance) && (pills != null);
            
            GameObject item;
            if (spawnPill && currentPillIndex < pills.Length)
            {
                item = pills[currentPillIndex];
                currentPillIndex = (currentPillIndex + 1) % pills.Length;
                Debug.Log($"[ItemSpawner] 알약 스폰 - 인덱스: {currentPillIndex}");
            }
            else
            {
                item = waterDrops[currentIndex];
                currentIndex = (currentIndex + 1) % count;
                Debug.Log($"[ItemSpawner] 물방울 스폰 - 인덱스: {currentIndex}");
            }
            
            if (item == null)
            {
                Debug.LogError($"[ItemSpawner] 아이템이 null입니다! (인덱스: {i})");
                continue;
            }
            
            item.SetActive(false);
            item.SetActive(true);
            item.transform.SetParent(platform.transform);

            // 위치 계산
            float totalWidth = itemCount * (itemWidth + spacing) - spacing;
            float startX = centerX - totalWidth / 2f + itemWidth / 2f;
            float x = startX + i * (itemWidth + spacing);
            float y = (platformCol.bounds.size.y / 2f) + (itemHeight / 2f) + 0.5f;

            item.transform.localPosition = new Vector3(x, y, 0);
            Debug.Log($"[ItemSpawner] 아이템 {i} 배치 완료 - 위치: ({x}, {y})");
        }
        
        Debug.Log($"[ItemSpawner] 행 스폰 완료 - {itemCount}개 아이템");
    }
}
