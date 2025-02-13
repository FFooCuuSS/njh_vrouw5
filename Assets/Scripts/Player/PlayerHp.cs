using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHP : MonoBehaviour
{
    public GameObject heartPrefab;  // 하트 프리팹
    public GameObject diePanel; // 사망 패널
    public int maxHP = 5;  // 최대 체력
    public int currentHP = 5;  // 현재 체력
    public List<GameObject> heartObjects = new List<GameObject>();  // 하트 GameObject 리스트

    private bool isInvincible = false;  // 무적 상태 여부
    public float invincibilityDuration = 1f;  // 무적 상태 지속 시간
    Rigidbody2D rb;

    void Start()
    {
        CreateHearts();  // 게임 시작 시 하트 객체 생성
        UpdateHearts();  // 초기 하트 이미지 업데이트
        rb=GetComponent<Rigidbody2D>();
    }

    // 하트 프리팹을 이용해 하트 객체 생성
    void CreateHearts()
    {
        for (int i = 0; i < maxHP; i++)
        {
            //GameObject heart = Instantiate(heartPrefab, transform); // 프리팹을 인스턴스화하여 부모로 설정
            //heartObjects.Add(heart);  // 하트 객체 리스트에 추가
        }
    }

    // 체력 감소 처리
    public void TakeDamage(int damage, Vector2 targetpos)
    {
        // 무적 상태일 경우 데미지 무효화
        if (isInvincible) return;

        currentHP -= damage;

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
        // 넉백 방향 계산 (목표와 현재 위치의 차이)
        Vector2 knockbackDirection = ((Vector2)transform.position - targetpos).normalized;

        // 넉백 강도 설정 (값을 조정하여 넉백의 강도를 결정)
        float knockbackStrength = 5f;

        // 현재 Rigidbody2D의 velocity에 반대 방향으로 넉백 적용
        rb.velocity = knockbackDirection * knockbackStrength;


        // 하트 업데이트
        UpdateHearts();

        // 무적 상태 시작
        StartCoroutine(InvincibilityCoroutine());
    }

    // 하트 상태 업데이트
    public void UpdateHearts()
    {
        for (int i = 0; i < heartObjects.Count; i++)
        {
            // 현재 체력에 맞는 하트를 활성화 또는 비활성화
            heartObjects[i].SetActive(i < currentHP);
        }
        if (currentHP <= 1)
        {
            SoundManager.Instance.PlaySFX(1);
        }
    }

    // 플레이어 사망 처리
    public void Die()
    {
        Debug.Log("플레이어 사망");

        if (diePanel != null)
        {
            DiePanel panelScript = diePanel.GetComponent<DiePanel>();
            if (panelScript != null)
            {
                panelScript.Bravo6(); // 애니메이션 실행
            }
            else
            {
                Debug.LogWarning("diePanel에 DiePanel 스크립트가 없음.");
            }
        }
        else
        {
            Debug.LogWarning("diePanel이 할당되지 않았음.");
        }

        Invoke("LoadGameOverScene", 0.5f);
    }

    // 게임 오버 씬 로드
    private void LoadGameOverScene()
    {
        SceneManager.LoadScene("Gameover");
    }

    // 무적 상태 코루틴
    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;  // 무적 상태 시작
        yield return new WaitForSeconds(invincibilityDuration);  // 지정된 시간 동안 대기
        isInvincible = false;  // 무적 상태 종료
    }
}
