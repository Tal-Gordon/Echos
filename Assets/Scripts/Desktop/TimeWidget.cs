using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class TimeWidget : MonoBehaviour
{
    private bool is24HourFormat;

    private TextMeshProUGUI textMeshPro;
    private DataManager.DataCategory systemCat;
    async void Start()
    {
        textMeshPro = transform.Find("Time").GetComponent<TextMeshProUGUI>();
        systemCat = DataManager.DataCategory.System;
        is24HourFormat = await DataManager.ReadDataAsync<bool>(systemCat, "is24HourFormat");
    }
    void Update()
    {
        textMeshPro.text = is24HourFormat 
            ? $"{DateTime.Now:HH:mm}\n{DateTime.Now:dd/MM/yyyy}"  // 24-hour format
            : $"{DateTime.Now:h:mm tt}\n{DateTime.Now:dd/MM/yyyy}"; // 12-hour format
    }
    public void Set24HourFormat()
    {
        is24HourFormat = true;
    }
    public void Set12HourFormat()
    {
        is24HourFormat = false;
    }
}
