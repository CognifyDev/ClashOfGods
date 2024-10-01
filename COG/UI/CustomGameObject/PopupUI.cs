using COG.Utils;
using UnityEngine;

namespace COG.UI.CustomGameObject;

public class PopupUI : ICustomGameObject
{
    public readonly GenericPopup Popup;
    public readonly SpriteRenderer Background;
    public readonly GameObject ExitButtonGameObject;
    
    public PopupUI(string name)
    {
        Popup = GameUtils.Popup!;
        Popup.name = name;
        Popup.transform.localPosition = new Vector3(0, 0, 0);
        Object.DontDestroyOnLoad(Popup);
        
        Popup.transform.Find("Text_TMP").gameObject.SetActive(false);
        
        Background = Popup.transform.Find("Background").GetComponent<SpriteRenderer>();
        
        var exitButton = Popup.transform.Find("ExitGame");
        var exitPassiveButton = exitButton.GetComponent<PassiveButton>();
        exitPassiveButton.OnClick.RemoveAllListeners();
        exitPassiveButton.OnMouseOver.RemoveAllListeners();
        exitPassiveButton.StopAllCoroutines();
        ExitButtonGameObject = Object.Instantiate(exitButton.gameObject);
        exitButton.gameObject.SetActive(false);
    }

    public void Show(string text = "")
    {
        Popup.Show(text);
    }

    public void AddButton(GameObject button)
    {
        var buttonTransform = button.transform;
        buttonTransform.SetParent(Popup.transform);
    }
}