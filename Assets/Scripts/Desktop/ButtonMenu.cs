using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonMenu : MonoBehaviour
{
    [SerializeField] Button[] buttons;
    private GameObject buttonsPanel;
    void Awake()
    {
        buttonsPanel = transform.Find("Buttons").gameObject;
        buttons = new Button[buttonsPanel.transform.childCount];
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i] = buttonsPanel.transform.GetChild(i).gameObject.GetComponent<Button>();
        }
    }
    void OnMouseEnter()
    {
        StartCoroutine(ToggleButtonMenu(true));
    }
    private void OnMouseExit()
    {
        StartCoroutine(ToggleButtonMenu(false));
    }
    private IEnumerator ToggleButtonMenu(bool active)
    {
        yield return new WaitForSeconds(0.2f);
        buttonsPanel.SetActive(active);
    }
}
