using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Object
{
    public class ObjectCreateHandler : MonoBehaviour
    {
        public ObjectPlacementHandler objectPlacementHandler;
        public GameObject installWarningPopup;// 설치 오류 팝업
        public Material previewMaterial;// 설치전 보일 material(붉은색)
        public GameObject infoPopup;// 오브젝트 정보 팝업

        private GameObject previewObject;// 팝업에서 선택한 Object
        private PrimitiveType currentType;// 현재 Object 타입
        private GameObject selectedObject = null;// 선택한 오브젝트(설치된 걸 선택)
        private GameObject lastHovered = null;// 이전 호버 오브젝트
        private SelectMode selectMode = SelectMode.DEFAULT;// 오브젝트 선택 모드 초기값
        
        private bool isPlacing = false;// 설치중 여부
        private bool isDragging = false;// 드래그 여부
        private bool isRotating = false;// 회전 여부
        
        public Vector3 originalPosition;// 드래그 시작 시점 위치를 저장할 변수
        
        private GameObject cubePrefab, spherePrefab, capsulePrefab, cylinderPrefab;// 프리팹 오브젝트
        private Dictionary<PrimitiveType, GameObject> prefabMap;

        private void Start()
        {// 시작시 프리팹 미리 load
            prefabMap = new Dictionary<PrimitiveType, GameObject>
            {
                { PrimitiveType.Cube, Resources.Load<GameObject>("Prefabs/Cube") },
                { PrimitiveType.Sphere, Resources.Load<GameObject>("Prefabs/Sphere") },
                { PrimitiveType.Capsule, Resources.Load<GameObject>("Prefabs/Capsule") },
                { PrimitiveType.Cylinder, Resources.Load<GameObject>("Prefabs/Cylinder") },
            };
        }

        private void Update()
        {
            if (isPlacing && previewObject)
            {//만약 설치중이고 선택한 오브젝트(previewObject)가 존재한다면
                FixObject();// 오브젝트 고정
            }
            else
            {
                objectPlacementHandler.DetectHoverAndSelect();// 호버 감지
                
                if (selectedObject && Mouse.current.rightButton.wasPressedThisFrame)
                {// 선택한 오브젝트가 존재하고, 마우스 우클릭을 수행하면
                    float angle = Keyboard.current.leftShiftKey.isPressed ? -45f : 45f;// shift를 누르고 우클릭하면 반대로
                    // StartCoroutine(objectPlacementHandler.RotateSelectedObject(Vector3.up, angle));// y축 기준 회전
                    StartCoroutine(objectPlacementHandler.RotateSelectedObject(Vector3.right, angle));// X축 기준 회전
                    // StartCoroutine(objectPlacementHandler.RotateSelectedObject(Vector3.forward, angle));// z축 기준 회전
                }

                if (isDragging && selectedObject)
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

        // 오브젝트 생성
        void SpawnObjects(PrimitiveType type)
        {
            if (previewObject)
            {// 만약 이미 선택한 Object가 있다면 제거
                Destroy(previewObject);
            }
            
            currentType = type;// 위치 확정 전 보여질 타입을 지정(마우스를 따라다니면서 보여질 타입 지정)

            if (prefabMap.TryGetValue(type, out GameObject prefab))
            {
                previewObject = Instantiate(prefab);//Instantiate는 유니티에서 게임 오브젝트를 복제(생성)할 때 사용하는 함수 >> 프리팹은 설계도이고 Instantiate는 실제 오브젝트를 찍어내는 도장
                previewObject.GetComponent<MeshRenderer>().material = previewMaterial;// 투명한 붉은색 머티리얼 생성 및 적용
                previewObject.GetComponent<Collider>().enabled = false;// 콜라이더를 통해 물리 적용
                isPlacing = true;//설치중으로 변경.
            }
            else
            {
                Debug.LogWarning($"{type} 프리팹을 찾을 수 없습니다.");
            }
        }

        // 오브젝트 설치
        void FixObject()
        {
            if (!isPlacing || !previewObject)
            {// 설치중이 아니거나 생성한 오브젝트가 존재하지 않으면
                return;
            }

            // 마우스 위치에서 Ray를 쏴서 평면과 충돌하는 지점 찾기
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit[] hits = Physics.RaycastAll(ray);
            
            if (hits.Length > 0)
            {// 만약 아무것도 부딪힌것이 있다면(공중이 아니라면)
                Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));// 거리에 따라 정렬
                Vector3 position = hits[0].point;// 첫 번째 유효한 위치에 previewObject 이동
                float objectHeight = previewObject.GetComponent<Renderer>().bounds.size.y;// 오브젝트의 높이를 고려하여 바닥면이 지면에 닿도록 보정
                position.y += objectHeight / 2f;// 살짝 띄우기
                previewObject.transform.position = position;// 위치를 계산한 만큼 설정(이동)
            }
            
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {// 마우스 왼쪽 클릭 시 위치(설치)확정
                // 기존 프리뷰 오브젝트를 설치용으로 전환
                GameObject placed = previewObject;// 설치전 오브젝트를 변수화
                previewObject = null;// 설치전 오브젝트 null처리
                isPlacing = false;// 설치중 false
                placed.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/Default");// 머티리얼 원래대로 복구
                placed.GetComponent<Collider>().enabled = true;// 콜라이더 활성화
                placed.name = currentType.ToString();// 이름 설정
                
                // Plane을 찾기 위한 적절한 hit 찾기
                Transform witchPlane = null;
                
                foreach (RaycastHit hit in hits)
                {
                    if (hit.transform == placed.transform)
                    {
                        continue; // 자기 자신은 건너뛰기
                    }
                    // PlaneRoot를 포함하는 부모 찾기
                    witchPlane = objectPlacementHandler.FindPlaneRoot(hit.transform);
                    if (witchPlane != null)
                    {
                        break;
                    }
                }
                
                if (witchPlane)
                {// 위치값이 존재한다면
                    placed.transform.SetParent(witchPlane);//해당 위치값을 부모로 설정
                }
                else
                {// 위치값이 존재하지 않는다면
                    Debug.LogWarning("Plane1 또는 Plane2를 찾을 수 없습니다.");
                }
                
                // 선택 가능하게 만들기
                SelectableObject selectable = placed.AddComponent<SelectableObject>();// 설치한 오브젝트를 선택 가능한 컴포넌트 추가
                selectable.objectType = currentType;// 오브젝트 타입 직접 지정
                selectable.onReselect = (type, position) =>
                {
                    selectedObject = placed;// 선택된 오브젝트 등록
                    RePlacing();// 오브젝트 이동
                };
                
            }
        }
    
        // 오브젝트 삭제
        public void DeleteSelectedObject()
        {
            if (selectedObject)
            {// 만약 선택한 오브젝트가 존재한다면
                Destroy(selectedObject);// 오브젝트 제거
                selectedObject = null;// 참조 초기화

                if (infoPopup)
                {// 만약 정보 팝업이 존재한다면
                    infoPopup.SetActive(false);// 정보 팝업 숨김
                }
            }
            else
            {// 선택한 오브젝트가 없다면
                Debug.LogWarning("삭제할 선택된 오브젝트가 없습니다.");
            }
        }
        
        // 기존 오브젝트를 선택 후 이동을 위한 준비
        void RePlacing()
        {
            
            if (selectedObject.GetComponent<SelectableObject>().selectMode == SelectMode.MOVE)
            {
                isDragging = true;// 드래그 상태 true
            }
            else
            {
                isDragging = false;// 드래그 상태 false
            }

            isPlacing = false;// 새로 생성하지 않음
            
            if (selectedObject)
            {// 선택한 오브젝트가 존재한다면
                originalPosition = selectedObject.transform.position;// 시작 위치 저장
                objectPlacementHandler.SetSelectedObject(selectedObject);// 선택한 오브젝트 set
                objectPlacementHandler.SetInstallWarningPopup(installWarningPopup);// 경고 팝업 set
                objectPlacementHandler.StartDragging();// 드래그 시작
            }
        }
        
    }
}