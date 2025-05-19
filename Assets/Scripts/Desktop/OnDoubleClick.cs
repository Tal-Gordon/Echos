using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static Unity.Collections.AllocatorManager;

public class OnDoubleClick : MonoBehaviour
{
    public UnityEvent onSingleClick;
    public UnityEvent onDoubleClick;

    private readonly float ClickDeltaTime = 0.3f;
    private bool click = false;
    private float clickTime;

    private void Update()
    {
        if (click && Time.time > (clickTime + ClickDeltaTime))
        {
            //Single clicked
            click = false;
            onSingleClick?.Invoke();
        }
    }

    private void OnMouseUp()
    {
        if (click && Time.time <= (clickTime + ClickDeltaTime))
        {
            //Double clicked
            click = false;
            onDoubleClick?.Invoke();
        }
        else
        {
            click = true;
            clickTime = Time.time;
        }
    }
}
