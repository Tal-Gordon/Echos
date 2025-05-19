using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HoverPanel : MonoBehaviour
{
    public float fadeValue = 0.1f;
    public Color32 color = new(150, 150, 150, 0);
    public Sprite hoverPanelSprite = null;

    private GameObject hoverPanel;
    private BoxCollider2D boxCollider;
    private RectTransform rectTransform;
    private RectTransform hoverPanelRectTransform;
    private Image hoverPanelImage;

    private bool oneTimeMouseExit = false;
    void Awake()
    {
        boxCollider = Utils.GetOrAddComponent<BoxCollider2D>(gameObject);
        rectTransform = GetComponent<RectTransform>();
    }
    void Start()
    {
        SetHoverPanelSize(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);

        hoverPanel = Instantiate(Resources.Load<GameObject>("Prefabs/HoverPanel"), transform.position, quaternion.identity, transform);
        hoverPanel.name = "HoverPanel";
        hoverPanelImage = hoverPanel.GetComponent<Image>();
        hoverPanelImage.sprite = hoverPanelSprite;
        hoverPanelImage.color = color;
        hoverPanelRectTransform = hoverPanel.GetComponent<RectTransform>();
        hoverPanelRectTransform.sizeDelta = rectTransform.sizeDelta;
        //hoverPanel.GetComponent<RectTransform>().anchorMin = new(0, 1);
        //hoverPanel.GetComponent<RectTransform>().anchorMax = new(0, 1);
    }
    private void OnMouseEnter()
    {
        StartCoroutine(FadeAlpha(fadeValue, 0.2f));
        oneTimeMouseExit = true;
    }
    private void OnMouseExit()
    {
        if (oneTimeMouseExit)
        {
            oneTimeMouseExit = false;
            StartCoroutine(FadeAlpha(0f, 0.2f));
        }
    }
    IEnumerator FadeAlpha(float alphaValue, float fadeTime)
    {
        Color color = hoverPanelImage.color;
        float alpha = color.a;
        for (float i = 0f; i < 1f; i += Time.deltaTime / fadeTime)
        {
            Color newColor = new(color.r, color.g, color.b, Mathf.Lerp(alpha, alphaValue, i));
            hoverPanelImage.color = newColor;
            yield return null;
        }
    }
    public void SetHoverPanelSize(float x, float y)
    {
        Vector2 newSize = new(x, y);

        if (hoverPanelRectTransform != null) { hoverPanelRectTransform.sizeDelta = newSize; }
        if (boxCollider != null) { boxCollider.size = newSize; }
        if (hoverPanel != null) { hoverPanel.transform.position = transform.position; }
    }
}
