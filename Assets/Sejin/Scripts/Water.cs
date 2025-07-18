using UnityEngine;

public class Water : MonoBehaviour
{
    public int scoreValue = 100;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("💧 Player가 물과 충돌 - 점수 증가 + 오브젝트 제거");

            //GameManager.instance.AddScore(scoreValue);
            Destroy(gameObject);
        }
    }
}
