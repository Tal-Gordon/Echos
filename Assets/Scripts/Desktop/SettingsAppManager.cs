using UnityEngine;
using UnityEngine.UI;

public class SettingsAppManager : MonoBehaviour
{
    public TimeWidget timeWidget;
    public CustomToggle timeSettingToggle;

    private DataManager.DataCategory systemCat;
    async void Start()
    {
        timeSettingToggle = transform.Find("Content").Find("Time format setting").Find("Setting controller").GetChild(0).GetComponent<CustomToggle>();
        timeSettingToggle.onValueChanged.AddListener(async delegate { await TimeFormatToggle(); });
        timeWidget = FindAnyObjectByType<DesktopManager>().transform.Find("Taskbar").GetChild(0).Find("TimeWidget").GetComponent<TimeWidget>();

        systemCat = DataManager.DataCategory.System;
        bool is24HourFormat = await DataManager.ReadDataAsync<bool>(systemCat, "is24HourFormat");

        timeSettingToggle.isOn = !is24HourFormat;
        await TimeFormatToggle();
    }

    void Update()
    {

    }

    public async Awaitable TimeFormatToggle()
    {
        if (timeSettingToggle.isOn)
        {
            timeWidget.Set12HourFormat();
        }
        else
        {
            timeWidget.Set24HourFormat();
        }
        await DataManager.WriteDataAsync(systemCat, "is24HourFormat", !timeSettingToggle.isOn);
    }
}