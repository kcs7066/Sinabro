using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LootAlertItem : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private float lifetime = 3f; // 알림이 떠 있는 시간
    [SerializeField] private float fadeDuration = 0.5f; // 사라지는 데 걸리는 시간

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // 아이템 정보를 설정하고 애니메이션 시작
    public void Setup(ItemData itemData)
    {
        itemIcon.sprite = itemData.itemIcon;
        itemNameText.text = $"{itemData.itemName}을(를) 획득했습니다.";
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        // 설정된 시간만큼 대기 (사라지기 전까지)
        yield return new WaitForSeconds(lifetime - fadeDuration);

        // 서서히 투명하게
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }

        // 애니메이션이 끝나면 오브젝트 파괴
        Destroy(gameObject);
    }
}