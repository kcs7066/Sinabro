using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("기본 능력치")]
    public int Level = 0;
    public int MaxHP = 100;
    public int CurrentHP = 100;
    public int AttackPower = 10;
    public float AttackRange = 1.5f; // 공격 사거리
    public float AttackCooldown = 1f; // 공격 딜레이

    public void TakeDamage(int damage)
    {
        CurrentHP -= damage;
        CurrentHP = Mathf.Clamp(CurrentHP, 0, MaxHP);
        Debug.Log($"{gameObject.name}이(가) {damage} 피해를 입음. 현재 HP: {CurrentHP}");

        if (CurrentHP <= 0)
        {
            Die();

        }
    }
    private void Die()
    {
        Debug.Log($"{gameObject.name}이(가) 쓰러졌습니다.");
        // 여기에 사망 애니메이션, 아이템 드랍, 오브젝트 파괴 등 로직 추가
        // 예시: 2초 뒤에 오브젝트 비활성화
        gameObject.SetActive(false);
    }

}