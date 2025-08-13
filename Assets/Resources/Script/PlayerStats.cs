using UnityEngine;

public class PlayerStats : CharacterStats
{
    [Header("플레이어 기본 능력치")]
    public int baseAttackPower = 10;
    public float baseAttackSpeed = 1.0f;
    public float baseAttackRange = 1.5f;
    public float baseMoveSpeed = 5f;
    public float AttackCooldown = 2f;

    [Header("최종(Final) 능력치")]
    public int finalAttackPower;
    public float finalAttackSpeed;
    public float finalAttackRange;
    public float finalMoveSpeed;
    public float finalItemDropRateBonus;

    [Header("데미지 텍스트")]
    public GameObject damageTextPrefab;
    public Transform damageTextSpawnPoint;

    private EquipmentManager equipmentManager;
    void Awake()
    {
        equipmentManager = GetComponent<EquipmentManager>();
        CurrentHP = MaxHP; // 부모의 변수를 그대로 사용
    }
    void Start()
    {
        // Start에서 한 번 계산해주고, 장비가 바뀔 때마다 다시 호출됩니다.
        CalculateFinalStats();
    }

    // 모든 장비의 능력치를 합산하여 최종 능력치를 계산하는 함수
    public void CalculateFinalStats()
    {
        // 1. 모든 능력치를 기본값으로 초기화
        finalAttackPower = baseAttackPower;
        finalAttackSpeed = baseAttackSpeed;
        finalAttackRange = baseAttackRange;
        finalMoveSpeed = baseMoveSpeed;
        finalItemDropRateBonus = 0f;
        // ... 다른 스탯들도 초기화 ...

        // 2. 장착한 모든 장비를 순회하며 능력치를 더함
        if (equipmentManager != null)
        {
            foreach (var item in equipmentManager.equippedItems.Values)
            {
                if (item is EquipmentData equip)
                {
                    finalAttackPower += equip.attackPower;
                    finalAttackPower += equip.additionalAttackPower;
                    finalAttackSpeed += equip.attackSpeed;
                    finalAttackSpeed *= (1 + equip.additionalAttackSpeed / 100f);
                    finalAttackRange += equip.attackRange;
                    finalMoveSpeed += equip.moveSpeedBonus;
                    finalItemDropRateBonus += equip.itemDropRateBonus;
                }
            }
        }

        Debug.Log("능력치 재계산 완료! 최종 공격력: " + finalAttackPower);
    }
    protected override void Die()
    {
        // 아무것도 하지 않음 (또는 게임오버 로직)
        Debug.Log("플레이어는 죽지 않습니다.");
    }

}
