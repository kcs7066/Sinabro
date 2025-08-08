using UnityEngine;
using System.Collections; // 코루틴을 사용하기 위해 필요

public class FieldItem : MonoBehaviour
{
    public ItemData itemData;
    private Collider2D col; // 콜라이더 참조

    [Header("드롭 애니메이션 효과")]
    public Transform spriteHolder; // 2단계에서 만든 SpriteHolder 연결
    public float dropHeight = 1.5f; // 튀어 오르는 높이
    public float dropDuration = 0.5f; // 떨어지는 데 걸리는 시간

    private Vector3 initialWorldPosition;

    private Rigidbody2D rb; // Rigidbody 참조 변수

    // 아이템이 이미 주워졌는지 확인하는 플래그
    private bool isPickedUp = false;
    void Awake()
    {
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>(); // Awake에서 Rigidbody 컴포넌트 찾아두기
    }

    public void Setup(ItemData data, Vector3 spawnPosition)
    {
        itemData = data;

        if (spriteHolder != null)
        {
            spriteHolder.GetComponent<SpriteRenderer>().sprite = data.itemIcon;
        }
        else // SpriteHolder가 없다면 자기 자신의 SpriteRenderer를 사용
        {
            GetComponent<SpriteRenderer>().sprite = data.itemIcon;
        }

        // 전달받은 월드 위치를 저장
        initialWorldPosition = spawnPosition;

        // 드롭 애니메이션 시작
        StartCoroutine(DropAnimation());

        // ▼▼▼ 자동 습득 코루틴 시작 ▼▼▼
        StartCoroutine(AutoPickupAfterDelay(5f));
    }


    // 지정된 시간 뒤에 자동으로 아이템을 줍는 코루틴
    IEnumerator AutoPickupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        BePickedUp();
    }

    // 아이템이 주워졌을 때 호출되는 공개 함수
    public void BePickedUp()
    {
        // 이미 주워졌다면 아무것도 하지 않음 (중복 실행 방지)
        if (isPickedUp) return;
        isPickedUp = true;

        // "Player" 태그를 가진 오브젝트를 찾아 Controller 스크립트를 가져옴
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        if (player != null)
        {
            // 플레이어에게 아이템 획득 로직을 실행하라고 명령
            player.AcquireItem(itemData);
        }

        // 아이템 오브젝트 파괴
        Destroy(gameObject);
    }

    IEnumerator DropAnimation()
    {
        if (spriteHolder == null) yield break; // SpriteHolder가 없으면 애니메이션 실행 안 함

        // 애니메이션 동안에는 주울 수 없도록 콜라이더 비활성화
        if (col != null) col.enabled = false;

        // 기준점을 월드 위치로 설정
        Vector3 floorPos = initialWorldPosition;
        Vector3 peakPos = floorPos + Vector3.up * dropHeight;

        float timeElapsed = 0;
        float halfDuration = dropDuration / 2;

        // 1. 위로 솟아오르기 (크기 커지기)
        while (timeElapsed < halfDuration)
        {
            // localPosition 대신 position을 직접 제어
            spriteHolder.position = Vector3.Lerp(floorPos, peakPos, timeElapsed / halfDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // 2. 아래로 떨어지기 (크기 작아지기)
        timeElapsed = 0;
        while (timeElapsed < halfDuration)
        {
            // localPosition 대신 position을 직접 제어
            spriteHolder.position = Vector3.Lerp(peakPos, floorPos, timeElapsed / halfDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        spriteHolder.position = floorPos; // 정확한 최종 위치 보정

        // 애니메이션이 끝나면 다시 주울 수 있도록 콜라이더 활성화
        if (col != null) col.enabled = true;

        // ▼▼▼ 물리 효과를 꺼서 튕겨나가지 않게 함 ▼▼▼
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;
    }

}