using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("기본 능력치")]
    public int Level = 0;
    public int MaxHP = 100;
    public int CurrentHP = 100;

    protected bool isDead = false; // 사망 상태 중복 실행 방지

    public virtual void TakeDamage(int baseDamage, int attackerLevel)
    {
        if (isDead) return; // 이미 죽었다면 피해를 받지 않음

        // --- ▼ 레벨 차이 데미지 보정 로직 수정 ▼ ---
        int levelDifference = attackerLevel - this.Level;
        float damageModifier;

        if (levelDifference <= -10)
        {
            damageModifier = 0f; // 조건 1: 10레벨 이상 낮으면 데미지 0배
        }
        else if (levelDifference >= 10)
        {
            damageModifier = 2f; // 조건 2: 10레벨 이상 높으면 데미지 2배 고정
        }
        else
        {
            damageModifier = 1.0f + (levelDifference * 0.1f); // 조건 3: 그 외에는 레벨당 10% 증감
        }
        // --- ▲ 로직 끝 ▲ ---


        int finalDamage = Mathf.RoundToInt(baseDamage * damageModifier);

        CurrentHP -= finalDamage;
        CurrentHP = Mathf.Clamp(CurrentHP, 0, MaxHP);
        Debug.Log($"{gameObject.name}이(가) {finalDamage} 피해를 입음. (보정: {damageModifier * 100}%)");


        if (CurrentHP <= 0)
        {
            Die();

        }
    }
    protected virtual void Die()
    {
        if (isDead) return; // Die 함수가 여러 번 호출되는 것을 방지
        isDead = true;

        Debug.Log($"{gameObject.name}이(가) 쓰러졌습니다.");
    }

}