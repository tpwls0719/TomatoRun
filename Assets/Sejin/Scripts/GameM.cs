using UnityEngine;

public class GameM : MonoBehaviour
{
    public static GameM Instance { get; private set; }

    [Header("게임 설정")]
    public int maxHearts = 3;
    public int startingScore = 0;

    private bool isGameOver = false;

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeGame();
    }

    // 게임 초기화
    public void InitializeGame()
    {
        isGameOver = false;

        // UIManager 초기값 세팅
        if (UIManager.Instance != null)
        {
            UIManager1.Instance.currentHearts = maxHearts;
            UIManager1.Instance.maxHearts = maxHearts;
            UIManager1.Instance.UpdateHeartDisplay();

            UIManager1.Instance.UpdateScore(-UIManager1.Instance.currentScore + startingScore); // 점수 초기화
        }
    }

    // 장애물에 맞았을 때 데미지 처리
    public void PlayerTakeDamage()
    {
        if (isGameOver) return;

        if (UIManager1.Instance != null)
        {
            UIManager1.Instance.TakeDamage();

            if (UIManager1.Instance.currentHearts <= 0)
            {
                GameOver();
            }
        }
    }

    // 물방울 아이템 획득 시 점수 추가
    public void PlayerCollectWaterDrop()
    {
        if (isGameOver) return;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.CollectWaterDrop();
        }
    }

    // 알약(무적) 아이템 획득
    public void PlayerCollectPill()
    {
        if (isGameOver) return;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.CollectPill();
        }
    }

    // 햇빛(생명) 아이템 획득
    public void PlayerCollectSunlight()
    {
        if (isGameOver) return;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.CollectSunlight();
        }
    }

    // 게임 오버 처리
    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        Debug.Log("게임 오버!");

        // TODO: 게임 오버 UI 띄우기, 사운드 재생 등

        // 예: 3초 후 씬 재시작
        Invoke(nameof(RestartGame), 3f);
    }

    // 게임 재시작
    void RestartGame()
    {
        // 현재 씬 다시 불러오기
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

