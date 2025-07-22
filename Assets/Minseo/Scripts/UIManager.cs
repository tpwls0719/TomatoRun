using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("캐릭터 아이콘 슬라이더")]
    public RectTransform characterIcon;
    public RectTransform sliderBackground;
    
    [Header("점수 시스템")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreText;    // 최고 점수 텍스트
    public int waterDropScore = 10;  // 물방울당 점수
    private int currentScore = 0;
    private int bestScore = 0;       // 최고 점수
    
    [Header("생명 시스템")]
    public Image[] heartImages;      // 하트 이미지 배열
    public int maxHearts = 3;        // 최대 생명 수
    public int currentHearts = 3;    // 현재 생명 수
    
    [Header("무적 모드")]
    public InvincibilityItem invincibilityController;  // 무적 모드 컨트롤러
    
    [Header("게임 오버 UI")]
    public GameObject gameOverUI;        // 게임 오버 UI 패널
    public TextMeshProUGUI gameOverScoreText;  // 게임 오버 시 현재 점수 표시
    
    [Header("설정")]
    public float stageDuration = 30f;
    public int maxStages = 5;
    
    private float gameTime = 0f;
    
    public static UIManager Instance { get; private set; }  // 싱글톤 인스턴스
    
    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // 초기 점수 설정
        LoadBestScore();  // 최고 점수 로드
        UpdateScore(0);
        
        // 초기 생명 설정
        UpdateHeartDisplay();
        
        // 게임 오버 UI 초기화
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }

    void Update()
    {
        // 슬라이더에 캐릭터 아이콘 위치 업데이트
        UpdateIconPosition();
    }
    
    void UpdateIconPosition()
    {
        if (characterIcon == null || sliderBackground == null) return;
        
        gameTime += Time.deltaTime;
        
        // 전체 게임 시간 기준으로 진행률 계산 (0~1)
        float totalTime = stageDuration * maxStages;
        float progress = Mathf.Clamp01(gameTime / totalTime);
        
        // 슬라이더 너비 기준으로 X 위치 계산
        float width = sliderBackground.rect.width;
        float x = (-width / 2f) + (progress * width);
        
        characterIcon.anchoredPosition = new Vector2(x, characterIcon.anchoredPosition.y);
    }
    
    // 점수 업데이트 메서드
    public void UpdateScore(int scoreToAdd)
    {
        currentScore += scoreToAdd;
        
        // 현재 점수 텍스트 업데이트
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
        
        // 최고 점수 갱신 확인
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            SaveBestScore();
            UpdateBestScoreDisplay();
        }
    }
    
    // 최고 점수 로드
    void LoadBestScore()
    {
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        UpdateBestScoreDisplay();
    }
    
    // 최고 점수 저장
    void SaveBestScore()
    {
        PlayerPrefs.SetInt("BestScore", bestScore);
        PlayerPrefs.Save();
    }
    
    // 최고 점수 UI 업데이트
    void UpdateBestScoreDisplay()
    {
        if (bestScoreText != null)
        {
            bestScoreText.text = "Best: " + bestScore.ToString();
        }
    }
    
    // 물방울 아이템 획득 처리
    public void CollectWaterDrop()
    {
        // 물방울 점수 추가
        UpdateScore(waterDropScore);
        Debug.Log($"물방울 획득! +{waterDropScore}점 (현재 점수: {currentScore})");
    }
    
    // 알약(무적) 아이템 획득 처리
    public void CollectPill()
    {
        // InvincibilityItem.cs에서 이미 무적 모드 활성화를 처리하므로 
        // 여기서는 추가 로직만 필요하다면 추가 (현재는 불필요)
        Debug.Log("알약 획득 - UIManager에서 처리됨");
    }
    
    // 햇빛(생명) 아이템 획득 처리
    public void CollectSunlight()
    {
        // 최대 생명 수를 초과하지 않도록 생명 추가
        if (currentHearts < maxHearts)
        {
            currentHearts++;
            UpdateHeartDisplay();
            Debug.Log($"햇빛 획득! 하트 +1 (현재 하트: {currentHearts}/{maxHearts})");
        }
        else
        {
            Debug.Log($"햇빛 획득했지만 하트가 이미 최대입니다! ({currentHearts}/{maxHearts})");
        }
    }
    
    // 장애물 충돌 처리 (생명 감소)
    public void TakeDamage()
    {
        Debug.Log("UIManager.TakeDamage 호출됨!");
        Debug.Log($"invincibilityController null? {invincibilityController == null}");
        if (invincibilityController != null)
        {
            Debug.Log($"무적 상태? {invincibilityController.IsInvincible}");
        }
        
        // 무적 상태가 아닐 때만 데미지 처리
        if (invincibilityController == null || !invincibilityController.IsInvincible)
        {
            currentHearts--;
            UpdateHeartDisplay();
            
            Debug.Log($"하트 감소! 현재 하트: {currentHearts}/{maxHearts}");
            
            // 생명이 0이 되면 게임 오버
            if (currentHearts <= 0)
            {
                GameOver();
            }
        }
        else
        {
            Debug.Log("무적 상태이므로 데미지를 받지 않습니다!");
        }
    }
    
    // 하트 UI 업데이트
    void UpdateHeartDisplay()
    {
        if (heartImages == null || heartImages.Length == 0) return;
        
        // 하트 이미지 활성화/비활성화
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                heartImages[i].enabled = (i < currentHearts);
            }
        }
    }
    
    // 게임 오버 처리
    void GameOver()
    {
        // 최고 점수 최종 저장
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            SaveBestScore();
        }
        
        // 게임 오버 UI에 현재 점수 표시
        UpdateGameOverScore();
        
        // 게임 오버 UI 표시
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
        
        // 게임 시간 정지
        Time.timeScale = 0f;
        
        Debug.Log($"게임 오버! 최종 점수: {currentScore}, 최고 점수: {bestScore}");
    }
    
    // 게임 재시작 (게임 오버 UI에서 호출)
    public void RestartGame()
    {
        // 시간 정지 해제
        Time.timeScale = 1f;
        
        // 현재 점수 리셋
        currentScore = 0;
        UpdateScore(0);
        
        // 하트 복구
        currentHearts = maxHearts;
        UpdateHeartDisplay();
        
        // 게임 오버 UI 숨기기
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
        
        Debug.Log("게임 재시작!");
    }
    
    // 게임 오버 UI에 현재 점수 업데이트
    void UpdateGameOverScore()
    {
        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = currentScore.ToString();
            Debug.Log($"게임 오버 점수 UI 업데이트: {currentScore}점");
        }
        else
        {
            Debug.LogWarning("게임 오버 점수 텍스트가 설정되지 않았습니다!");
        }
    }
}
