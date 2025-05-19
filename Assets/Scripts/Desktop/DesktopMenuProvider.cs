using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DesktopMenuProvider : MonoBehaviour, IMenuProvider
{
    private SubmenuMenuItem viewSubmenu;
    private SubmenuMenuItem newSubmenu;
    private DesktopManager desktopManager;

    private bool arrangeIcons;
    private bool alignIcons;
    private bool toggleIcons;
    private async void Start()
    {
        desktopManager = FindAnyObjectByType<DesktopManager>();
        await LoadSettings();

        viewSubmenu = new("View", new List<MenuItem> 
        { 
            new ToggleMenuItem("Auto arrange icons", ArrangeIcons, arrangeIcons),
            new ToggleMenuItem("Align icons to grid", AlignIcons, alignIcons),
            new ToggleMenuItem("Show desktop icons", ToggleIcons, toggleIcons),
        });
        newSubmenu = new("New", new List<MenuItem> 
        { 
            new ActionMenuItem("Text file", NewTextFile) 
        }); // TODO

    }
    public List<MenuItem> GetMenuItems()
    {
        return new List<MenuItem>
        {
            viewSubmenu,
            newSubmenu,
            new ActionMenuItem("Desktop settings", OpenSettings)
        };
    }
    public void ArrangeIcons()
    {
        desktopManager.ArrangeIcons();
    }
    public void AlignIcons()
    {
        desktopManager.AlignIcons();
    }
    public void ToggleIcons()
    {
        desktopManager.ToggleIcons();
    }
    public void OpenSettings()
    {
        desktopManager.OpenSettings();
    }
    
    public void NewTextFile()
    {
        desktopManager.NewTextFile();
    }

    private async Awaitable LoadSettings()
    {
        DataManager.DataCategory systemCat = DataManager.DataCategory.System;
        arrangeIcons = await DataManager.ReadDataAsync<bool>(systemCat, "IconsArranged");
        alignIcons = await DataManager.ReadDataAsync<bool>(systemCat, "IconsAligned");
        toggleIcons = await DataManager.ReadDataAsync<bool>(systemCat, "IconsShown");
    }
}
