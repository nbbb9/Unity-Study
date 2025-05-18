using UnityEngine;
using UnityEngine.UI;

public class SelectableObject : MonoBehaviour {
    private Color originalColor;//기존 색상(회색)
    private Renderer rend;// 
    public System.Action<PrimitiveType, Vector3> onReselect;// 콜백
    public GameObject infoPopup; // 선택한 Object popup
    public Text nameText;        // O*N 아래 Text
    public Text typeText;        // O*T 아래 Text
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
        }
        // UI 활성화 및 정보 출력
        if (infoPopup != null && nameText != null && typeText != null) {
            infoPopup.SetActive(true);
            nameText.text = $"이름: {gameObject.name}";
            typeText.text = $"타입: {GetPrimitiveTypeFromName(gameObject.name)}";
        }
    }

    PrimitiveType GetPrimitiveTypeFromName(string name) {
        return (PrimitiveType)System.Enum.Parse(typeof(PrimitiveType), name);
    }
}