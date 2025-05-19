using System.Collections.Generic;
using UnityEngine;

public class WizardPageNavigator
{
    private readonly List<GameObject> pages;
    private int currentPageIndex = 0;

    public int CurrentPageIndex 
    { 
        get => currentPageIndex; 
        set => currentPageIndex = value; 
    }

    public WizardPageNavigator(List<GameObject> pages)
    {
        this.pages = pages;
    }

    public void Initialize()
    {
        foreach (GameObject page in pages)
        {
            page.SetActive(false);
        }
        pages[0].SetActive(true);
    }

    public int GetCurrentPageIndex()
    {
        return CurrentPageIndex;
    }

    public void PageByIndex(int index)
    {
        if (index < pages.Count)
        {
            pages[CurrentPageIndex].SetActive(false);
            CurrentPageIndex = index;
            pages[CurrentPageIndex].SetActive(true);
        }
        else
        {
            Debug.LogError($"PageNavigator: Invalid page index: {index}");
        }
    }

    public void NextPage()
    {
        if (CurrentPageIndex < pages.Count - 1)
        {
            PageByIndex(CurrentPageIndex + 1);
        }
    }

    public void PreviousPage()
    {
        if (CurrentPageIndex > 0)
        {
            PageByIndex(CurrentPageIndex - 1);
        }
    }
}