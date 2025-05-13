using UnityEngine;
using UnityEngine.InputSystem;

public class CreateObject : MonoBehaviour {
    public GameObject selectObject;
    public Material previewMaterial;// 설치 전 Material
    private GameObject previewObject;// 팝업에서 선택한 Object
    private bool isPlacing = false;// 설치 여부
    private PrimitiveType currentType;// 현재 Object 타입

    public bool IsPlacing() {
        return isPlacing;
    }

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

        // if (selectObject == null) {
        //     selectObject = GameObject.Find("SelectObject");
        // }

        currentType = type;
        previewObject = GameObject.CreatePrimitive(type);
        previewObject.GetComponent<Collider>().enabled = false;

        // 투명한 붉은색 머티리얼 생성 및 적용
        Renderer rend = previewObject.GetComponent<Renderer>();
        if (rend != null) {
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader != null) {
                Material redTransparentMat = new Material(urpShader);

                // 1. 색상 (불투명도 0.5)
                redTransparentMat.color = new Color(1f, 0f, 0f, 0.5f); // 붉은색 + 투명도

                // 2. URP용 투명 설정
                redTransparentMat.SetFloat("_Surface", 1); // 0: Opaque, 1: Transparent
                redTransparentMat.SetFloat("_Blend", 0);   // Alpha blending
                redTransparentMat.SetFloat("_ZWrite", 0);
                redTransparentMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                redTransparentMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                // 3. 알파 프리멀티플 키워드 제거 (옵션)
                redTransparentMat.DisableKeyword("_ALPHATEST_ON");
                redTransparentMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                redTransparentMat.EnableKeyword("_ALPHABLEND_ON");

                rend.material = redTransparentMat;
            } else {
                Debug.LogError("URP 셰이더를 찾을 수 없습니다.");
            }
        }

        isPlacing = true;
    }

    public void RestartPlacingFromObject(GameObject existingObject, PrimitiveType type) {
        if (previewObject != null) {
            Destroy(previewObject);
        }

        currentType = type;

        previewObject = existingObject;
        isPlacing = true;

        // 충돌 방지를 위해 Collider 끔
        Collider col = previewObject.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 노란색 머티리얼 적용
        Renderer rend = previewObject.GetComponent<Renderer>();
        if (rend != null) {
            Material yellowMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            yellowMat.color = new Color(1f, 1f, 0f, 0.4f); // 투명한 노란색
            yellowMat.SetFloat("_Surface", 1); // Transparent
            yellowMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            yellowMat.renderQueue = 3000;
            rend.material = yellowMat;
        }
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

            placed.name = currentType.ToString();

            Destroy(previewObject);
            previewObject = null;
            isPlacing = false;
        }

    }

}