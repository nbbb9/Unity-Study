using UnityEngine;
using UnityEngine.InputSystem;

namespace Ui
{
    public class CameraMovementController : MonoBehaviour 
    {
        private InputActionAsset inputAsset;
        private InputAction zoomAction;
        public InputAction panningAction;
        public InputAction rotateAction;
        
        private float zoomSpeed = 100f;// 줌 속도(휠 감도)
        private float minZoom = 3f;// 최소 줌 크기
        private float maxZoom = 15f;// 최대 줌 크기
        private float panSpeed = 0.1f;// 카메라 패닝 속도
        private float rotateSpeed = 0.1f;// 회전 속도
        
        private Camera cam;// 카메라 컴포넌트를 저장할 변수
        private Vector3 lastMousePosition;// 이전 마우스 위치 저장
        private Vector3 initialPosition;// 초기 카메라 위치 저장
        private Quaternion initialRotation;// 초기 회전 저장
        private float initialSize;// 초기 줌 크기 저장
        
        public Transform rotationTarget;// Plane 같은 회전 기준 오브젝트를 드래그해서 할당

        private void Awake()
        {
            cam = GetComponent<Camera>();
            
            // InputActionAsset 로드 (사진에 보이는 이름에 맞춰 수정)
            inputAsset = Resources.Load<InputActionAsset>("InputSystem_Actions");
            
            if (inputAsset == null)
            {
                return;
            }
            
            // 이름으로 InputAction 찾기 (Input Map 이름이 "Camera", Action 이름이 "Zoom" 등일 경우)
            zoomAction = inputAsset.FindAction("Camera/Zoom");
            panningAction = inputAsset.FindAction("Camera/Panning");
            rotateAction = inputAsset.FindAction("Camera/Rotate");
        }

        private void Start()
        {   
            cam = GetComponent<Camera>();
            initialPosition = cam.transform.position;// 카메라 초기 위치
            initialRotation = cam.transform.rotation;// 카메라 초기 회전
            initialSize = cam.orthographicSize;// 카메라 초기 줌
            
            // 이벤트 연결
            zoomAction.performed += OnZoom;
            panningAction.performed += OnPanning;
            rotateAction.performed += OnRotate;

            // panningAction.canceled += ctx => StopPanning();
            // rotateAction.canceled += ctx => StopRotate();
        }
        
        private void OnEnable()
        {// 액션들 활성화
            inputAsset?.Enable();
        }

        private void OnDisable()
        { // 액션들 비활성화
            inputAsset?.Disable();
        }
        
        // Zoom 이벤트 핸들러
        private void OnZoom(InputAction.CallbackContext context)
        {
            float scroll = context.ReadValue<float>();// zoomAction값을 읽어들임. 위로 굴리면 +, 아래로 굴리면 -

            if (Mathf.Approximately(scroll, 0f))
            {
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
            {
                return;
            }
            
            Vector3 move = new Vector3(-delta.x * panSpeed, -delta.y * panSpeed, 0);// 사실상 평면 이동이므로 x축 y축만 계산. Vector3를 사용해야 하는 이유는 transform.Translate()이 인자로 Vector3를 받음
            cam.transform.Translate(move, Space.Self);// 계산한 값(위치)로 이동            
        }

        // 회전 메서드
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