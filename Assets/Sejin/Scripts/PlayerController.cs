using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //public AudioClip deathClip;

    public float jumpForce = 500f;

    private int jumpCount = 0;
    private bool isGrounded = false;
    private bool isDead = false;

    private Rigidbody2D playerRigidbody;
    private Animator animator;
    private AudioSource playerAudio;
    
    public int maxHealth = 3;
    private int currentHealth;
    void Start()
    {
        //게임 오브젝트로부터 사용할 컴포넌트들을 가져와 변수에 할당
        playerRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        //playerAudio = GetComponent<AudioSource>();

        currentHealth = maxHealth;
    }
    
    // 플레이어 상태를 초기화하는 메서드 (GameManager에서 호출)
    public void ResetPlayerState()
    {
        Debug.Log("플레이어 상태 초기화 시작");
        
        // 기본 상태 초기화
        jumpCount = 0;
        isGrounded = false;
        isDead = false;
        currentHealth = maxHealth;
        
        // 물리 상태 초기화
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
        }
        
        // 애니메이터 상태 초기화
        if (animator != null)
        {
            animator.SetBool("Grounded", isGrounded);
            // 사망 상태에서 일반 상태로 복귀 (필요한 경우)
            animator.ResetTrigger("Die");
        }
        
        Debug.Log("플레이어 상태 초기화 완료 - 체력: " + currentHealth + "/" + maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
        {
            //사망 시 처리를 더 이상 진행하지 않고 종료
            return;
        }

        //마우스 왼쪽 버튼을 눌르고 최대 점프 횟수에 도달하지 않았다면
        if (Input.GetMouseButtonDown(0) && jumpCount < 2)
        {
            //점프 횟수 증가
            jumpCount++;
            //점프 직전에 속도를 순간적으로 제로로 변경
            playerRigidbody.linearVelocity = Vector2.zero;
            //리지드바디에 위쪽으로 힘을 주기
            playerRigidbody.AddForce(new Vector2(0, jumpForce));
            //오디오 소스 재생
            //playerAudio.Play();
        }
        //애니메이터의 Grounded 파라미터를 isGrounded 값으로 갱신
        animator.SetBool("Grounded", isGrounded);

    }
    private void Die()
    {
        //애니메이터의 Die 트리거 파라미터를 셋
        animator.SetTrigger("Die");

        //오디오 소스에 할당된 오디오 클립을 deathClip으로 변경
        //playerAudio.clip = deathClip;
        //사망 효과음 재생
        //playerAudio.Play();

        // 속도를 제로(0, 0)로 변경
        playerRigidbody.linearVelocity = Vector2.zero;
        // 사망 상태를 true로 변경
        isDead = true;

        // 게임 매니저의 게임 오버 처리 실행
        GameManager.Instance.GameOver();
    }

    private void TakeDamage(int damage)
    {
        Debug.Log("TakeDamage 메서드 호출됨! 데미지: " + damage);
        
        // 무적 상태 확인
        InvincibilityItem invincibilityController = GetComponent<InvincibilityItem>();
        if (invincibilityController != null && invincibilityController.IsInvincible)
        {
            Debug.Log("무적 상태이므로 데미지를 받지 않습니다!");
            return;
        }
        
        Debug.Log("장애물과 충돌! UIManager로 데미지 처리");

        // UIManager를 통해 하트 UI 업데이트 (UIManager에서 하트 개수와 게임오버 관리)
        if (UIManager.Instance != null)
        {
            Debug.Log("UIManager.Instance 찾음. TakeDamage 호출");
            UIManager.Instance.TakeDamage();
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다! UIManager가 씬에 있는지 확인하세요.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D 호출됨! 충돌한 오브젝트: " + other.name + ", 태그: " + other.tag);
        
        if (other.tag == "Dead" && !isDead)
        {
            // 충돌한 상대방의 태그가 Dead이며 아직 사망하지 않았다면 Die() 실행
            Debug.Log("죽음");
            Die();
        }
        else if (other.tag == "Hit" && !isDead)
        {
            Debug.Log("Hit 태그 장애물과 충돌! TakeDamage 호출");
            TakeDamage(1); // 체력 1 깎기
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 어떤 콜라이더와 닿았으며, 충돌 표면이 위쪽을 보고 있으면
        if (collision.contacts[0].normal.y > 0.7f)
        {
            // isGrounded를 true로 변경하고, 누적 점프 횟수를 0으로 리셋
            isGrounded = true;
            jumpCount = 0;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // 어떤 콜라이더에서 떼어진 경우 isGrounded를 false로 변경
        isGrounded = false; 
    }
}
