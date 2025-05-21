using UnityEngine;
using TMPro;

public class SelectableObject : MonoBehaviour 
{
    private Color originalColor;//기존 색상(회색)
    private Renderer rend;// 
    public System.Action<PrimitiveType, Vector3> onReselect;// 
    public GameObject infoPopup;// 선택한 Object popup
    public TextMeshProUGUI nameText;// O*N 아래 Text
    public TextMeshProUGUI typeText;// O*T 아래 Text
    public SelectClickMode selectClickMode = SelectClickMode.NONE;
    void Start()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            Transform InfoPopupTransform = canvas.transform.Find("ObjectInfoPopup");
            Transform popupTransform = canvas.transform.Find("ObjectInfoPopup/GameObjectInfo");
            if (popupTransform != null)
            {
                infoPopup = InfoPopupTransform.gameObject;

                Transform nameObj = popupTransform.Find("name");
                if (nameObj != null)
                    nameText = nameObj.GetComponent<TextMeshProUGUI>();

                Transform typeObj = popupTransform.Find("type");
                if (typeObj != null)
                    typeText = typeObj.GetComponent<TextMeshProUGUI>();
            }
        }

        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalColor = rend.material.color;
        }
    }

    void Update()
    {
        Vector3 pos = transform.position;

        // x와 z를 -5에서 5로 제한
        pos.x = Mathf.Clamp(pos.x, -11.5f, 11.5f);
        pos.z = Mathf.Clamp(pos.z, -7.5f, 7.5f);
        pos.y = 1.5f; // Plane 바로 위로 고정

        // y는 그대로 유지, z는 위에서 클램핑
        transform.position = new Vector3(pos.x, pos.y, pos.z);
    }

    // 호버 활성화
    public void OnHover()
    {
        if (rend != null)
        {
            rend.material.color = Color.yellow;
        }
    }

    // 호버 비활성화
    public void OnUnhover()
    {
        if (rend != null)
        {
            rend.material.color = originalColor;
        }
    }

    // 
    public void OnSelect()
    {
        if (onReselect != null)
        {
            PrimitiveType type = GetPrimitiveTypeFromName(gameObject.name);
            onReselect.Invoke(type, transform.position);
        }

        // UI 활성화 및 정보 출력
        infoPopup.SetActive(true);
        nameText.text = $"{gameObject.name}";
        typeText.text = $"{GetPrimitiveTypeFromName(gameObject.name)}";
    }

    // 이름에서 타입 추출
    PrimitiveType GetPrimitiveTypeFromName(string name)
    {
        return (PrimitiveType)System.Enum.Parse(typeof(PrimitiveType), name);
    }
}