using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COG.UI.CustomGameObject;

public class PhoneUI : ICustomGameObject
{
    private readonly MeetingHud _meetingHud; 
    private readonly Transform _phoneUI;
    private readonly Transform _container;

    private List<Transform> Children { get; } = new();

    public Transform[] GetChildren()
    {
        return Children.ToArray();
    }
    
    public PhoneUI(MeetingHud hud)
    {
        _meetingHud = hud;
        _phoneUI = Object.FindObjectsOfType<Transform>().FirstOrDefault(x => x.name == "PhoneUI")!;
        _container = Object.Instantiate(_phoneUI, _meetingHud.transform)!;
        _container.transform.localPosition = new Vector3(0, 0, -5f);
        _container.transform.localScale *= 0.75f;
    }

    public void AddChild(Transform child)
    {
        child.transform.SetParent(_container);
        Children.Add(child);
    }

    public void Destroy()
    {
        Object.Destroy(_container.gameObject);
    }
}