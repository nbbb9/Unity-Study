using System;
using Enums;
using TMPro;
using UnityEngine;

namespace Object
{
    public class SelectableObject : MonoBehaviour 
    {
        private Color originalColor;//기존 색상(회색)
        private Renderer rend;
        public Action<PrimitiveType, Vector3> onReselect;// 
        public PrimitiveType objectType;
        public GameObject infoPopup;// 선택한 ObjectPopup
        public TextMeshProUGUI nameText;// O*N 아래 Text
        public TextMeshProUGUI typeText;// O*T 아래 Text
        public SelectMode selectMode = SelectMode.DEFAULT;// 오브젝트 선택 모드 초기값
        private static readonly Color HoverColor = Color.yellow;
    
        private void Start()
        {
            GameObject canvas = GameObject.Find("Canvas");// 게임 오브젝트에서 찾기
            Transform infoPopupTransform = canvas.transform.Find("ObjectInfoPopup");// 게임 오브젝트에서 찾기
            Transform objectInfo = canvas.transform.Find("ObjectInfoPopup/GameObjectInfo");// 게임 오브젝트에서 찾기
        
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

            rend = GetComponent<Renderer>();// 현재 오브젝트에 붙어 있는 Renderder 컴포넌트를 가져와서 rend에 저장
            if (rend)
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
            pos.y = 1.6f;// Plane 바로 위로 고정
            transform.position = new Vector3(pos.x, pos.y, pos.z);// 제한 위치 설정
            
            // JUSTSELECT 모드일 경우 노란색 유지
            if ((selectMode == SelectMode.JUSTSELECT && rend && rend.material.color != HoverColor) ||
                (selectMode == SelectMode.HOVERED && rend && rend.material.color != HoverColor))
            {
                rend.material.color = HoverColor;
            }
            // 그 외 모드에서는 원래 색상 복원
            else if (selectMode == SelectMode.DEFAULT && rend && rend.material.color != originalColor)
            {
                rend.material.color = originalColor;
            }
        }

        // 호버 컨트롤러
        public void HoverController(String type)
        {
            if (!rend) return;
            
            bool isJustSelect = selectMode == SelectMode.JUSTSELECT;
            
            switch (type)
            {
                case "activate":
                    if (!isJustSelect)
                        selectMode = SelectMode.HOVERED;
                    rend.material.color = HoverColor;
                    break;

                case "deactivate":
                    if (!isJustSelect)
                        selectMode = SelectMode.DEFAULT;
                    rend.material.color = originalColor;
                    break;
            }
        }
    
        // 선택했을 때 수행되는 메서드
        public void OnSelect()
        {
            // 선택 모드 전환 처리
            switch (selectMode)
            {
                case SelectMode.DEFAULT:
                    selectMode = SelectMode.JUSTSELECT;
                    break;
                case SelectMode.HOVERED:
                    selectMode = SelectMode.JUSTSELECT;
                    break;
                case SelectMode.JUSTSELECT:
                    selectMode = SelectMode.MOVE;
                    break;
                case SelectMode.MOVE:
                    selectMode = SelectMode.JUSTSELECT;
                    break;
            }

            Debug.Log("현재 모드 : " + selectMode);
            
            if (onReselect != null)
            {
                // PrimitiveType type = GetPrimitiveTypeFromName(gameObject.name);// 오브젝트 이름에서 타입 추출
                // onReselect.Invoke(type, transform.position);
                onReselect.Invoke(objectType, transform.position); // 이름 대신 타입 필드 사용
            }

            if (infoPopup && nameText && typeText)
            {
                // UI 활성화 및 정보 출력
                infoPopup.SetActive(true);
                nameText.text = $"{gameObject.name}";
                typeText.text = $"{objectType}";
            }
        }
        
    }
}