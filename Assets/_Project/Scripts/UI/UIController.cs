using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Button button;
    public CanvasGroup[] UIToShow;
    public CanvasGroup[] UIToHide;
    void Awake()
    {
        button = GetComponent<Button>();
    }
    void Start()
    {
        if(button != null)
        {
            button.onClick.AddListener(DoAction);
        }
    }
    public void DoAction()
    {
        Show();
        Hide();
    }
    public void Show()
    {
        foreach(CanvasGroup uiToShow in UIToShow)
        {
            uiToShow.blocksRaycasts = true;
            uiToShow.alpha = 1;
            uiToShow.interactable = true;
        }
    }
    public void Hide()
    {
        foreach(CanvasGroup uiToHide in UIToHide)
        {
            uiToHide.blocksRaycasts = false;
            uiToHide.alpha = 0;
            uiToHide.interactable = false;
        }
    }
}
