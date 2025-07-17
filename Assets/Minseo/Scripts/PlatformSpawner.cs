using UnityEngine;
using System.Collections.Generic;

public class PlatformSpawner : MonoBehaviour
{
    [Header("플랫폼 프리팹")]
    public GameObject[] stage1Platforms = new GameObject[2]; // 스테이지 1 플랫폼 2개
    public GameObject[] stage2Platforms = new GameObject[2]; // 스테이지 2 플랫폼 2개
    public GameObject[] stage3Platforms = new GameObject[2]; // 스테이지 3 플랫폼 2개
    public GameObject[] stage4Platforms = new GameObject[2]; // 스테이지 4 플랫폼 2개
    
    [Header("스폰 설정")]
    public float spawnDistance = 5f; // 플랫폼 간 거리
    public float spawnTimer = 2f; // 스폰 간격 (초)
    public Transform spawnPoint; // 스폰 위치
    
    [Header("현재 스테이지")]
    public int currentStage = 1; // 현재 스테이지 (1~4)
    
    private float timer = 0f;
    private List<GameObject[]> allStagePlatforms;
    
    void Start()
    {
        // 모든 스테이지 플랫폼을 리스트로 관리
        allStagePlatforms = new List<GameObject[]>
        {
            stage1Platforms,
            stage2Platforms,
            stage3Platforms,
            stage4Platforms
        };
        
        // 스폰 포인트가 없으면 현재 위치를 스폰 포인트로 설정
        if (spawnPoint == null)
            spawnPoint = transform;
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= spawnTimer)
        {
            SpawnPlatform();
            timer = 0f;
        }
    }
    
    void SpawnPlatform()
    {
        // 현재 스테이지의 플랫폼 배열 가져오기
        GameObject[] currentPlatforms = GetCurrentStagePlatforms();
        
        if (currentPlatforms == null || currentPlatforms.Length == 0)
        {
            Debug.LogWarning($"스테이지 {currentStage}의 플랫폼이 설정되지 않았습니다!");
            return;
        }
        
        // 랜덤하게 플랫폼 선택 (2개 중 1개)
        int randomIndex = Random.Range(0, currentPlatforms.Length);
        GameObject platformToSpawn = currentPlatforms[randomIndex];
        
        if (platformToSpawn != null)
        {
            // 플랫폼 생성
            GameObject newPlatform = Instantiate(platformToSpawn, spawnPoint.position, spawnPoint.rotation);
            Debug.Log($"스테이지 {currentStage} 플랫폼 {randomIndex + 1} 생성");
        }
        else
        {
            Debug.LogWarning($"스테이지 {currentStage}의 플랫폼 {randomIndex + 1}이 null입니다!");
        }
    }
    
    GameObject[] GetCurrentStagePlatforms()
    {
        // 스테이지 범위 체크 (1~4)
        if (currentStage < 1 || currentStage > 4)
        {
            Debug.LogError($"잘못된 스테이지: {currentStage}. 1~4 사이의 값이어야 합니다.");
            return null;
        }
        
        return allStagePlatforms[currentStage - 1]; // 스테이지는 1부터 시작하지만 배열은 0부터
    }
    
    // 외부에서 스테이지 변경할 때 호출
    public void ChangeStage(int newStage)
    {
        if (newStage >= 1 && newStage <= 4)
        {
            currentStage = newStage;
            Debug.Log($"스테이지 {currentStage}로 변경됨");
        }
        else
        {
            Debug.LogError($"잘못된 스테이지: {newStage}. 1~4 사이의 값만 가능합니다.");
        }
    }
    
    // 테스트용 메서드들
    [ContextMenu("테스트: 플랫폼 즉시 생성")]
    public void TestSpawnPlatform()
    {
        SpawnPlatform();
    }
    
    [ContextMenu("테스트: 스테이지 2로 변경")]
    public void TestChangeToStage2()
    {
        ChangeStage(2);
    }
    
    [ContextMenu("테스트: 스테이지 3으로 변경")]
    public void TestChangeToStage3()
    {
        ChangeStage(3);
    }
    
    [ContextMenu("테스트: 스테이지 4로 변경")]
    public void TestChangeToStage4()
    {
        ChangeStage(4);
    }
}
