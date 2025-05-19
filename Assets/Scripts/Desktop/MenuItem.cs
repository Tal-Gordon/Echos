using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class MenuItem
{
    public string Title { get; set; }
    public virtual void OnClick() { Debug.LogError("Accessed MenuItem base OnClick method"); }
}
// A simple button that performs an action when clicked
public class ActionMenuItem : MenuItem
{
    private readonly UnityEngine.Events.UnityAction action;

    public ActionMenuItem(string title, UnityEngine.Events.UnityAction action)
    {
        Title = title;
        this.action = action;
    }

    public override void OnClick()
    {
        action?.Invoke();
    }
}
// A button that switches between two states (on/off, enabled/disabled)
public class ToggleMenuItem : MenuItem // todo fix not saving state - data gets reserialized every right click, initialState unchanged
{
    private readonly UnityEngine.Events.UnityAction action;
    public bool isOn;

    public ToggleMenuItem(string title, UnityEngine.Events.UnityAction action, bool initialState)
    {
        Title = title;
        this.action = action;
        isOn = initialState;
    }

    public bool IsOn => isOn;

    public override void OnClick()
    {
        action?.Invoke();
        isOn = !isOn;
        // Perform additional action if needed, such as visual update
        // Debug.Log($"{Title} is now {(isOn ? "On" : "Off")}");
    }
}
// A button that, when hovered over, opens up a submenu with more options
public class SubmenuMenuItem : MenuItem, IMenuProvider
{
    public List<MenuItem> SubmenuItems { get; private set; }
    public bool isSubmenuVisible = false;

    public SubmenuMenuItem(string title, List<MenuItem> submenuItems)
    {
        Title = title;
        SubmenuItems = submenuItems;
    }

    public override void OnClick() { }

    public List<MenuItem> GetMenuItems()
    {
        return SubmenuItems;
    }
}
