using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using SelectMode = Enums.SelectMode;

namespace Object
{
    public class ObjectPlacementHandler : MonoBehaviour
    {
        private GameObject selectedObject;// 선택한 오브젝트
        private GameObject lastSelectedObject;// 마지막으로 선택한 오브젝트
        private Vector3 originalPosition;// 드래그 시작 시점 위치를 저장할 변수
        private GameObject installWarningPopup;// 설치 오류 팝업
        private GameObject lastHoveredObject;// 이전 호버 오브젝트
        
        private bool isRotating = false; // 회전 중 여부
        public float rotationDuration = 0.5f; // 회전 애니메이션 시간(부드러운 회전에 걸리는 시간) - 0.3초


        // 선택한 오브젝트 및 기존 위치 Set
        public void SetSelectedObject(GameObject obj)
        {
            selectedObject = obj;
            originalPosition = obj ? obj.transform.position : Vector3.zero;
        }
        
        // 경고 문구 팝업 Set
        public void SetInstallWarningPopup(GameObject popup)
        {
            installWarningPopup = popup;
        }

        // 드래그 시작 준비
        public void StartDragging()
        {// 드래그가 시작되기 전, 드래그 대상 오브젝트의 원래 위치를 저장(드래그 도중 설치 불가한 위치인 경우 되돌리기 위한 기준점을 설정하기 위해)
            if (selectedObject)
            {// 선택한 오브젝트가 존재한다면
                originalPosition = selectedObject.transform.position;
            }
        }
    
        // 드래그 수행
        public void Drag()
        {
            if (!selectedObject) return;
        
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());// 카메라 기준 ray 선언
            RaycastHit[] hits = Physics.RaycastAll(ray);// ray기준 모든 충돌을 감지
            
            // 만약 아무것도 부딪힌것이 없다면.(공중이라면)
            if (hits.Length == 0) return;// 그냥 리턴(드래그 중지)

            Vector3? newPosition = null;//드래그 대상 오브젝트의 새로운 위치 저장 변수
            Transform newParent = null;// 드래그 대상 오브젝트의 새로운 부모 저장 변수

            foreach (RaycastHit hit in hits) // => foreach는 향상된 for문 과 같다
            {// 만약 Ray가 충돌한 모든 오브젝트에 대해 하나씩 처리하기 위한 반복문
                if (hit.transform != selectedObject.transform && newPosition == null)
                {// Ray가 충돌한 오브젝트가 선택한 오브젝트가 아니고, 아직 새로운 position이 설정되지 않았을 경우
                    Vector3 position = hit.point;// 충돌 지점의 위치
                    float objectHeight = selectedObject.GetComponent<Renderer>().bounds.size.y;// 선택한 오브젝트의 높이
                    position.y += objectHeight / 2f;// 오브젝트 살짝 띄우기
                    newPosition = position;// 새로운 위치 갱신
                }
                
                if (hit.transform != selectedObject.transform)
                {// 만약 부딪힌 transform이 선택한 오브젝트의 트랜스폼과 다르다면(선택된 오브젝트가 아닌 대상에 대해, 해당 트랜스폼이 부모 Plane이 될 수 있는지)
                    Transform candidateParent = FindPlaneRoot(hit.transform);// 부모 평면은 드래그할 오브젝트를 제외한 첫 번째 plane 후보
                    if (candidateParent)
                    {// 만약 부모 후보가 있다면
                        newParent = candidateParent;// 새로운 부모 오브젝트 갱신
                        break;// plane 후보는 하나만 잡고 종료( 더 이상 다른 후보를 찾지 않기 위해 )
                    }
                }
            }
            
            if (newPosition.HasValue)
            {// 만약 드래그 대상 오브젝트의 새로운 위치가 값을 가지고 있다면
                selectedObject.transform.position = newPosition.Value;// 새로운 위치의 값을 선택한 오브젝트의 위치값에 갱신
            }

            if (newParent && selectedObject.transform.parent != newParent)
            {// 만약 새로운 부모가 존재하고 선택한 오브젝트의 부모와 새로운 부모가 다르다면
                selectedObject.transform.SetParent(newParent);// 선택한 오브젝트의 부모 갱신
            }
            
        }

        // 드래그 종료
        public void EndDragging()
        {
            if (!selectedObject) return;
            SelectableObject selectable = selectedObject.GetComponent<SelectableObject>();// 선택한 오브젝트의 컴포넌트 세팅
            if (selectable && selectable.infoPopup)
            {// 선택한 오브젝트가 존재하고 정보 팝업이 존재한다면
                selectable.infoPopup.SetActive(false);// 정보 팝업 비활성화
                selectable.selectMode = SelectMode.DEFAULT;// Default 모드로 변경
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
                        lastSel.HoverModeController("deactivate");// 해당 오브젝트의 호버를 비활성화
                    }
                    lastHoveredObject = null;// 마지막 호버 오브젝트 제거
                }

                if (hitObject.TryGetComponent<SelectableObject>(out SelectableObject newSel))
                {// Ray에 부딪힌 오브젝트에 SelectableObject 컴포넌트가 존재한다면 newSel 변수에 해당 컴포넌트를 할당하고 조건문 안으로 들임
                    newSel.HoverModeController("activate");// 해당 오브젝트의 호버를 활성화
                    lastHoveredObject = hitObject;// 마지막 호버 오브젝트 갱신
                }
                // --- 여기까지가 호버고 아래는 사실상 선택 ---
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {// 만약 마우스 좌클릭을 수행한다면
                    if (lastSelectedObject && hitObject != lastSelectedObject)
                    {// 이전에 선택한 오브젝트가 존재하고 Ray에 부딪힌 오브젝트와 같지 않다면
                        lastSelectedObject.GetComponent<SelectableObject>().selectMode = SelectMode.DEFAULT;// 이전 선택한 오브젝트를 DEFAULT 처리
    
                        selectedObject = hitObject;
                        lastSelectedObject = null;

                        if (selectedObject.TryGetComponent<SelectableObject>(out SelectableObject sel))
                        {
                            sel.OnSelect(); // 새로 선택한 오브젝트의 OnSelect 호출
                            lastSelectedObject = selectedObject; // 마지막 선택 오브젝트 갱신
                        }
                    }
                    else
                    {// 이전에 선택한 오브젝트가 존재하지 않고
                        if (hitObject.TryGetComponent<SelectableObject>(out SelectableObject selected))
                        {// Ray에 부딪힌 오브젝트에 SelectableObject 컴포넌트가 존재한다면 selected 변수에 해당 컴포넌트를 할당하고 조건문 안으로 들임
                            lastSelectedObject = selected.gameObject;
                            selected.OnSelect();//해당 오브젝트의 onSelect() 메서드 수행
                        }
                    }
                }
            }
            else
            {// 만약 부딪힌 것이 없다면
                if (lastHoveredObject)
                {// 이전 호버된 오브젝트가 존재한다면
                    if (lastHoveredObject.TryGetComponent<SelectableObject>(out SelectableObject lastSel))
                    {// 이전 호버된 오브젝트가 존재한다면
                        lastSel.HoverModeController("deactivate");// 호버 비활성화
                    }
                    lastHoveredObject = null;// 이전 호버된 오브젝트 제거
                }
            }
        }
    
        // 부모가 Plane1 또는 Plane2인지 판단하는 메서드
        public Transform FindPlaneRoot(Transform hitTransform)
        {
            Transform parent = hitTransform.parent;// 인자로 받은 트랜스 폼의 부모를 변수화
            
            if (parent && parent.name is "Plane1" or "Plane2")
            {// 만약 부모가 존재하고 이름이 Plane1또는 Plane2라면
                return parent;// 부모 반환
            }
            
            if (hitTransform.name is "Plane1" or "Plane2")
            {// 인자로 받은 트랜스폼이 Plane1 또는 Plane2 그 자체일 때
                return hitTransform;
            }

            return null;
        }
        
        // 오브젝트 회전
        public IEnumerator RotateSelectedObject(Vector3 axis, float angle)
        {
            if (!selectedObject) yield break;// 선택한 오브젝트가 존재하지 않으면 코루틴 종료
    
            isRotating = true;// 회전 플래그
            
            Quaternion startRotation = selectedObject.transform.rotation;// 현재 오브젝트의 회전 상태를 Quaternion 형태로 저장. 회전의 시작점
            Quaternion endRotation = Quaternion.AngleAxis(angle, axis) * startRotation;// 특정 축을 기준으로 각도만큼 회전하는 쿼터니언을 생성. 쿼터니언 곱셈은 누적 회전을 의미한다.
            
            float elapsed = 0f;// 경과 시간을 저장할 변수

            while (elapsed < rotationDuration)
            {// rotationDuration만큼 회전 반복 실행
                selectedObject.transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / rotationDuration);// 시작 회전에서 끝 회전까지 **부드럽게 보간(Spherical Linear Interpolation)**합니다.
                elapsed += Time.deltaTime;// 매 프레임마다 경과 시간에 Time.deltaTime(프레임 간 시간 간격)을 누적
                yield return null;// 현재 프레임을 마치고 다음 프레임까지 대기. 
                // 코루틴의 핵심: 이렇게 함으로써 while 루프가 한 프레임씩 실행되어 애니메이션처럼 동작
            }

            selectedObject.transform.rotation = endRotation;// 마지막에 회전이 미세하게 부족할 수 있으므로, 정확하게 최종 회전 상태로 보정

            isRotating = false;// 회전 플래그
        }

    }
}