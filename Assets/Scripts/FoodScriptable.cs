using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains food data. 
/// Optional: Adds a menu item to easily create instances in the Unity Editor
/// </summary>
[CreateAssetMenu(fileName = "FoodScriptable", menuName = "MyGame/FoodData")]
public class FoodScriptable : ScriptableObject
{
    public int pointsValue;
    public AudioClip foodSound;
    public Sprite foodSprite;
}