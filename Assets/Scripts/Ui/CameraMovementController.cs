using UnityEngine;
using UnityEngine.InputSystem;

namespace Ui
{
    public class CameraMovementController : MonoBehaviour 
    {
        private Camera cam;// 카메라 컴포넌트를 저장할 변수
        
        private InputActionAsset cameraInputAsset;
        private InputAction zoomAction;
        private InputAction panningAction;
        private InputAction rotateAction;
        
        private float zoomSpeed = 100f;// 줌 속도(휠 감도)
        private float minZoom = 3f;// 최소 줌 크기
        private float maxZoom = 15f;// 최대 줌 크기
        private float panSpeed = 0.1f;// 카메라 패닝 속도
        private float rotateSpeed = 0.1f;// 회전 속도
        
        private Vector3 lastMousePosition;// 이전 마우스 위치 저장
        private Vector3 initialPosition;// 초기 카메라 위치 저장
        private Quaternion initialRotation;// 초기 회전 저장
        private float initialSize;// 초기 줌 크기 저장
        
        public Transform rotationTarget;// Plane 같은 회전 기준 오브젝트를 드래그해서 할당

        private void Awake()
        {
            cam = GetComponent<Camera>();
            
            cameraInputAsset = Resources.Load<InputActionAsset>("InputSystem_Actions");// InputActionAsset 불러오기
            
            if (!cameraInputAsset)
            {
                return;
            }
            // 이름으로 InputAction 찾기 (Input Map 이름이 "Camera", Action 이름이 "Zoom" 등일 경우)
            zoomAction = cameraInputAsset.FindAction("Camera/Zoom");
            panningAction = cameraInputAsset.FindAction("Camera/Panning");
            rotateAction = cameraInputAsset.FindAction("Camera/Rotate");
        }
        
        private void OnEnable()
        {// 
            cameraInputAsset?.Enable();
        }

        private void Start()
        {
            initialPosition = cam.transform.position;// 카메라 초기 위치
            initialRotation = cam.transform.rotation;// 카메라 초기 회전
            initialSize = cam.orthographicSize;// 카메라 초기 줌
            // Unity Input System에서 입력 액션이 발생했을 때, 그에 대응하는 메서드를 실행하도록 이벤트를 연결
            zoomAction.performed += OnZoom;// Zoom 입력 액션이 실행(Performed)되었을 때, OnZoom메서드를 호출
            panningAction.performed += OnPanning;// Panning 입력 액션이 실행(Performed)되었을 때, OnPanning메서드를 호출
            rotateAction.performed += OnRotate;// Rotate 입력 액션이 실행(Performed)되었을 때, OnRotate메서드를 호출
            // canceled는 입력이 해제되었을 때 발생 시점을 바라본다. 여기선 메서드들이 이벤트 안에서 즉시 한 번만 처리되므로 canceled는 불필요하다.
            // panningAction.canceled += ctx => StopPanning();
            // rotateAction.canceled += ctx => StopRotate();
        }

        private void OnDisable()
        { /** 해당 코드가 필요한 이유
            * 1. 오브젝트가 비활성화될 때 입력 이벤트도 중지되어야 함. Unity에서는 MonoBehaviour가 꺼지거나 씬에서 제거될 때 OnDisable()이 자동 호출.
            * 이때 입력을 받는 상태로 계속 두면, 이미 꺼진 오브젝트에 이벤트가 계속 전달될 수 있어 문제가 발생.
            * 2. 메모리 누수 및 이벤트 중복 방지
            * 3. 컴포넌트 생명주기에 맞는 적절한 Input 처리
            */
            cameraInputAsset?.Disable();
        }
        
        // Zoom 이벤트 핸들러
        private void OnZoom(InputAction.CallbackContext context)
        {
            float scroll = context.ReadValue<float>();// zoomAction값을 읽어들임. 위로 굴리면 +, 아래로 굴리면 -
            if (Mathf.Approximately(scroll, 0f))
            {// scroll과 0f가 같은지(근사한지) 비교. 부동 소수점은 사람이 인식하기에는 같은 값일 수 있으나, ==의 결과가 false가 나올 수 있어 근사치로 비교.
                return;
            }
            cam.orthographicSize -= scroll * zoomSpeed * Time.deltaTime;// 카메라의 확대/축소 정도. Time.deltaTime을 곱함으로서 프레임 속도에 상관없이 일정한 속도로 움직임.
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);// 최소/최대 줌 값 사이로만 움직임.
        }

        // panning 이벤트 핸들러
        private void OnPanning(InputAction.CallbackContext context)
        {
            Vector2 delta = context.ReadValue<Vector2>();// panningAction의 값을 읽어들임.
            if (delta == Vector2.zero)
            {// 입력값이 0이면(사용자가 움직이지 않았다면)
                return;
            }
            Vector3 move = new Vector3(-delta.x * panSpeed, -delta.y * panSpeed, 0);// 사실상 평면 이동이므로 x축 y축만 계산. Vector3를 사용해야 하는 이유는 transform.Translate()이 인자로 Vector3를 받음
            cam.transform.Translate(move, Space.Self);// 계산한 값(위치)로 이동            
        }

        // rotate 이벤트 핸들러
        private void OnRotate(InputAction.CallbackContext context)
        {
            Vector2 delta = context.ReadValue<Vector2>();// rotateAction의 값을 읽어들임
            if (delta == Vector2.zero)
            {
                return;
            }
            float rotX = delta.y * rotateSpeed;
            float rotY = delta.x * rotateSpeed;
            // x축 회전은 카메라의 오른쪽 축 기준, y축 회전은 월드 상 y축 기준
            // 회전 기준을 Plane 등 오브젝트의 위치로 변경
            cam.transform.RotateAround(rotationTarget.position, cam.transform.right, -rotX);
            cam.transform.RotateAround(rotationTarget.position, Vector3.up, rotY);
        }
        
        // 카메라 시점 초기화
        public void ResetCameraView()
        {
            cam.transform.position = initialPosition;
            cam.transform.rotation = initialRotation;
            cam.orthographicSize = initialSize;
        }
    }
    
}