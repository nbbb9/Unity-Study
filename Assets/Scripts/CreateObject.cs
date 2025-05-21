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
    public Vector3 originalPosition; // 드래그 시작 시점 위치를 저장할 변수
    public GameObject installWarningPopup;// 설치 오류 팝업
    private SelectClickMode selectMode = SelectClickMode.NONE;// 오브젝트 선택 모드 초기값

    private void Update()
    {
        if (isPlacing && previewObject != null)
        {//만약 설치중이고 선택한 오브젝트(previewObject)가 존재한다면
            FixObject();// 오브젝트 고정
        }
        else
        {
            DetectHoverAndSelect();// 호버 감지

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
        {// 만약 이미 선택한 Object가 있다면 제거
            Destroy(previewObject);
        }

        currentType = type;// 위치 확정 전 보여질 타입을 지정(마우스를 따라다니면서 보여질 타입 지정)

        previewObject = GameObject.CreatePrimitive(type);// 타입에 맞는 오브젝트 생성
        previewObject.GetComponent<MeshRenderer>().material = previewMaterial;// 투명한 붉은색 머티리얼 생성 및 적용
        previewObject.GetComponent<Collider>().enabled = false;// 콜라이더를 통해 물리 적용

        isPlacing = true;//설치중으로 변경.
    }

    // 오브젝트 설치
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
        {//마우스 움직임에 따른 이동
            Vector3 position = hit.point;

            float objectHeight = previewObject.GetComponent<Renderer>().bounds.size.y;// 오브젝트의 높이를 고려하여 바닥면이 지면에 닿도록 보정
            position.y += objectHeight / 2f;

            previewObject.transform.position = position;// 위치를 계산한 만큼 설정(이동)
        }
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {// 마우스 왼쪽 클릭 시 확정
            // Debug.Log("오브젝트 설치!");
            GameObject placed = GameObject.CreatePrimitive(currentType);// 현재 타입을 적용한 오브젝트 생성 후 설치
            placed.transform.position = previewObject.transform.position + Vector3.up * 0.5f;// 살짝 띄워서 시작
            placed.transform.rotation = previewObject.transform.rotation;
            placed.name = currentType.ToString();// 현재 타입을 String으로 변환하여 이름으로 설정
            
            Transform witchPlane = FindPlaneRoot(hit.transform);// 마우스의 Ray Hit값을 인자로 넣는다.
            if (witchPlane != null)
            {// 위치값이 존재한다면
                placed.transform.SetParent(witchPlane);//해당 위치값을 부모로 설정
            }
            else
            {// 위치값이 존재하지 않는다면(공중이라면)
                Debug.LogWarning("Plane1 또는 Plane2를 찾을 수 없습니다.");
            }
            
            Rigidbody rb = placed.AddComponent<Rigidbody>();// 중력을 적용할 수 있도록 Rigidbody 추가
            rb.useGravity = true;// 중력 사용
            rb.constraints = RigidbodyConstraints.FreezeRotation;// 필요시 회전 제한

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
    
    // 현재 마우스의 위치가 Plane1 또는 Plane2인지 판단하는 메서드
    public Transform FindPlaneRoot(Transform hitTransform)
    {
        // Debug.Log("부딫힌 트랜스폼: " + hitTransform);
        Transform current = hitTransform;
        while (current != null)
        {
            if (current.name == "Plane1" || current.name == "Plane2")
            {//만약 인자로 받은 TransForm이 Plane1또는 Plane2라면 현재 위치 반환.
                return current;
            }
            current = current.parent;
        }
        return null;//
    }


    // 설치한 Object에 Hover기능
    void DetectHoverAndSelect()
    {
        // 마우스 위치에서 Ray를 쏴서 평면과 충돌하는 지점 찾기
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {// 만약 부딫힌것이 있다면
            GameObject hitObject = hit.collider.gameObject;// Ray에 부딫힌 오브젝트 변수화

            if (hitObject != lastHovered)
            {//만약 부딫힌 오브젝트가 이전 호버된 오브젝트와 다르다면
                // Debug.Log("1");
                if (lastHovered != null)
                {// 만약 이전 호버된 오브젝트가 존재한다면
                    SelectableObject lastSel = lastHovered.GetComponent<SelectableObject>();// 이전 호버된 오브젝트에 SelectableObject 컴포넌트 추가
                    if (lastSel != null)
                    {// 만약 이전 호버된 오브젝트가 존재한다면
                        lastSel.OnUnhover();// 호버 비활성화 처리
                    }
                }

                SelectableObject newSel = hitObject.GetComponent<SelectableObject>();// 새로 부딫힌 오브젝트 SelectableObject 컴포넌트 추가
                if (newSel != null)
                {// 새로 부딫힌 오브젝트가 존재한다면
                    newSel.OnHover();// 호버 활성화
                }

                lastHovered = hitObject;// 이전 호버 오브젝트 갱신
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {// 부딫힌것이 있는데 마우스 좌클릭하면
                // Debug.Log("2");
                SelectableObject selected = hitObject.GetComponent<SelectableObject>();// 선택한 오브젝트로 설정
                if (selected != null)
                {// 만약 선택한 오브젝트가 존재한다면
                    // Debug.Log("2.1");
                    selected.OnSelect();// 선택 상태로 설정
                }
            }
        }
        else
        {// 만약 부딫힌 것이 없다면
            if (lastHovered != null)
            {// 이전 호버된 오브젝트가 존재한다면
                // Debug.Log("4");
                SelectableObject lastSel = lastHovered.GetComponent<SelectableObject>();// 이전 호버된 오브젝트에 SelectableObject 컴포넌트 추가
                if (lastSel != null)
                {// 이전 호버된 오브젝트가 존재한다면
                    lastSel.OnUnhover();// 호버 비활성화
                }
                lastHovered = null;// 이전 호버된 오브젝트 제거
            }
        }
    }

    // 기존 오브젝트를 선택 후 이동을 위한 준비
    void RePlacing(PrimitiveType type, Vector3? startPosition = null)
    {
        // Debug.Log("이동 준비 상태");
        isPlacing = false;// 새로 생성하지 않음
        isDragging = true;// 드래그 상태 true

        if (selectedObject != null)
        {// 선택한 오브젝트가 존재한다면
            originalPosition = selectedObject.transform.position;// 시작 위치 저장
        }
        
    }
    
    // 선택된 오브젝트를 드래그로 이동
    void DragSelectedObject()
    {
        // Debug.Log("드래그 시작!");
        
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        //
        if (Physics.Raycast(ray, out hit))
        {// 만약 ray에 부딫힌것이 있다면
            Vector3 position = hit.point;// 부딫힌 부분 위치 변수
            
            float objectHeight = selectedObject.GetComponent<Renderer>().bounds.size.y;// 오브젝트 위치 설정
            position.y += objectHeight / 2f;// 오브젝트 높이 계산
            selectedObject.transform.position = position;// 선택한 오브젝트의 위치를 갱신
        }
        
        Ray downRay = new Ray(selectedObject.transform.position, Vector3.down);// 선택한 오브젝트를 기준으로 아래로 Ray생성.
        RaycastHit downHit;
        
        if (Physics.Raycast(downRay, out downHit))
        {// 만약 오브젝트 기준으로 부딫히는 부분이 있다면
            // 부모 평면 업데이트 로직
            Transform newParent = FindPlaneRoot(downHit.transform);
            // Debug.Log("새로운 부모??? : " + newParent.name);
            if (newParent != null && selectedObject.transform.parent != newParent)
            {
                // Debug.Log("부모 평면 변경됨: " + newParent.name);
                selectedObject.transform.SetParent(newParent);
            }
        }
    }

    // 드래그 끝남
    void EndDragging()
    {
        // Debug.Log("드래그 끝!");
        // 현재 위치가 유효한 Plane 위가 아닌 경우 되돌림
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {// 만약 ray에 부딫힌것이 있다면
            Transform plane = FindPlaneRoot(hit.transform);// 현재 부딫힌 평면 찾기
            // Debug.Log("드래그가 끝나면서 부딫힌 평면 : " + plane.name);
            if (plane == null)
            {// 만약 Plane이 null이라면
                installWarningPopup.SetActive(true);// 경고 문구 출력
                // Debug.Log("평면이 아니므로 위치 되돌림");
                if (selectedObject != null)
                {// 선택한 오브젝트가 존재한다면
                    selectedObject.transform.position = originalPosition;// 이전 위치로 되돌림
                }
            }
        }
        else
        {// 만약 ray에 부딫힌 것이 없다면
            installWarningPopup.SetActive(true);// 경고 문구 출력
            // Debug.Log("Ray에 아무것도 걸리지 않음. 위치 되돌림");
            if (selectedObject != null)
            {// 만약 선택한 오브젝트가 존재한다면
                selectedObject.transform.position = originalPosition;// 이전 위치로 되돌림
            }
        }
        
        isDragging = false;// 드래그 상태 종료
        selectedObject = null;// 선택한 오브젝트 제거
    }
    
    // 현재 선택된 오브젝트 삭제
    public void DeleteSelectedObject()
    {
        if (selectedObject != null)
        {// 만약 선택한 오브젝트가 존재한다면
            Destroy(selectedObject);// 오브젝트 제거
            selectedObject = null;// 참조 초기화

            if (infoPopup != null)
            {// 만약 정보 팝업이 존재한다면
                infoPopup.SetActive(false);// 정보 팝업 숨김
            }
        }
        else
        {// 선택한 오브젝트가 없다면
            Debug.LogWarning("삭제할 선택된 오브젝트가 없습니다.");
        }
    }
    
}