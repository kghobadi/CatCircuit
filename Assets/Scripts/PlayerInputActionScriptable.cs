using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Optional: Adds a menu item to easily create instances in the Unity Editor
[CreateAssetMenu(fileName = "PlayerInputActionScriptable", menuName = "MyGame/PlayerActionsData")]
public class PlayerInputActionScriptable : ScriptableObject
{
    public PlayerType playerInputType;
    public string HorizontalInput = "Horizontal";
    public string VerticalInput = "Vertical";
    public KeyCode Meow = KeyCode.Z;
    public KeyCode Purr = KeyCode.X;
    public KeyCode Hiss = KeyCode.C;
    public KeyCode Scratch = KeyCode.V;
}