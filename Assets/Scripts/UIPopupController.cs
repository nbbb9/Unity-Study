using UnityEngine;

public class UIPopupController : MonoBehaviour {
    public GameObject selectObject;// 팝업으로 띄울 패널(파일명이 같아야함)

    private bool isPopupOpen = false;// 팝업 열림 여부

    public void TogglePopup() {
        isPopupOpen = !isPopupOpen;
        selectObject.SetActive(isPopupOpen);// 패널 열림 설정(boolean에 따라 활성화 여부)
    }
}
