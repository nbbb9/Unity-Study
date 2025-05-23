using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Object
{
    public class ObjectCreateHandler : MonoBehaviour
    {
        public ObjectPlacementHandler objectPlacementHandler;
    
        private GameObject previewObject;// 팝업에서 선택한 Object
        public Material previewMaterial;// 설치전 보일 material(붉은색)
    
        private GameObject selectedObject = null;// 선택한 오브젝트(설치된 걸 선택)
        private bool isPlacing = false;// 설치중 여부
        private bool isDragging = false;// 드래그 여부
        private PrimitiveType currentType;// 현재 Object 타입
        private GameObject lastHovered = null;// 이전 호버 오브젝트
        public GameObject infoPopup;// 오브젝트 정보 팝업
        public Vector3 originalPosition; // 드래그 시작 시점 위치를 저장할 변수
        public GameObject installWarningPopup;// 설치 오류 팝업
        private SelectMode selectMode = SelectMode.DEFAULT;// 오브젝트 선택 모드 초기값
        
        private Dictionary<PrimitiveType, GameObject> prefabCache = new();

        private void Awake()
        {
            if (objectPlacementHandler == null)
            {
                objectPlacementHandler = GetComponent<ObjectPlacementHandler>();
            }
        }

        private void Update()
        {
            if (isPlacing && previewObject != null)
            {//만약 설치중이고 선택한 오브젝트(previewObject)가 존재한다면
                FixObject();// 오브젝트 고정
            }
            else
            {
                objectPlacementHandler.DetectHoverAndSelect();// 호버 감지

                if (isDragging && selectedObject != null)
                {// 드래그 중이고 선택한 오브젝트가 있다면
                    objectPlacementHandler.Drag();// 드래그 시작
                }

                if (isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
                {// 드래그 중이고 마우스 좌클릭이 풀어진다면
                    objectPlacementHandler.EndDragging();// 드래그 종료
                }
            }
        }
        
        public void SpawnCube() => SpawnObjects(PrimitiveType.Cube);// 정육면체 생성
        public void SpawnSphere() => SpawnObjects(PrimitiveType.Sphere);// 구 생성
        public void SpawnCapsule() => SpawnObjects(PrimitiveType.Capsule);// 캡슐 생성
        public void SpawnCylinder() => SpawnObjects(PrimitiveType.Cylinder);// 원기둥 생성

        // 타입에 맞는 오브젝트 생성 메서드
        void SpawnObjects(PrimitiveType type)
        {
            if (previewObject != null)
            {// 만약 이미 선택한 Object가 있다면 제거
                Destroy(previewObject);
            }

            currentType = type;// 위치 확정 전 보여질 타입을 지정(마우스를 따라다니면서 보여질 타입 지정)
            
            string prefabName = type.ToString(); // Cube, Sphere 등 프리팹 이름은 PrimitiveType 이름과 동일하다고 가정
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");
    
            if (prefab != null)
            {
                previewObject = Instantiate(prefab); // 프리팹을 인스턴스화
            }
            else
            {
                Debug.LogError($"Prefabs/{prefabName} 프리팹을 찾을 수 없습니다.");
                return;
            }

            // previewObject = GameObject.CreatePrimitive(type);// 타입에 맞는 오브젝트 생성
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
                // GameObject placed = GameObject.CreatePrimitive(currentType);// 현재 타입을 적용한 오브젝트 생성 후 설치
                GameObject placed = Instantiate(Resources.Load<GameObject>($"Prefabs/{currentType.ToString()}"));
                placed.transform.position = previewObject.transform.position + Vector3.up * 0.5f;// 살짝 띄워서 시작
                placed.transform.rotation = previewObject.transform.rotation;
                placed.name = currentType.ToString();// 현재 타입을 String으로 변환하여 이름으로 설정
            
                Transform witchPlane = objectPlacementHandler.FindPlaneRoot(hit.transform);// 마우스의 Ray Hit값을 인자로 넣는다.
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
                selectable.objectType = currentType; // 오브젝트 타입 직접 지정
                selectable.onReselect = (type, pos) =>
                {
                    selectedObject = placed;// 선택된 오브젝트 등록
                    RePlacing();// 오브젝트 이동
                };

                Destroy(previewObject);// 프리뷰 제거
                previewObject = null;// 설치 후 초기화
                isPlacing = false;// 설치 여부 false
            }
        }
    
    
        // 기존 오브젝트를 선택 후 이동을 위한 준비
        void RePlacing()
        {
            // Debug.Log("이동 준비 상태");
            isPlacing = false;// 새로 생성하지 않음
            isDragging = true;// 드래그 상태 true

            if (selectedObject != null)
            {// 선택한 오브젝트가 존재한다면
                originalPosition = selectedObject.transform.position;// 시작 위치 저장
                objectPlacementHandler.setSelectedObject(selectedObject);// 선택한 오브젝트 set
                objectPlacementHandler.setInstallWarningPopup(installWarningPopup);// 경고 팝업 set
                objectPlacementHandler.StartDragging();// 드래그 시작
            }
        
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
}