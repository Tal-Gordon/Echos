using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePictureManager : MonoBehaviour
{
    public List<Sprite> pictureSprites;
    public GameObject panel;
    public GameObject picturesParent;
    public GameObject colorsParent;
    public GameObject pictureButtonPrefab;
    public List<Button> pictureButtons;
    void Start()
    {
        InstantiatePictures();
    }

    private void InstantiatePictures()
    {
        foreach (Sprite texture in pictureSprites)
        {
            GameObject pic = Instantiate(pictureButtonPrefab, picturesParent.transform);
            Image picImage = pic.GetComponent<Image>();
            picImage.sprite = texture;
            pic.GetComponent<Button>().onClick.AddListener(() => SelectPicture(picImage.sprite));
        }
    }

    public void ShowPanel()
    {
        panel.SetActive(true);
    }

    public void HidePanel()
    {
        panel.SetActive(false);
    }

    public void SelectPicture(Sprite picture)
    {

    }

    public void SelectColor()
    {

    }
}
