// DamageText.cs -> DamageNumber.cs 로 이름 변경 또는 새로 생성
using UnityEngine;
using System.Collections;

public class DamageNumber : MonoBehaviour
{
    [Header("스프라이트 설정")]
    public GameObject digitPrefab;      // 2단계에서 만든 Digit 프리팹
    public Sprite[] numberSprites;    // 0~9 순서로 숫자 스프라이트 배열

    [Header("애니메이션 설정")]
    public float moveSpeed = 2f;
    public float fadeDuration = 1f;


    // 데미지 값을 설정하고 숫자 스프라이트를 생성하는 함수
    public void Setup(int damage)
    {
        string damageString = damage.ToString();

        Debug.Log("표시할 숫자: " + damageString); // 로그 1: 전체 숫자 확인

        foreach (char digitChar in damageString)
        {
            Debug.Log("현재 숫자: " + digitChar); // 로그 2: 반복문이 몇 번 도는지 확인

            // 숫자에 해당하는 스프라이트를 찾아 설정
            int digit = int.Parse(digitChar.ToString());
            Sprite numberSprite = numberSprites[digit];

            // Digit 프리팹을 생성하여 자식으로 추가
            GameObject newDigit = Instantiate(digitPrefab, transform);
            newDigit.GetComponent<SpriteRenderer>().sprite = numberSprite;
        }

        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        // 모든 자식 스프라이트 렌더러를 가져옴
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        Color startColor = renderers[0].color;

        float timer = 0;
        while (timer < fadeDuration)
        {
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;

            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);

            // 모든 자식 스프라이트를 서서히 투명하게
            foreach (SpriteRenderer r in renderers)
            {
                r.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            yield return null;
        }
        Destroy(gameObject);
    }
}