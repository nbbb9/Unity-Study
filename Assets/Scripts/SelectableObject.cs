using UnityEngine;

public class SelectableObject : MonoBehaviour {
    private Color originalColor;//기존 색상(회색)
    private Renderer rend;// 
    public System.Action<PrimitiveType, Vector3> onReselect;// 콜백
    void Start() {
        rend = GetComponent<Renderer>();
        if (rend != null) {
            originalColor = rend.material.color;
        }
    }

    public void OnHover() {
        if (rend != null) {
            rend.material.color = Color.yellow;
        }
    }

    public void OnUnhover() {
        if (rend != null){
            rend.material.color = originalColor;
        }
    }

    public void OnSelect() {
        if (onReselect != null) {
            PrimitiveType type = GetPrimitiveTypeFromName(gameObject.name);
            onReselect.Invoke(type, transform.position);
            Destroy(gameObject); // 기존 오브젝트 제거
        }
    }

    PrimitiveType GetPrimitiveTypeFromName(string name) {
        return (PrimitiveType)System.Enum.Parse(typeof(PrimitiveType), name);
    }
}