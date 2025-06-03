using System;
using Enums;
using TMPro;
using UnityEngine;

namespace Object
{
    public class SelectableObject : MonoBehaviour 
    {
        private Renderer rend;
        
        public Action<String, Vector3> onReselect;// ??????????????
        
        public String objectType;// 현재 오브젝트의 타입
        
        public GameObject infoPopup;// 선택한 ObjectPopup
        private TextMeshProUGUI nameText;// O*N 아래 Text
        private TextMeshProUGUI typeText;// O*T 아래 Text
        
        public SelectMode selectMode;// 오브젝트 선택 모드 초기값
        
        private static readonly Color HoverColor = Color.yellow;// 호버 색상 
        private Color originalColor;//기존 색상(회색)
    
        private void Start()
        {
            GameObject canvas = GameObject.Find("Canvas");
            Transform infoPopupTransform = canvas.transform.Find("Popup/ObjectInfoPopup");
            Transform objectInfo = canvas.transform.Find("Popup/ObjectInfoPopup/GameObjectInfo");
        
            if (objectInfo)
            {//오브젝트 정보창이 존재한다면
                infoPopup = infoPopupTransform.gameObject;// 오브젝트 팝업 전역 변수에 할당
                Transform nameObj = objectInfo.Find("name");// 오브젝트 이름
                Transform typeObj = objectInfo.Find("type");// 오브젝트 타입
                if (nameObj && typeObj)
                {// 이름과 타입이 존재한다면
                    nameText = nameObj.GetComponent<TextMeshProUGUI>();// 오브젝트 이름 할당
                    typeText = typeObj.GetComponent<TextMeshProUGUI>();// 오브젝트 타입 할당
                }
            }

            selectMode = SelectMode.DEFAULT;// 선택 모드 초기값 '기본'
            
            rend = GetComponent<Renderer>();// 현재 오브젝트에 붙어 있는 Renderder 컴포넌트를 가져와서 rend에 저장
            if (rend) originalColor = rend.material.color;// 현재 오브젝트의 머터리얼에서 색상을 가져와 originalColor에 저장
        }
        
        // 호버모드 컨트롤러
        public void HoverModeController(String type)
        {
            if (!rend) return;// 
            
            bool isJustSelect = selectMode == SelectMode.JUSTSELECT;// 단순 선택 모드 여부 Flag
            
            switch (type)
            {// 타입에 따른 호버 활성, 비활성
                case "activate":
                    if (!isJustSelect) selectMode = SelectMode.HOVERED;
                    rend.material.color = HoverColor;
                    break;

                case "deactivate":
                    if (isJustSelect) selectMode = SelectMode.JUSTSELECT;
                    else rend.material.color = originalColor;
                    break;
            }
        }
    
        // 선택했을 때 수행되는 메서드
        public void OnSelect()
        {
            switch (selectMode)
            {// 선택 모드 전환
                case SelectMode.DEFAULT:
                    selectMode = SelectMode.JUSTSELECT;
                    rend.material.color = HoverColor;
                    break;
                case SelectMode.HOVERED:
                    selectMode = SelectMode.JUSTSELECT;
                    rend.material.color = HoverColor;
                    break;
                case SelectMode.JUSTSELECT:
                    selectMode = SelectMode.MOVE;
                    rend.material.color = HoverColor;
                    break;
                case SelectMode.MOVE:
                    selectMode = SelectMode.JUSTSELECT;
                    rend.material.color = HoverColor;
                    break;
            }
            
            Debug.Log(" 333 ");
            if (onReselect != null)
            {// 
                Debug.Log(" 444 ");
                onReselect.Invoke(objectType, transform.position); // 이름 대신 타입 필드 사용
            }

            if (infoPopup && nameText && typeText)
            {// UI 활성화 및 정보 출력
                infoPopup.SetActive(true);
                nameText.text = $"{gameObject.name}";
                typeText.text = $"{objectType}";
            }
        }
        
    }
}