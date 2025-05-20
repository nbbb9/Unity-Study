using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CreateObject : MonoBehaviour 
{
    private GameObject previewObject;// 팝업에서 선택한 Object
    public Material previewMaterial;// 설치전 보일 material(붉은색)
    private bool isPlacing = false;// 설치중 여부
    private GameObject selectedObject = null;// 선택한 오브젝트(설치된 걸 선택)
    private bool isDragging = false;// 드래그 여부
    private PrimitiveType currentType;// 현재 Object 타입
    private GameObject lastHovered = null;// 이전 호버 오브젝트
    public GameObject infoPopup;// 오브젝트 정보 팝업
    public Text nameText;// 선택한 오브젝트 이름 
    public Text typeText;// 선택한 오브젝트 타입
    private SelectClickMode selectMode = SelectClickMode.JUSTSELECT;// 오브젝트 선택 모드 초기값

    void Update()
    {
        if (isPlacing && previewObject != null)
        {//만약 설치중이고 선택한 오브젝트(previewObject)가 존재한다면
            FixObject();// 오브젝트 고정
        }
        else
        {
            DetectHoverAndSelect();// 호버

            if (isDragging && selectedObject != null)
            {// 드래그 중이고 선택한 오브젝트가 있다면
                DragSelectedObject();// 드래그 시작
            }

            if (isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
            {// 드래그 중이고 마우스 좌클릭이 풀어진다면
                EndDragging();// 드래그 종료
            }
        }
    }

    // 정육면체 생성
    public void SpawnCube()
    {
        SpawnObjects(PrimitiveType.Cube);
    }

    // 구 생성
    public void SpawnSphere()
    {
        SpawnObjects(PrimitiveType.Sphere);
    }
    
    // 캡슐 생성
    public void SpawnCapsule()
    {
        SpawnObjects(PrimitiveType.Capsule);
    }

    // 원기둥 생성
    public void SpawnCylinder()
    {
        SpawnObjects(PrimitiveType.Cylinder);
    }


    // 타입에 맞는 오브젝트 생성 메서드
    void SpawnObjects(PrimitiveType type)
    {
        if (previewObject != null)
        {
            Destroy(previewObject);// 만약 이미 선택한 Object가 있다면 제거
        }

        currentType = type;// 위치 확정 전 보여질 타입을 지정(마우스를 따라다니면서 보여질 타입 지정)

        Debug.Log(previewMaterial);
        previewObject = GameObject.CreatePrimitive(type);// 타입에 맞는 오브젝트 생성
        previewObject.GetComponent<MeshRenderer>().material = previewMaterial;// 투명한 붉은색 머티리얼 생성 및 적용
        previewObject.GetComponent<Collider>().enabled = false;// 콜라이더를 통해 물리 법칙? 적용

        isPlacing = true;//설치중으로 변경.
    }

    // 마우스 좌클릭을 하면 해당 위치에 고정
    void FixObject()
    {
        if (!isPlacing || previewObject == null)
        {// 설치중이 아니거나 생성한 오브젝트가 존재하지 않으면
            return;
        }

        // 마우스 위치에서 Ray를 쏴서 평면과 충돌하는 지점 찾기
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 position = hit.point;

            float objectHeight = previewObject.GetComponent<Renderer>().bounds.size.y;// 오브젝트의 높이를 고려하여 바닥면이 지면에 닿도록 보정
            position.y += objectHeight / 2f;// 

            previewObject.transform.position = position;
        }

        // 마우스 왼쪽 클릭 시 확정
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            GameObject placed = GameObject.CreatePrimitive(currentType);// 현재 타입을 적용한 오브젝트 생성 후 설치
            placed.transform.position = previewObject.transform.position + Vector3.up * 0.5f;// 살짝 띄워서 시작
            placed.transform.rotation = previewObject.transform.rotation;// 
            placed.name = currentType.ToString();// 현재 타입을 String으로 변환하여 이름으로 설정
            
            //Todo  매번 찾는것 보다 한번만 찾도록
            Transform witchPlane = FindPlaneRoot(hit.transform);
            if (witchPlane != null)
            {
                placed.transform.SetParent(witchPlane);
            }
            else
            {
                Debug.LogWarning("Plane1 또는 Plane2를 찾을 수 없습니다.");
            }
            
            Rigidbody rb = placed.AddComponent<Rigidbody>();// 중력을 적용할 수 있도록 Rigidbody 추가
            rb.useGravity = true;// 중력 사용
            rb.constraints = RigidbodyConstraints.FreezeRotation; // 필요시 회전 제한

            SelectableObject selectable = placed.AddComponent<SelectableObject>();// 설치한 오브젝트를 선택 가능한 컴포넌트 추가
            selectable.onReselect = (type, pos) =>
            {
                selectedObject = placed;// 선택된 오브젝트 등록
                RePlacing(type, pos);// 오브젝트 이동
            };

            Destroy(previewObject);// 프리뷰 제거
            previewObject = null;// 설치 후 초기화
            isPlacing = false;// 설치 여부 false
        }
    }
    
    Transform FindPlaneRoot(Transform hitTransform)
    {
        Transform current = hitTransform;
        while (current != null)
        {
            if (current.name == "Plane1" || current.name == "Plane2")
            {
                return current;
            }
            current = current.parent;
        }
        return null;
    }


    // 설치한 Object에 Hover기능
    void DetectHoverAndSelect()
    {
        // 마우스 위치에서 Ray를 쏴서 평면과 충돌하는 지점 찾기
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject != lastHovered)
            {
                if (lastHovered != null)
                {
                    SelectableObject lastSel = lastHovered.GetComponent<SelectableObject>();
                    if (lastSel != null)
                        lastSel.OnUnhover();
                }

                SelectableObject newSel = hitObject.GetComponent<SelectableObject>();
                if (newSel != null)
                {
                    newSel.OnHover();
                }

                lastHovered = hitObject;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                SelectableObject selected = hitObject.GetComponent<SelectableObject>();
                if (selected != null)
                {
                    selected.OnSelect();
                }
            }
        }
        else
        {
            if (lastHovered != null)
            {// Ray에 아무것도 안 걸리면 hover 해제
                SelectableObject lastSel = lastHovered.GetComponent<SelectableObject>();
                if (lastSel != null)
                {
                    lastSel.OnUnhover();
                }
                lastHovered = null;
            }
        }
    }

    // 기존 오브젝트를 선택 후 이동을 위한 준비
    void RePlacing(PrimitiveType type, Vector3? startPosition = null)
    {
        isPlacing = false;// 새로 생성하지 않음
        isDragging = true;// 드래그 상태 true

        if (infoPopup != null && nameText != null && typeText != null && selectedObject != null)
        {// UI 정보 갱신
            infoPopup.SetActive(true);
            nameText.text = $"이름: {selectedObject.name}";
            typeText.text = $"타입: {type}";
        }
    }
    
    // 선택된 오브젝트를 마우스 위치로 이동
    void DragSelectedObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 position = hit.point;
            float objectHeight = selectedObject.GetComponent<Renderer>().bounds.size.y;
            position.y += objectHeight / 2f;
            selectedObject.transform.position = position;
            
            // ✅ 부모 평면 업데이트 로직
            Transform newParent = FindPlaneRoot(hit.transform);
            if (newParent != null && selectedObject.transform.parent != newParent)
            {
                selectedObject.transform.SetParent(newParent);
            }
        }
    }

    // 드래그 끝남
    void EndDragging()
    {
        isDragging = false;// 드래그 상태 종료
        selectedObject = null;// 
    }
    // 현재 선택된 오브젝트 삭제
    public void DeleteSelectedObject()
    {
        if (selectedObject != null)
        {
            Destroy(selectedObject); // 오브젝트 제거
            selectedObject = null;   // 참조 초기화

            if (infoPopup != null)
            {
                infoPopup.SetActive(false); // 정보 팝업 숨김
            }
        }
        else
        {
            Debug.LogWarning("삭제할 선택된 오브젝트가 없습니다.");
        }
    }

    
}