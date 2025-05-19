using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
[Serializable]
public class ToggleEvent : UnityEvent<bool> { }
public class CustomToggle : MonoBehaviour, IPointerClickHandler
{
    public GameObject toggleOn;
    public GameObject toggleOff;
    public ToggleEvent onValueChanged;
    public bool isOn;

    private void Start()
    {
        SetGraphic();
    }

    public void Toggle(bool value)
    {
        isOn = value;
        SetGraphic();

        onValueChanged?.Invoke(isOn);
    }
    public void Toggle()
    {
        Toggle(!isOn);
    }
    private void SetGraphic()
    {
        toggleOn.SetActive(isOn);
        toggleOff.SetActive(!isOn);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        Toggle();
    }
}
