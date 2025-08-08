using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용하기 위해 필요
public class UI_Slot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI quantityText;
    public int slotIndex; // 👈 몇 번째 슬롯인지 인덱스를 저장할 변수
}
