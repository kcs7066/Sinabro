using UnityEngine;
using UnityEngine.UI; // UI 요소를 사용하기 위해 꼭 필요합니다!

// 이 스크립트는 플레이어의 체력을 관리하고 체력 바 UI를 업데이트합니다.
public class PlayerHealth : MonoBehaviour
{
    // 유니티 에디터에서 설정할 변수들
    public int maxHealth = 100; // 최대 체력
    public Slider healthSlider;   // 체력 바 슬라이더 UI

    private int currentHealth;    // 현재 체력

    // 게임이 시작될 때 한 번만 호출됩니다.
    void Start()
    {
        // 현재 체력을 최대 체력으로 초기화합니다.
        currentHealth = maxHealth;

        // 슬라이더의 최대값을 플레이어의 최대 체력과 일치시킵니다.
        healthSlider.maxValue = maxHealth;
        // 슬라이더의 현재 값도 최대 체력으로 설정하여 꽉 찬 상태로 시작합니다.
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
        // 현재 체력에서 받은 피해량을 뺍니다.
        currentHealth -= damage;

        // 체력이 0보다 작아지지 않도록 합니다.
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        // 슬라이더의 값을 현재 체력으로 업데이트합니다.
        healthSlider.value = currentHealth;

        Debug.Log("플레이어 체력: " + currentHealth);

        // 만약 체력이 0 이하라면,
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 사망 처리 함수
    void Die()
    {
        Debug.Log("플레이어가 사망했습니다!");
        // TODO: 여기에 게임 오버 로직이나 부활 로직을 나중에 추가할 수 있습니다.
    }
}
