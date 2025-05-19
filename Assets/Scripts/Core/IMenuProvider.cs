using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public interface IMenuProvider
{
    List<MenuItem> GetMenuItems();
}
