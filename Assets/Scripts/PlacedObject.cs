using UnityEngine;

public class PlaceableObject : MonoBehaviour {
    private Renderer objectRenderer;
    private Color originalColor;
    private bool isMouseOver = false;
    public bool isMovable = false;

    void Start() {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null) {
            originalColor = objectRenderer.material.color;
        }
    }

    void Update() {
        if (isMovable && isMouseOver && Input.GetMouseButton(0)) {
            MoveWithMouse();
        }

        if (isMovable && Input.GetMouseButtonUp(0)) {
            isMovable = false; // 드래그 종료
        }
    }

    void MoveWithMouse() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            Vector3 position = hit.point;
            float height = GetComponent<Renderer>().bounds.size.y;
            position.y += height / 2f;
            transform.position = position;
        }
    }

    void OnMouseEnter() {
        isMouseOver = true;
        if (objectRenderer != null) {
            objectRenderer.material.color = Color.yellow;
        }
    }

    void OnMouseExit() {
        isMouseOver = false;
        if (objectRenderer != null) {
            objectRenderer.material.color = originalColor;
        }
    }

    void OnMouseDown() {
        if (isMouseOver) {
            isMovable = true;
            Debug.Log($"{gameObject.name} is now movable!");
            // 추가적으로 이동 로직을 여기에 연결하세요
        }
    }
}
