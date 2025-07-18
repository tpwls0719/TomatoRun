using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    public GameObject platformPrefab;  // 생성할 발판의 원본
    public GameObject obstaclePrefab;  // 생성할 장애물의 원본
    public int count = 2;   // 생상할 발판의 개수
    public int obstacleCount = 3;  // 생성할 장애물의 개수

    public float timeBetSpawnMin = 1.25f;  // 발판 생성 간격 최소값
    public float timeBetSpawnMax = 2.5f;  // 발판 생성 간격 최대값
    private float timeBetSpawn;  // 다음 배치까지의 시간 간격

    public float yMin = -3.5f;   // 배치할 위치의 최소 y값
    public float yMax = 1.5f;    // 배치할 위치의 최소 y값
    private float xPos = 20f;  // 배치할 위치의 x값
    private GameObject[] platforms;  // 생성된 발판들을 저장할 배열
    private GameObject[] obstacles;  // 생성된 장애물들을 저장할 배열
    private int currentIndex = 0;  // 현재 발판의 인덱스
    private int currentObstacleIndex = 0;  // 현재 장애물의 인덱스
    private Vector2 poolPosition = new Vector2(0, 25f);  // 발판이 풀링될 위치
    private float lastSpawnTime;  // 마지막 발판 생성 시간
    
    [Header("장애물 설정")]
    public float obstacleSpawnChance = 0.3f;  // 장애물 스폰 확률 (30%)
    public float obstacleHeightOffset = 2f;  // 플랫폼 위 장애물 높이
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // count만큼의 공가을 가지는 새로운 발판 배열 생성
        platforms = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            // platformPrefab을 원본으로 새 발판을 poolPosition 위치에 복제 생성
            // 생성된 발판을 platform 배열에 할당
            platforms[i] = Instantiate(platformPrefab, poolPosition, Quaternion.identity);
        }

        // 장애물 배열 생성
        if (obstaclePrefab != null)
        {
            obstacles = new GameObject[obstacleCount];
            for (int i = 0; i < obstacleCount; i++)
            {
                obstacles[i] = Instantiate(obstaclePrefab, poolPosition, Quaternion.identity);
                obstacles[i].SetActive(false);
            }
        }

        // 마지막 배치 시점 초기화
        lastSpawnTime = 0f;

        // 다음번 배치까지의 시간 간격을 0으로 초기화
        timeBetSpawn = 0;
        
    }

    // Update is called once per frame
    void Update()
    {
        // if (GameManager.instance.isGameOver)
        // {
        //     return;  // 게임이 끝나면 발판 생성 중지
        // }

        if (Time.time >= lastSpawnTime + timeBetSpawn)
        {
            // 기록된 마지막 배치 시점을 현재 시점으로 갱신
            lastSpawnTime = Time.time;

            // 다음 배치까지의 시간 간격을 timeBetSpawnMin과 timeBetSpawnMax 사이의 랜덤 값으로 설정
            timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);

            // 배치할 위치의 높이를 yMin과 yMax 사이의 랜덤 값으로 설정
            float yPos = Random.Range(yMin, yMax);

            // 사용할 현재 순번의 발판 게임 오브젝트를 비활성화하고 즉시 다시 활성화
            // 이때 발판의 Platform 컴포넌트의 OnEnable 메서드가 실행됨
            platforms[currentIndex].SetActive(false);
            platforms[currentIndex].SetActive(true);

            // 현재 순번의 발판을 화면 오른쪽으로 재배치
            platforms[currentIndex].transform.position = new Vector2(xPos, yPos);

            // 아이템 스폰
            if (ItemSpawner.Instance != null)
            {
                ItemSpawner.Instance.SpawnItemsOnPlatform(platforms[currentIndex]);
            }

            // 장애물 스폰 (확률적으로)
            SpawnObstacle(platforms[currentIndex]);

            // 순번 넘기기
            currentIndex++;

            // 마지막 순번에 도달했다면 순번을 리셋
            if (currentIndex >= count)
            {
                currentIndex = 0;
            }
        }
    }

    void SpawnObstacle(GameObject platform)
    {
        // 장애물 프리팹이 없거나 확률에 맞지 않으면 스폰 안함
        if (obstaclePrefab == null || obstacles == null) return;
        
        float randomValue = Random.Range(0f, 1f);
        if (randomValue > obstacleSpawnChance) return;

        // 플랫폼 콜라이더 크기 가져오기
        Collider2D platformCollider = platform.GetComponent<Collider2D>();
        if (platformCollider == null) return;

        // 현재 순번의 장애물 가져오기
        GameObject obstacle = obstacles[currentObstacleIndex];
        
        // 장애물 활성화
        obstacle.SetActive(false);
        obstacle.SetActive(true);
        
        // 플랫폼의 자식으로 설정
        obstacle.transform.SetParent(platform.transform);
        
        // 플랫폼 중앙에 배치
        float localX = 0f; // 플랫폼 중앙
        float localY = (platformCollider.bounds.size.y / 2f) + obstacleHeightOffset;
        
        obstacle.transform.localPosition = new Vector3(localX, localY, 0);
        
        // 장애물 인덱스 순환
        currentObstacleIndex++;
        if (currentObstacleIndex >= obstacleCount)
        {
            currentObstacleIndex = 0;
        }
        
        Debug.Log($"장애물 스폰됨! (확률: {obstacleSpawnChance * 100}%)");
    }
}
