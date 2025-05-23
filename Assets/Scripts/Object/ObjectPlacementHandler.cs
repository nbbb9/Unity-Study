using System.Net.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;
using SelectMode = Enums.SelectMode;

namespace Object
{
    public class ObjectPlacementHandler : MonoBehaviour
    {
        private GameObject selectedObject;// 선택한 오브젝트
        private SelectableObject lastSelectedObject;// 마지막으로 선택한 오브젝트
        private Vector3 originalPosition;// 드래그 시작 시점 위치를 저장할 변수
        private GameObject installWarningPopup;// 설치 오류 팝업
        private GameObject lastHoveredObject;// 이전 호버 오브젝트

        public void SetSelectedObject(GameObject obj)
        {
            selectedObject = obj;
            originalPosition = obj ? obj.transform.position : Vector3.zero;
        }
    
        public void SetInstallWarningPopup(GameObject popup)
        {
            installWarningPopup = popup;
        }

        public void StartDragging()
        {
            if (selectedObject)
            {// 선택한 오브젝트가 존재한다면
                originalPosition = selectedObject.transform.position;
            }
        }

    
        // 드래그 시작
        public void Drag()
        {
            if (!selectedObject)
            {
                return;
            }
        
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit[] hits = Physics.RaycastAll(ray);

            if (hits.Length == 0)
            {
                return;
            }
            
            Vector3? newPosition = null;
            Transform newParent = null;

            foreach (RaycastHit hit in hits)
            {// 
                if (hit.transform != selectedObject.transform && newPosition == null)
                {
                    Vector3 position = hit.point;
                    float objectHeight = selectedObject.GetComponent<Renderer>().bounds.size.y;
                    position.y += objectHeight / 2f;
                    newPosition = position;
                }
                if (hit.transform != selectedObject.transform)
                {
                    // 부모 평면은 드래그할 오브젝트를 제외한 첫 번째 plane 후보
                    Transform candidateParent = FindPlaneRoot(hit.transform);
                    if (candidateParent)
                    {
                        newParent = candidateParent;
                        break; // plane 후보는 하나만 잡고 종료
                    }
                }
            }
            
            if (newPosition.HasValue)
            {
                selectedObject.transform.position = newPosition.Value;
            }

            if (newParent && selectedObject.transform.parent != newParent)
            {
                selectedObject.transform.SetParent(newParent);
            }
            
        }

        // 드래그 종료
        public void EndDragging()
        {
            if (!selectedObject)
            {
                return;
            }
            
            // 드래그 끝나면 infoPopup 비활성화
            SelectableObject selectable = selectedObject.GetComponent<SelectableObject>();// 선택한 오브젝트의 컴포넌트 세팅
            if (selectable && selectable.infoPopup)
            {// 선택한 오브젝트가 존재하고 정보 팝업이 존재한다면
                selectable.infoPopup.SetActive(false);// 정보 팝업 비활성화
                selectable.selectMode = SelectMode.DEFAULT;// Default 모드로 변경
            }
        
            // Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            Ray downRay = new Ray(selectedObject.transform.position, Vector3.down);// 선택한 오브젝트를 기준으로 아래로 Ray생성.

            if (Physics.Raycast(downRay, out RaycastHit hit))
            {// 만약 ray에 부딪힌것이 있다면
                Transform plane = FindPlaneRoot(hit.transform);// 현재 부딪힌 평면 찾기
                if (!plane)
                {// 만약 Plane이 null이라면
                    installWarningPopup.SetActive(true);// 경고 문구 출력
                    selectedObject.transform.position = originalPosition;// 이전 위치로 되돌림
                }
                selectedObject.transform.SetParent(plane);// 부모 설정
            }
            else
            {// 만약 ray에 부딪힌 것이 없다면
                installWarningPopup.SetActive(true);// 경고 문구 출력
                selectedObject.transform.position = originalPosition;// 이전 위치로 되돌림
            }
        
            selectedObject = null;// 선택한 오브젝트 제거
        }
    
        // 설치한 Object에 Hover기능
        public void DetectHoverAndSelect()
        {
            // 마우스 위치에서 Ray를 쏴서 평면과 충돌하는 지점 찾기
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {// 만약 부딪힌것이 있다면
                GameObject hitObject = hit.collider.gameObject;// Ray에 부딪힌 오브젝트 변수화
                if (lastHoveredObject && lastHoveredObject != hitObject)
                {// 마지막 호버 오브젝트가 존재하고 현재 부딪힌 오브젝트와 다르다면
                    if (lastHoveredObject.TryGetComponent<SelectableObject>(out SelectableObject lastSel))
                    {// 마지막 호버 오브젝트에 SelectableObject컴포넌트가 존재한다면 lastSel 변수에 해당 컴포넌트를 할당하고 조건문 안으로 들임
                        lastSel.HoverController("deactivate");// 해당 오브젝트의 호버를 비활성화
                    }
                    lastHoveredObject = null;// 마지막 호버 오브젝트 제거
                }

                if (hitObject.TryGetComponent<SelectableObject>(out SelectableObject newSel))
                {// Ray에 부딪힌 오브젝트에 SelectableObject 컴포넌트가 존재한다면 newSel 변수에 해당 컴포넌트를 할당하고 조건문 안으로 들임
                    newSel.HoverController("activate");// 해당 오브젝트의 호버를 활성화
                    lastHoveredObject = hitObject;// 마지막 호버 오브젝트 갱신
                }

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {// 만약 마우스 좌클릭을 수행한다면
                    if (lastSelectedObject && hitObject != lastHoveredObject)
                    {// 이전에 선택한 오브젝트가 존재하고 이전 Ray에 부딪힌 오브젝트와 같지 않다면
                        lastSelectedObject.GetComponent<SelectableObject>().selectMode = SelectMode.DEFAULT;// 이전 선택한 오브젝트를 DEFAULT 처리
                        selectedObject = null;
                    }
                    else
                    {// 이전에 선택한 오브젝트가 존재하지 않고 
                        if (hitObject.TryGetComponent<SelectableObject>(out SelectableObject selected))
                        {// Ray에 부딪힌 오브젝트에 SelectableObject 컴포넌트가 존재한다면 selected 변수에 해당 컴포넌트를 할당하고 조건문 안으로 들임
                            lastSelectedObject = selected;
                            selected.OnSelect();//해당 오브젝트의 onSelect() 메서드 수행
                        }
                    }
                }
            }
            else
            {// 만약 부딪힌 것이 없다면
                if (lastHoveredObject)
                {// 이전 호버된 오브젝트가 존재한다면
                    if (lastHoveredObject.TryGetComponent<SelectableObject>(out var lastSel))
                    {// 이전 호버된 오브젝트가 존재한다면
                        lastSel.HoverController("deactivate");// 호버 비활성화
                    }
                    lastHoveredObject = null;// 이전 호버된 오브젝트 제거
                }
            }
        }
    
        // 현재 마우스의 위치가 Plane1 또는 Plane2인지 판단하는 메서드
        public Transform FindPlaneRoot(Transform hitTransform)
        { 
            Transform parent = hitTransform.parent;

            if (parent && parent.name is "Plane1" or "Plane2")
            {
                return parent;
            }

            // 자식이 Plane 자체인 경우도 허용하려면 아래 조건도 추가
            if (hitTransform.name is "Plane1" or "Plane2")
            {
                return hitTransform;
            }

            return null;
        }

    }
}