using UnityEngine;
using UnityEngine.UI; // UI 요소를 사용하기 위해 꼭 필요합니다!

// 이 스크립트는 플레이어의 체력을 관리하고 체력 바 UI를 업데이트합니다.
public class PlayerHealth : MonoBehaviour
{
    // 유니티 에디터에서 설정할 변수들
    public int maxHealth = 100;
    // public Slider healthSlider; // 이제 Inspector에서 연결할 필요가 없습니다.

    private Slider healthSlider; // 스크립트가 직접 찾아서 할당할 변수
    private int currentHealth;

    // 게임이 시작될 때 한 번만 호출됩니다.
    void Start()
    {
        // "HealthSlider" 태그를 가진 게임 오브젝트를 찾아서 Slider 컴포넌트를 가져옵니다.
        healthSlider = GameObject.FindWithTag("HealthSlider").GetComponent<Slider>();

        // --- 이 아래는 기존 코드와 동일합니다 ---
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
    }

    // 테스트를 위해 매 프레임마다 호출됩니다.
    void Update()
    {
        // 테스트용: 스페이스바를 누르면 체력이 10씩 감소합니다.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(10);
        }
    }

    // 피해를 받는 함수
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        healthSlider.value = currentHealth;
        Debug.Log("플레이어 체력: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 사망 처리 함수
    void Die()
    {
        Debug.Log("플레이어가 사망했습니다!");
    }
}
