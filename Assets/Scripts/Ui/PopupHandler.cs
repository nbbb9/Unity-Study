using UnityEngine;

// 유니티의 UI조작을 위해 꼭 필요한 엔진

namespace Ui
{
    public class PopupHandler : MonoBehaviour 
    {
        public GameObject selectObjectListPopup;
        private bool isPopupOpen = false;// 팝업 열림 여부

        public void TogglePopup()
        {
            isPopupOpen = !isPopupOpen;
            selectObjectListPopup.SetActive(isPopupOpen);// 패널 열림 설정(boolean에 따라 활성화 여부)
        }
    
    }
}