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
    public int waterDropScore = 10;  // 물방울당 점수
    private int currentScore = 0;
    
    [Header("생명 시스템")]
    public Image[] heartImages;      // 하트 이미지 배열
    public int maxHearts = 3;        // 최대 생명 수
    public int currentHearts = 3;    // 현재 생명 수
    
    [Header("무적 모드")]
    public InvincibilityItem invincibilityController;  // 무적 모드 컨트롤러
    
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
        UpdateScore(0);
        
        // 초기 생명 설정
        UpdateHeartDisplay();
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
        
        // 점수 텍스트 업데이트
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }
    
    // 물방울 아이템 획득 처리
    public void CollectWaterDrop()
    {
        // 물방울 점수 추가
        UpdateScore(waterDropScore);
    }
    
    // 알약(무적) 아이템 획득 처리
    public void CollectPill()
    {
        // 무적 모드 활성화
        if (invincibilityController != null)
        {
            invincibilityController.ActivateInvincibility();
        }
    }
    
    // 햇빛(생명) 아이템 획득 처리
    public void CollectSunlight()
    {
        // 최대 생명 수를 초과하지 않도록 생명 추가
        if (currentHearts < maxHearts)
        {
            currentHearts++;
            UpdateHeartDisplay();
        }
    }
    
    // 장애물 충돌 처리 (생명 감소)
    public void TakeDamage()
    {
        // 무적 상태가 아닐 때만 데미지 처리
        if (invincibilityController == null || !invincibilityController.IsInvincible)
        {
            currentHearts--;
            UpdateHeartDisplay();
            
            // 생명이 0이 되면 게임 오버
            if (currentHearts <= 0)
            {
                GameOver();
            }
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
        Debug.Log("게임 오버!");
        // 게임 오버 처리 (예: 게임 오버 화면 표시, 재시작 등)
        // GameManager가 있다면 연결: GameManager.Instance.GameOver();
    }
}
