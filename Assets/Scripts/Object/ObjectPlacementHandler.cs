using UnityEngine;
using UnityEngine.InputSystem;

namespace Object
{
    public class ObjectPlacementHandler : MonoBehaviour
    {
        private GameObject selectedObject;// 선택한 오브젝트
        private Vector3 originalPosition;// 드래그 시작 시점 위치를 저장할 변수
        private GameObject installWarningPopup;// 설치 오류 팝업
        private GameObject lastHoveredObject;// 이전 호버 오브젝트

        public void setSelectedObject(GameObject obj)
        {
            selectedObject = obj;
            originalPosition = obj != null ? obj.transform.position : Vector3.zero;
        }
    
        public void setInstallWarningPopup(GameObject popup)
        {
            installWarningPopup = popup;
        }

        public void StartDragging()
        {
            if (selectedObject != null)
            {// 선택한 오브젝트가 존재한다면
                originalPosition = selectedObject.transform.position;
            }
        }

    
        // 드래그 시작
        public void Drag()
        {
            if (selectedObject == null)
            {
                return;
            }
        
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        
            if (Physics.Raycast(ray, out RaycastHit hit))
            {// 만약 ray에 부딪힌것이 있다면
                Vector3 position = hit.point;// 부딪힌 부분 위치 변수
                float objectHeight = selectedObject.GetComponent<Renderer>().bounds.size.y;// 오브젝트 위치 설정
                position.y += objectHeight / 2f;// 오브젝트 높이 계산
                selectedObject.transform.position = position;// 선택한 오브젝트의 위치를 갱신
            }
        
            Ray downRay = new Ray(selectedObject.transform.position, Vector3.down);// 선택한 오브젝트를 기준으로 아래로 Ray생성.
        
            if (Physics.Raycast(downRay, out RaycastHit downHit))
            {// 만약 오브젝트 기준으로 부딪히는 부분이 있다면
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

        // 드래그 종료
        public void EndDragging()
        {
            if (selectedObject == null)
            {
                return;
            }
        
            // Debug.Log("드래그 끝!");
            // 현재 위치가 유효한 Plane 위가 아닌 경우 되돌림
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit))
            {// 만약 ray에 부딪힌것이 있다면
                Transform plane = FindPlaneRoot(hit.transform);// 현재 부딪힌 평면 찾기
                if (plane == null)
                {// 만약 Plane이 null이라면
                    installWarningPopup.SetActive(true);// 경고 문구 출력
                    selectedObject.transform.position = originalPosition;// 이전 위치로 되돌림
                }
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
                if (lastHoveredObject != null && lastHoveredObject != hitObject)
                {
                    if (lastHoveredObject.TryGetComponent<SelectableObject>(out var lastSel))
                    {
                        lastSel.HoverController("deactivate");
                    }
                    lastHoveredObject = null;
                }

                if (hitObject.TryGetComponent<SelectableObject>(out var newSel))
                {
                    newSel.HoverController("activate");
                    lastHoveredObject = hitObject;
                }

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    if (hitObject.TryGetComponent<SelectableObject>(out var selected))
                    {
                        selected.OnSelect();
                    }
                }
            }
            else
            {// 만약 부딪힌 것이 없다면
                if (lastHoveredObject != null)
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
            Transform current = hitTransform;
            while (current != null)
            {
                if (current.name == "Plane1" || current.name == "Plane2")
                {//만약 인자로 받은 TransForm이 Plane1또는 Plane2라면 현재 위치 반환.
                    return current;
                }
                current = current.parent;
            }
            return null;
        }

    }
}