using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ButtonPanel : MonoBehaviour
{
    public GameObject menu;

    private ButtonPanel[] buttonPanelChildren;
    private RectTransform rectTransform;
    private RectTransform menuRectTransform;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        menuRectTransform = menu.GetComponent<RectTransform>();
        buttonPanelChildren = GetComponentsInChildren<ButtonPanel>(true); // include inactive

        RectTransform[] allChildrenRectTransforms = GetComponentsInChildren<RectTransform>(true);
        foreach (RectTransform childRectTransform in allChildrenRectTransforms)
        {
            if (childRectTransform != rectTransform)
            {
                BoxCollider2D bc2d = Utils.GetOrAddComponent<BoxCollider2D>(childRectTransform.gameObject);
                bc2d.size = childRectTransform.sizeDelta;
            }
        }
        menu.SetActive(false);
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Transform raycastResultTransform = WhatClickedInsideMenu();
            if (raycastResultTransform != null)
            {
                if (raycastResultTransform == transform)
                {
                    MenuToggle();
                } 
            }
            else
            {
                SetAllMenusInactive();
            }
        }
    }
    private void SetAllMenusInactive()
    {
        foreach (ButtonPanel child in buttonPanelChildren)
        {
            child.menu.SetActive(false);
        }
        menu.SetActive(false);
    }
    public void MenuToggle()
    {
        menu.SetActive(!menu.activeSelf);
    }
    private Transform WhatClickedInsideMenu()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider != null && CheckIfHitMenuOrChild(hits[i].transform))
            {
                return hits[i].transform;
            }
        }

        return null;
    }
    private bool CheckIfHitMenuOrChild(Transform hit)
    {
        if (hit == transform || hit == menuRectTransform.transform) { return true; }

        Transform currentTransform = hit;
        while (currentTransform.parent != null)
        {
            if (currentTransform.parent == menuRectTransform.transform)
            { 
                return true; 
            }
            currentTransform = currentTransform.parent;
        }
        return false;
    }
}
