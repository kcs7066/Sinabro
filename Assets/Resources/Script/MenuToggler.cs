using UnityEngine;

public class MenuToggler : MonoBehaviour
{
    // 유니티 에디터에서 하위 메뉴 그룹을 연결할 변수
    public GameObject subMenuGroup;

    // 버튼을 클릭했을 때 호출할 함수
    public void ToggleSubMenu()
    {
        // subMenuGroup이 null이 아닌지 확인 (안전장치)
        if (subMenuGroup != null)
        {
            // 현재 활성화 상태의 반대 상태로 변경
            // (켜져 있으면 끄고, 꺼져 있으면 켠다)
            subMenuGroup.SetActive(!subMenuGroup.activeSelf);
        }
    }
}