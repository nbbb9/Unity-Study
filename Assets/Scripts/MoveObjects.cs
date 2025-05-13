using UnityEngine;
using UnityEngine.InputSystem;

public class MoveObjects : MonoBehaviour {
    public CreateObject createObject; // CreateObject 스크립트를 참조

    void Update() {
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) {
                GameObject target = hit.collider.gameObject;

                Debug.Log("선택된 오브젝트: " + target.name);

                PrimitiveType? type = GetPrimitiveTypeFromName(target.name);
                if (type != null) {
                    Debug.Log("PrimitiveType 감지됨: " + type.ToString());

                    // CreateObject 스크립트의 메서드 호출
                    createObject.RestartPlacingFromObject(target, (PrimitiveType)type);
                } else {
                    Debug.LogWarning("PrimitiveType 감지 실패: " + target.name);
                }
            }
        }
    }

    /// <summary>
    /// 이름에서 PrimitiveType 유추 (단순 예제용)
    /// </summary>
    PrimitiveType? GetPrimitiveTypeFromName(string name) {
        if (name.Contains("cube")) return PrimitiveType.Cube;
        if (name.Contains("sphere")) return PrimitiveType.Sphere;
        // 필요 시 Capsule, Cylinder 등 추가
        return null;
    }
}