using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CreateObject : MonoBehaviour {
    private GameObject previewObject;// 팝업에서 선택한 Object
    public Material previewMaterial;// 설치전 보일 material(붉은색)
    private bool isPlacing = false;// 설치중인지 여부
    private GameObject selectedObject = null;// 
    private bool isDragging = false;// 
    private PrimitiveType currentType;// 현재 Object 타입
    private GameObject lastHovered = null;
    public GameObject infoPopup;
    public Text nameText;
    public Text typeText;

    void Update() {
        if (isPlacing && previewObject != null) {//만약 설치중이고 previewObject가 존재한다면
            FixObject();
        }
        else{
            DetectHoverAndSelect();

            if (isDragging && selectedObject != null) {
                DragSelectedObject();
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging) {
                EndDragging();
            }
        }
    }

    /// <summary>
    /// 정육면체 생성
    /// </summary>
    public void SpawnCube() {
        SpawnObjects(PrimitiveType.Cube);
    }
    
    /// <summary>
    /// 구 생성
    /// </summary>
    public void SpawnSphere() {
        SpawnObjects(PrimitiveType.Sphere);
    }

    /// <summary>
    /// 타입에 맞는 오브젝트 생성 메서드
    /// </summary>
    /// <param name="type"></param>
    void SpawnObjects(PrimitiveType type) {
        if (previewObject != null) {
            Destroy(previewObject);// 만약 이미 선택한 Object가 있다면 제거
        }

        currentType = type;// 위치 확정 전 보일 타입을 지정(마우스를 따라다니면서 보일 타입 지정)

        previewObject = GameObject.CreatePrimitive(type);// 타입에 맞는 오브젝트 생성
        previewObject.GetComponent<MeshRenderer>().material = previewMaterial;// 투명한 붉은색 머티리얼 생성 및 적용
        previewObject.GetComponent<Collider>().enabled = false;// 콜라이더를 통해 물리 법칙? 적용

        isPlacing = true;//설치중으로 변경.
    }

    /// <summary>
    /// 마우스 좌클릭을 하면 해당 위치에 고정
    /// </summary>
    void FixObject() {
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
            placed.transform.position = previewObject.transform.position + Vector3.up * 0.5f; // 살짝 띄워서 시작
            placed.transform.rotation = previewObject.transform.rotation;
            placed.name = currentType.ToString();
            // 충돌한 평면을 부모로 설정
            placed.transform.SetParent(hit.transform);
            // 중력을 적용할 수 있도록 Rigidbody 추가
            Rigidbody rb = placed.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation; // 필요시 회전 제한

            var selectable = placed.AddComponent<SelectableObject>();// 선택 가능한 오브젝트로 생성.
            selectable.onReselect = (type, pos) => {
                selectedObject = placed;// 선택된 오브젝트 등록
                RePlacing(type, pos);// 오브젝트 이동
            };
            selectable.infoPopup = infoPopup;
            selectable.nameText = nameText;
            selectable.typeText = typeText;

            // 프리뷰 제거
            Destroy(previewObject);
            previewObject = null;
            isPlacing = false;
        }
    }

    /// <summary>
    /// 설치한 Object에 Hover기능
    /// </summary>
    void DetectHoverAndSelect() {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject != lastHovered) {
                if (lastHovered != null) {
                    var lastSel = lastHovered.GetComponent<SelectableObject>();
                    if (lastSel != null)
                        lastSel.OnUnhover();
                }

                var newSel = hitObject.GetComponent<SelectableObject>();
                if (newSel != null)
                    newSel.OnHover();

                lastHovered = hitObject;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame) {
                var selected = hitObject.GetComponent<SelectableObject>();
                if (selected != null) {
                    selected.OnSelect();
                }
            }
        }
        else {
            // Ray에 아무것도 안 걸리면 hover 해제
            if (lastHovered != null) {
                var lastSel = lastHovered.GetComponent<SelectableObject>();
                if (lastSel != null)
                    lastSel.OnUnhover();
                lastHovered = null;
            }
        }
    }

    /// <summary>
    /// 기존 오브젝트를 선택 후 이동을 위한 준비
    /// </summary>
    void RePlacing(PrimitiveType type, Vector3? startPosition = null)
    {
        isPlacing = false; // 새로 생성하지 않음
        isDragging = true;
        
        // UI 정보 갱신
        if (infoPopup != null && nameText != null && typeText != null && selectedObject != null) {
            infoPopup.SetActive(true);
            nameText.text = $"이름: {selectedObject.name}";
            typeText.text = $"타입: {type}";
        }
    }
    
    /// <summary>
    /// 선택된 오브젝트를 마우스 위치로 이동
    /// </summary>
    void DragSelectedObject() {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            Vector3 position = hit.point;
            float objectHeight = selectedObject.GetComponent<Renderer>().bounds.size.y;
            position.y += objectHeight / 2f;
            selectedObject.transform.position = position;
        }
    }

    /// <summary>
    /// 드래그 끝낼 때 호출
    /// </summary>
    void EndDragging() {
        isDragging = false;
        selectedObject = null;
    }

}