using UnityEngine;

public class UIManager : MonoBehaviour
{
    // 이 UIManager의 단 하나뿐인 인스턴스를 저장할 변수
    public static UIManager instance;

    void Awake()
    {
        // 인스턴스가 아직 없다면 (최초의 UI 캔버스라면)
        if (instance == null)
        {
            // 자기 자신을 인스턴스로 등록
            instance = this;
            // 씬이 전환되어도 이 UI 캔버스(gameObject)를 파괴하지 않음
            DontDestroyOnLoad(gameObject);
        }
        // 인스턴스가 이미 존재한다면 (다른 씬에서 넘어와서 복제된 캔버스라면)
        else
        {
            // 이 복제품을 파괴하여 중복을 막음
            Destroy(gameObject);
        }
    }
}