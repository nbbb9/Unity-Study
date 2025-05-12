using UnityEngine;
using UnityEngine.InputSystem;

public class CreateObject : MonoBehaviour {
    public GameObject selectObject;
    public Material previewMaterial;// 설치 전 Material
    private GameObject previewObject;// 팝업에서 선택한 Object
    private bool isPlacing = false;// 설치 여부
    private PrimitiveType currentType;// 

    /// <summary>
    /// 정육면체 생성
    /// </summary>
    public void SpawnCube() {
        StartPlacing(PrimitiveType.Cube);
    }
    /// <summary>
    /// 구 생성
    /// </summary>
    public void SpawnSphere() {
        StartPlacing(PrimitiveType.Sphere);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    void StartPlacing(PrimitiveType type) {
        if (previewObject != null) {
            Destroy(previewObject);//만약 이미 선택한 Object가 있다면 제거
        }

        if (selectObject == null) {
            selectObject = GameObject.Find("SelectObject");
        }

        currentType = type;
        previewObject = GameObject.CreatePrimitive(type);
        previewObject.GetComponent<Collider>().enabled = false;

        // 머티리얼을 투명하게 설정
        if (previewMaterial != null) {
            Renderer rend = previewObject.GetComponent<Renderer>();
            if (rend != null) {
                Material matCopy = new Material(previewMaterial);// 복사본 생성

                rend.material = matCopy;
                
                Debug.Log("Material: " + previewObject.GetComponent<Renderer>().material.name + matCopy);
                Debug.Log("Shader: " + previewObject.GetComponent<Renderer>().material.shader.name);
            }
            
        }

        isPlacing = true;
    }

    void Update() {
        if (!isPlacing || previewObject == null) return;

        // 마우스 위치에서 Ray를 쏴서 평면과 충돌하는 지점 찾기
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;


        if (Physics.Raycast(ray, out hit)) {
            Vector3 position = hit.point;

            // 오브젝트의 높이를 고려하여 바닥면이 지면에 닿도록 보정
            float objectHeight = previewObject.GetComponent<Renderer>().bounds.size.y;
            position.y += objectHeight / 2f;

            previewObject.transform.position = position;
        }

        // 마우스 왼쪽 클릭 시 확정
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            GameObject placed = GameObject.CreatePrimitive(currentType);
            placed.transform.position = previewObject.transform.position;
            placed.transform.rotation = previewObject.transform.rotation;

            Destroy(previewObject);
            previewObject = null;
            isPlacing = false;
        }

    }
}