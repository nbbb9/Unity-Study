using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Object
{
    public class ObjectCreateHandler : MonoBehaviour
    {
        private InputActionAsset objectInputAsset;
        private InputAction moveBeforeFix;
        private InputAction clickAction;
        
        public ObjectPlacementHandler objectPlacementHandler;
        public GameObject installWarningPopup;// 설치 오류 팝업
        public Material previewMaterial;// 설치전 보일 material(붉은색)
        public GameObject infoPopup;// 오브젝트 정보 팝업

        private GameObject previewObject;// 팝업에서 선택한 Object
        private String currentType;// 현재 Object 타입
        private GameObject selectedObject;// 선택한 오브젝트(설치된 걸 선택)
        private GameObject lastHovered = null;// 이전 호버 오브젝트
        private SelectMode selectMode = SelectMode.DEFAULT;// 오브젝트 선택 모드 초기값
        
        private bool isPlacing;// 설치중 여부
        public bool IsPlacing => isPlacing;// 외부 접근 허용
        
        private Dictionary<String, GameObject> prefabMap;
        
        private void Awake()
        {// 시작시 프리팹 미리 load
            prefabMap = new Dictionary<String, GameObject>
            {
                { "Cube", Resources.Load<GameObject>("Prefabs/Cube") },
                { "Sphere", Resources.Load<GameObject>("Prefabs/Sphere") },
                { "Capsule", Resources.Load<GameObject>("Prefabs/Capsule") },
                { "Cylinder", Resources.Load<GameObject>("Prefabs/Cylinder") },
            };
            
            objectInputAsset = Resources.Load<InputActionAsset>("InputSystem_Actions");

            if (!objectInputAsset)
            {
                return;
            }
            
            moveBeforeFix = objectInputAsset.FindAction("MoveBeforeFix");
            clickAction = objectInputAsset.FindAction("Click");
        }
        
        private void Update()
        {
            // if (isPlacing && previewObject)
            // {//만약 설치중이고 선택한 오브젝트(previewObject)가 존재한다면
            //     FixObject();// 오브젝트 고정
            // }
            // else
            // {
            //     objectPlacementHandler.DetectHoverAndSelect();// 호버 감지
            //     
            //     if (selectedObject && Mouse.current.rightButton.wasPressedThisFrame)
            //     {// 선택한 오브젝트가 존재하고, 마우스 우클릭을 수행하면
            //         float angle = Keyboard.current.leftShiftKey.isPressed ? -45f : 45f;// shift를 누르고 우클릭하면 반대로
            //         // StartCoroutine(objectPlacementHandler.RotateSelectedObject(Vector3.up, angle));// y축 기준 회전
            //         StartCoroutine(objectPlacementHandler.RotateSelectedObject(Vector3.right, angle));// X축 기준 회전
            //         // StartCoroutine(objectPlacementHandler.RotateSelectedObject(Vector3.forward, angle));// z축 기준 회전
            //     }
            //
            //     if (isDragging && selectedObject)
            //     {// 드래그 중이고 선택한 오브젝트가 있다면
            //         objectPlacementHandler.Drag();// 드래그 시작
            //     }
            //
            //     if (isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
            //     {// 드래그 중이고 마우스 좌클릭이 풀어진다면
            //         objectPlacementHandler.EndDragging();// 드래그 종료
            //     }
            // }
        }

        // 오브젝트 생성
        public void SpawnObjects(String type)
        {
            if (previewObject) 
            {
                Destroy(previewObject);// 만약 이미 선택한 Object가 있다면 제거
            }
            
            if (prefabMap.TryGetValue(type, out GameObject prefab))
            {
                previewObject = Instantiate(prefab);//Instantiate는 유니티에서 게임 오브젝트를 복제(생성)할 때 사용하는 함수 >> 프리팹은 설계도이고 Instantiate는 실제 오브젝트를 찍어내는 도장
            //     previewObject.GetComponent<Collider>().enabled = false;// 콜라이더를 통해 물리 적용
                isPlacing = true;//설치중으로 변경.
                moveBeforeFix.performed += ReadMousePosition;// 움직임을 위해 마우스 위치값 읽기 바인딩 활성화
            }
            else Debug.LogWarning($"{type} 프리팹을 찾을 수 없습니다.");
        }
        
        private void ReadMousePosition(InputAction.CallbackContext context)
        {
            Vector2 mousePosition = context.ReadValue<Vector2>();
            Debug.Log("마우스 포지션 : "+ mousePosition);
            
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
        }

        // 오브젝트 설치
        void FixObject(RaycastHit[] hits)
        {
            if (!isPlacing || !previewObject)  
            {// 설치중이 아니거나 생성한 오브젝트가 존재하지 않으면
                return;
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
                    if (hit.transform == placed.transform) continue; // 자기 자신은 건너뛰기
                    // PlaneRoot를 포함하는 부모 찾기
                    witchPlane = objectPlacementHandler.FindPlaneRoot(hit.transform);
                    if (witchPlane != null) break;
                }
                
                if (witchPlane) placed.transform.SetParent(witchPlane);// 위치값이 존재한다면 해당 위치값을 부모로 설정
                else Debug.LogWarning("Plane1 또는 Plane2를 찾을 수 없습니다.");// 위치값이 존재하지 않는다면
                
                moveBeforeFix.performed -= ReadMousePosition;// 움직임을 위해 마우스 위치값 읽기 바인딩 활성화
                
                // 선택 가능하게 만들기
                SelectableObject selectable = placed.AddComponent<SelectableObject>();// 설치한 오브젝트를 선택 가능한 컴포넌트 추가
                
                selectable.objectType = currentType;// 오브젝트 타입 직접 지정
                selectable.onReselect = (type, position) =>
                {
                    Debug.Log(" 111 ");
                    selectedObject = placed;// 선택된 오브젝트 등록
                    Debug.Log(" 222 ");
                    objectPlacementHandler.RePlacing(selectedObject);// 오브젝트 이동
                };
                
            }
        }
    
        // 오브젝트 삭제
        public void DeleteSelectedObject()
        {
            if (!selectedObject)
            {
                Debug.LogWarning("삭제할 선택된 오브젝트가 없습니다.");
                return;
            }
            
            Destroy(selectedObject);// 오브젝트 제거
            selectedObject = null;// 참조 초기화
            if (infoPopup) 
            {
                infoPopup.SetActive(false);// 만약 정보 팝업이 존재한다면 정보 팝업 숨김
            }
        }
        
    }
}