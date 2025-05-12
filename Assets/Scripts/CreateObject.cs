using UnityEngine;
using UnityEngine.InputSystem;

public class CreateObject : MonoBehaviour {
    public Material previewMaterial; // 투명하게 보이게 할 머티리얼
    private GameObject previewObject;
    private bool isPlacing = false;

    public void SpawnCube() {
        StartPlacing(PrimitiveType.Cube);
    }

    public void SpawnSphere() {
        StartPlacing(PrimitiveType.Sphere);
    }

    void StartPlacing(PrimitiveType type) {
        if (previewObject != null) Destroy(previewObject);

        previewObject = GameObject.CreatePrimitive(type);
        previewObject.GetComponent<Collider>().enabled = false;

        // 머티리얼을 투명하게 설정
        if (previewMaterial != null) {
            Renderer rend = previewObject.GetComponent<Renderer>();
            rend.material = previewMaterial;
        }

        isPlacing = true;
    }

    void Update() {
        if (!isPlacing || previewObject == null) return;

        // 마우스 위치에서 Ray를 쏴서 평면과 충돌하는 지점 찾기
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            previewObject.transform.position = hit.point;
        }

        // 마우스 왼쪽 클릭 시 확정
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            GameObject placed = Instantiate(previewObject, previewObject.transform.position, Quaternion.identity);
            placed.GetComponent<Renderer>().material = null; // 기본 머티리얼 사용
            placed.AddComponent<BoxCollider>(); // 충돌 추가

            Destroy(previewObject);
            previewObject = null;
            isPlacing = false;
        }
    }
}
