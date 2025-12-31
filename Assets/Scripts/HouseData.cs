using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains house data. 
/// Optional: Adds a menu item to easily create instances in the Unity Editor
/// </summary>
[CreateAssetMenu(fileName = "HouseData", menuName = "MyGame/HouseData")]
public class HouseData : ScriptableObject
{
    [Tooltip("This will modify the visual of the house prefab.")]
    public Sprite homeSprite;
    public Vector3 zoneScale = Vector3.zero;
    public Vector3 zonePos = Vector3.zero;
}