using System;
using System.Linq;
using UnityEngine;

namespace COG.UI.CustomGameObject;

public class PanelUI : ICustomGameObject
{
    private MeetingHud MeetingHud { get; }
    
    public GameObject UI { get; }

    public Transform Container { get; }

    public PanelUI(MeetingHud hud, Vector3 localPosition)
    {
        MeetingHud = hud;
        var phoneUI = Object.FindObjectsOfType<Transform>()
            .FirstOrDefault(x => x.name == "PhoneUI"); 
        Container = Object.Instantiate(phoneUI, MeetingHud.transform)!;
        Container.transform.localPosition = localPosition;
        UI = Container.gameObject;
    }

    public void AddButton(Transform template, Sprite sprite, Vector3 localPosition
    , Vector3 localScale, Action action, Transform maskTemplate)
    {
        var buttonParent = new GameObject().transform;
        buttonParent.SetParent(Container);
        Object.Instantiate(maskTemplate, buttonParent);

        var button = Object.Instantiate(template,
            buttonParent);

        button.gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
        buttonParent.transform.localPosition = localPosition;
        buttonParent.transform.localScale = localScale;
        var passiveButton = button.GetComponent<PassiveButton>();
        passiveButton.OnClick.RemoveAllListeners();
        passiveButton.OnClick.AddListener(action);
        passiveButton.StopAllCoroutines();
        
        passiveButton.OnMouseOver.RemoveAllListeners();
    }
}