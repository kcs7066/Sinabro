using UnityEngine;

// 이 스크립트는 메인 카메라가 '로컬 플레이어'를 찾아 따라다니게 만듭니다.
public class CameraController : MonoBehaviour
{
    // 이제 이 변수는 public일 필요가 없습니다. 스크립트가 직접 찾아낼 겁니다.
    private Transform playerTransform;

    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    // LateUpdate는 모든 Update 함수가 호출된 후에 실행됩니다.
    // 플레이어가 움직인 '후'에 카메라가 따라가야 떨림(Jitter) 현상이 없기 때문에 카메라 이동은 LateUpdate에서 처리하는 것이 좋습니다.
    void LateUpdate()
    {
        // 만약 아직 플레이어를 찾지 못했다면,
        if (playerTransform == null)
        {
            // PlayerController가 등록해 둔 로컬 플레이어 인스턴스가 있는지 확인합니다.
            if (PlayerController.LocalInstance != null)
            {
                // 찾았다면, 그 플레이어의 Transform을 우리의 목표로 설정합니다.
                playerTransform = PlayerController.LocalInstance.transform;
            }
            else
            {
                // 아직 로컬 플레이어가 생성되지 않았다면, 아무것도 하지 않고 기다립니다.
                return;
            }
        }

        // --- 이 아래는 기존의 카메라 이동 로직과 동일합니다 ---
        Vector3 desiredPosition = playerTransform.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
