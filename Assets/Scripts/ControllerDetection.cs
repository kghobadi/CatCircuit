using UnityEngine;
using Rewired;

/// <summary>
/// Detects when controllers are connected/disconnected from the device. 
/// </summary>
public class ControllerDetection : MonoBehaviour 
{
    void Awake() 
    {
        // Subscribe to events
        ReInput.ControllerConnectedEvent += OnControllerConnected;
        ReInput.ControllerDisconnectedEvent += OnControllerDisconnected;
        ReInput.ControllerPreDisconnectEvent += OnControllerPreDisconnect;
        
        AssignControllersAtStart();
    }

    /// <summary>
    /// Assign each Joystick to a Player initially
    /// </summary>
    void AssignControllersAtStart()
    {
        foreach(Joystick j in ReInput.controllers.Joysticks) 
        {
            // Joystick is already assigned
            if(ReInput.controllers.IsJoystickAssigned(j)) 
                continue; 

            // Assign Joystick to first Player that doesn't have any assigned
            AssignJoystickToNextOpenPlayer(j);
        }
    }
    
    /// <summary>
    /// Assigns a given joystick to next open player. 
    /// </summary>
    /// <param name="j"></param>
    void AssignJoystickToNextOpenPlayer(Joystick j) 
    {
        foreach(Player p in ReInput.players.Players) 
        {
            // player already has a joystick
            if(p.controllers.joystickCount > 0) 
                continue; 
            // assign joystick to player
            p.controllers.AddController(j, true);
            Debug.LogFormat("Assigned controller {0} to player {1}", j.name, p.id + 1);
            return;
        }
    }

    // This function will be called when a controller is connected
    // You can get information about the controller that was connected via the args parameter
    void OnControllerConnected(ControllerStatusChangedEventArgs args) 
    {
        Debug.Log("A controller was connected! Name = " + args.name + " Id = " + args.controllerId + " Type = " + args.controllerType);
        
        // skip if this isn't a Joystick
        if(args.controllerType != ControllerType.Joystick) 
            return; 
 
        // Assign Joystick to first Player that doesn't have any assigned
        AssignJoystickToNextOpenPlayer(ReInput.controllers.GetJoystick(args.controllerId));
    }
    
    //Example - How to exclude certain Controller hardware... Use in above method if needed, but replace GUID. 
    // Get the Joystick from ReInput
    //Joystick joystick = ReInput.controllers.GetJoystick(args.controllerId);
    //if(joystick == null) return;

    // Exclude the Apple Siri Remote -- it will be left unassigned
    // The Siri Remote's Hardware Type Guid is bc043dba-df07-4135-929c-5b4398d29579
    // See this for more information on Hardware Type GUID.
    //if(joystick.hardwareTypeGuid == new System.Guid("bc043dba-df07-4135-929c-5b4398d29579")) return;
 
    // Assign Joystick to first Player that doesn't have any assigned
    //ReInput.controllers.AutoAssignJoystick(joystick);

    // This function will be called when a controller is fully disconnected
    // You can get information about the controller that was disconnected via the args parameter
    void OnControllerDisconnected(ControllerStatusChangedEventArgs args) 
    {
        Debug.Log("A controller was disconnected! Name = " + args.name + " Id = " + args.controllerId + " Type = " + args.controllerType);
    }

    // This function will be called when a controller is about to be disconnected
    // You can get information about the controller that is being disconnected via the args parameter
    // You can use this event to save the controller's maps before it's disconnected
    void OnControllerPreDisconnect(ControllerStatusChangedEventArgs args) 
    {
        Debug.Log("A controller is being disconnected! Name = " + args.name + " Id = " + args.controllerId + " Type = " + args.controllerType);
    }

    void OnDestroy() 
    {
        // Unsubscribe from events
        ReInput.ControllerConnectedEvent -= OnControllerConnected;
        ReInput.ControllerDisconnectedEvent -= OnControllerDisconnected;
        ReInput.ControllerPreDisconnectEvent -= OnControllerPreDisconnect;
    }
}