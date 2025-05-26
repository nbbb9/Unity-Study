using UnityEngine;
using UnityEngine.InputSystem;

namespace Ui
{
    public class CameraMovementController : MonoBehaviour 
    {
        public float zoomSpeed = 100f;// 줌 속도(휠 감도)
        public float minZoom = 3f;// 최소 줌 크기
        public float maxZoom = 15f;// 최대 줌 크기
        public float panSpeed = 5f;// 카메라 패닝 속도
        public float rotateSpeed = 20f; // 회전 속도
        private Camera cam;// 카메라 컴포넌트를 저장할 변수
        private Vector3 lastMousePosition;// 이전 마우스 위치 저장
        private Vector3 initialPosition;// 초기 카메라 위치 저장
        private Quaternion initialRotation;// 초기 회전 저장
        private float initialSize;// 초기 줌 크기 저장
        public Transform rotationTarget;// Plane 같은 회전 기준 오브젝트를 드래그해서 할당

        private void Start()
        {   // 해당 컴포넌트(코드 파일)를 붙인 오브젝트(카메라)에서 camera컴포넌트를 가져와 cam변수에 저장.
            // 이후 카메라 속성 변경 시 이 cam을 사용.
            // 초기 위치/회전/줌 정보를 저장
            cam = GetComponent<Camera>();
            initialPosition = cam.transform.position;// 카메라 초기 위치
            initialRotation = cam.transform.rotation;// 카메라 초기 회전
            initialSize = cam.orthographicSize;// 카메라 초기 줌
        }

        private void Update()
        {
            HandleZoom();
            HandlePan();
            HandleRotate();
        }

        // Zoom 메서드
        void HandleZoom()
        {
            float scroll = Mouse.current.scroll.ReadValue().y;// 현재 마우스 휠 입력값을 읽어들임. 위로 굴리면 +, 아래로 굴리면 -
            if (scroll != 0f)
            {
                cam.orthographicSize -= scroll * zoomSpeed * Time.deltaTime;// 카메라의 확대/축소 정도. Time.deltaTime을 곱함으로서 프레임 속도에 상관없이 일정한 속도로 움직임.
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);// 최소/최대 줌 값 사이로만 움직임.
            }
        }

        // panning 메서드
        void HandlePan()
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {   // 우클릭을 처음 누른 순간 현재 마우스 위치를 저장.
                // 이후 마우스를 얼마나 이동했는지를 계산하기 위해 기준점으로 사용.
                lastMousePosition = Mouse.current.position.ReadValue();
            }
            if (Mouse.current.rightButton.isPressed)
            {   // 마우스 우클릭을 계속 누르고 있을 때, 마우스를 얼마나 움직였는지
                // 계산해서 그 만큼 카메라를 XY 평면으로 이동
                Vector2 currentMousePos = Mouse.current.position.ReadValue();
                Vector2 delta = currentMousePos - (Vector2)lastMousePosition;// 이동 범위
                Vector3 move = new Vector3(-delta.x * panSpeed * Time.deltaTime, -delta.y * panSpeed * Time.deltaTime, 0);// 사실상 평면 이동이므로 x축 y축만 계산
                cam.transform.Translate(move);// 계산한 값(위치)로 이동
                lastMousePosition = currentMousePos;// 마지막 마우스 위치 갱신
            }
        }

        // 회전 메서드
        void HandleRotate()
        {
            if (Mouse.current.middleButton.isPressed && rotationTarget)
            {// 마우스 휠을 클릭한 상태에서 마우스를 움직이면 실행됨.
                Vector2 delta = Mouse.current.delta.ReadValue();// 현재 프레임에서 마우스가 얼마나 이동했는지를 얻는다.
                float rotX = delta.y * rotateSpeed * Time.deltaTime;
                float rotY = delta.x * rotateSpeed * Time.deltaTime;
                // x축 회전은 카메라의 오른쪽 축 기준, y축 회전은 월드 상 y축 기준
                // 회전 기준을 Plane 등 오브젝트의 위치로 변경
                cam.transform.RotateAround(rotationTarget.position, cam.transform.right, -rotX);
                cam.transform.RotateAround(rotationTarget.position, Vector3.up, rotY);
            }
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