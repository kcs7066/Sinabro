using UnityEngine;

// 이 스크립트는 메인 카메라가 플레이어를 따라다니게 만듭니다.
public class CameraController : MonoBehaviour
{
    // 유니티 에디터에서 따라다닐 대상을 지정해 줄 변수입니다.
    public Transform playerTransform;

    // 카메라의 부드러운 이동을 위한 변수입니다. 값이 작을수록 부드럽게 따라갑니다.
    public float smoothSpeed = 0.125f;

    // 카메라와 플레이어 사이의 Z축 거리를 유지하기 위한 변수입니다.
    public Vector3 offset;

    // LateUpdate는 모든 Update 함수가 호출된 후에 실행됩니다.
    // 플레이어가 움직인 '후'에 카메라가 따라가야 떨림(Jitter) 현상이 없기 때문에 카메라 이동은 LateUpdate에서 처리하는 것이 좋습니다.
    void LateUpdate()
    {
        // 만약 따라다닐 대상(플레이어)이 지정되지 않았다면, 아무것도 하지 않습니다.
        if (playerTransform == null)
        {
            return;
        }

        // 목표 위치는 플레이어의 위치에 오프셋(거리)을 더한 값입니다.
        Vector3 desiredPosition = playerTransform.position + offset;

        // Vector3.Lerp 함수를 사용해 현재 카메라 위치에서 목표 위치까지 부드럽게 이동하는 중간값을 계산합니다.
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 계산된 위치로 카메라를 이동시킵니다.
        transform.position = smoothedPosition;
    }
}
