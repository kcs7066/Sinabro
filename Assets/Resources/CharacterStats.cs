using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("기본 능력치")]
    public int Level = 0;
    public int MaxHP = 100;
    public int CurrentHP = 100;
    public int AttackPower = 10;

    //[Header("경험치")]
    //public int currentExp = 0;
    //public int expToNextLevel = 100;

    // 공격 처리 예시
    public void TakeDamage(int damage)
    {
        CurrentHP -= damage;
        CurrentHP = Mathf.Clamp(CurrentHP, 0, MaxHP);
        Debug.Log($"{gameObject.name}이(가) {damage} 피해를 입음. 현재 HP: {CurrentHP}");
    }

    //// 레벨업 처리 예시
    //public void GainExp(int exp)
    //{
    //    currentExp += exp;
    //    while (currentExp >= expToNextLevel)
    //    {
    //        currentExp -= expToNextLevel;
    //        LevelUp();
    //    }
    //}

    //void LevelUp()
    //{
    //    Level++;
    //    MaxHP += 20;
    //    AttackPower += 5;
    //    CurrentHP = MaxHP;

    //    expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.2f);

    //    Debug.Log($"레벨업! 현재 레벨: {Level}, HP: {MaxHP}, 공격력: {AttackPower}");
    //}
}