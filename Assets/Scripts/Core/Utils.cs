using System.IO;
using UnityEngine;

public static class Utils
{
    public static T GetOrAddComponent<T>(GameObject obj) where T : Component
    {
        T toReturn;
        if (!obj.GetComponent<T>())
        {
            toReturn = obj.AddComponent<T>();
        }
        else
        {
            toReturn = obj.GetComponent<T>();
        }
        return toReturn;
    }
    public static Vector3 GetMousePos()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        return mousePos;
    }
    public static Sprite Load(string imagePath, string spriteName)
    {
        Sprite[] all = Resources.LoadAll<Sprite>(imagePath);

        foreach (var s in all)
        {
            if (s.name == spriteName)
            {
                return s;
            }
        }
        return null;
    }

    public static Vector3[] GetObjectWorldCorners(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        return corners;
    }

    public static Vector3[] GetObjectLocalCorners(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetLocalCorners(corners);
        return corners;
    }

    public static string Get8CharacterRandomString()
    {
        string path = Path.GetRandomFileName();
        return path[..8];
    }
}
