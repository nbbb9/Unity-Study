using UnityEngine;
using TMPro;

public class SelectableObject : MonoBehaviour 
{
    private Color originalColor;//기존 색상(회색)
    private Renderer rend;
    public System.Action<PrimitiveType, Vector3> onReselect;// 
    public GameObject infoPopup;// 선택한 ObjectPopup
    public TextMeshProUGUI nameText;// O*N 아래 Text
    public TextMeshProUGUI typeText;// O*T 아래 Text
    public SelectClickMode selectClickMode = SelectClickMode.NONE;// 오브젝트 선택 모드 초기값
    private void Start()
    {
        GameObject canvas = GameObject.Find("Canvas");// 게임 오브젝트에서 찾기
        Transform infoPopupTransform = canvas.transform.Find("ObjectInfoPopup");// 게임 오브젝트에서 찾기
        Transform objectInfo = canvas.transform.Find("ObjectInfoPopup/GameObjectInfo");// 게임 오브젝트에서 찾기
        
        if (objectInfo != null)
        {//오브젝트 정보창이 존재한다면
            infoPopup = infoPopupTransform.gameObject;// 오브젝트 팝업 전역 변수에 할당

            Transform nameObj = objectInfo.Find("name");// 오브젝트 이름
            Transform typeObj = objectInfo.Find("type");// 오브젝트 타입
            if (nameObj != null && typeObj != null)
            {// 이름과 타입이 존재한다면
                nameText = nameObj.GetComponent<TextMeshProUGUI>();// 오브젝트 이름 할당
                typeText = typeObj.GetComponent<TextMeshProUGUI>();// 오브젝트 타입 할당
            }
            
        }

        rend = GetComponent<Renderer>();// 현재 오브젝트에 붙어 있는 Renderder 컴포넌트를 가져와서 rend에 저장
        if (rend != null)
        {// rend가 존재할 경우
            originalColor = rend.material.color;// 현재 오브젝트의 머터리얼에서 색상을 가져와 originalColor에 저장
        }
    }

    void Update()
    {
        Vector3 pos = transform.position;// 3차원 포지션 변수 선언

        // x와 z의 구역 제한
        pos.x = Mathf.Clamp(pos.x, -11.5f, 11.5f);
        pos.z = Mathf.Clamp(pos.z, -7.5f, 7.5f);
        pos.y = 1.51f;// Plane 바로 위로 고정

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
        Debug.Log($"[OnSelect] 현재 SelectClickMode: {selectClickMode}");

        // 선택 모드 전환 처리
        if (selectClickMode == SelectClickMode.NONE)
        {
            selectClickMode = SelectClickMode.JUSTSELECT;
            Debug.Log("[OnSelect] 상태 변경: NONE → JUSTSELECT");
        }
        else if (selectClickMode == SelectClickMode.JUSTSELECT)
        {
            selectClickMode = SelectClickMode.MOVE;
            Debug.Log("[OnSelect] 상태 변경: JUSTSELECT → MOVE");
        }
        else if (selectClickMode == SelectClickMode.MOVE)
        {
            selectClickMode = SelectClickMode.NONE;
            Debug.Log("[OnSelect] 상태 변경: MOVE → NONE");
        }

        if (onReselect != null)
        {
            PrimitiveType type = GetPrimitiveTypeFromName(gameObject.name);
            onReselect.Invoke(type, transform.position);
        }

        if (infoPopup != null && nameText != null && typeText != null)
        {
            // UI 활성화 및 정보 출력
            infoPopup.SetActive(true);
            nameText.text = $"{gameObject.name}";
            typeText.text = $"{GetPrimitiveTypeFromName(gameObject.name)}";
        }
    }


    // 이름에서 타입 추출
    PrimitiveType GetPrimitiveTypeFromName(string name)
    {
        return (PrimitiveType)System.Enum.Parse(typeof(PrimitiveType), name);
    }
}