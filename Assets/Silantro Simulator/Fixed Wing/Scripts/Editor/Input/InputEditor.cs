using UnityEngine;
using UnityEditor;
using System.IO;
using Oyedoyin;

public class InputEditor : EditorWindow
{
    [MenuItem("Oyedoyin/Fixed Wing/Miscellaneous/Setup Input", false, 4900)]
    public static void InitializeQuick()
    {
        try
        {
            Handler.ControlAxis(new Handler.Axis() { name = "---------------- Buttons", positiveButton = "-", gravity = 0f, sensitivity = 0f, type = 0, descriptiveName = "Key 01" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Start Engine", positiveButton = "f1", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 02" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Start Engine BandL", positiveButton = "f3", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 03" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Start Engine BandR", positiveButton = "f5", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 04" }, false);

            Handler.ControlAxis(new Handler.Axis() { name = "Stop Engine", positiveButton = "f2", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 05" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Stop Engine BandL", positiveButton = "f4", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 06" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Stop Engine BandR", positiveButton = "f6", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 07" }, false);

            Handler.ControlAxis(new Handler.Axis() { name = "Parking Brake", positiveButton = "x", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 08" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Brake Lever", positiveButton = "space", gravity = 0.0f, sensitivity = 0.6f, type = 0, descriptiveName = "Key 09" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Actuate Gear", positiveButton = "0", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 10" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "LightSwitch", positiveButton = "v", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 11" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Fire", positiveButton = "left ctrl", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 12" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Target Up", positiveButton = "m", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 13" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Target Down", positiveButton = "n", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 14" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Target Lock", positiveButton = "numlock", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 15" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Propeller Engage", positiveButton = "y", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 16" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Weapon Select", positiveButton = "q", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 17" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Weapon Release", positiveButton = "z", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 18" }, false);

            Handler.ControlAxis(new Handler.Axis() { name = "DropSwitch", positiveButton = "backspace", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 19" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Speed Brake", positiveButton = "o", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 20" }, false);

            Handler.ControlAxis(new Handler.Axis() { name = "Afterburner", positiveButton = "f12", altPositiveButton = "joystick button 3", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 21" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Reverse Thrust", positiveButton = "f11", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 22" }, false);

            Handler.ControlAxis(new Handler.Axis() { name = "Fuel Dump", positiveButton = "6", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 23" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Refuel", positiveButton = "5", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 24" }, false);

            Handler.ControlAxis(new Handler.Axis() { name = "Transition To VTOL", positiveButton = "7", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 25" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Transition To STOL", positiveButton = "8", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 26" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Transition To Normal", positiveButton = "9", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 27" }, false);

            Handler.ControlAxis(new Handler.Axis() { name = "Extend Flap", positiveButton = "3", altPositiveButton = "joystick button 4", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 28" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Retract Flap", positiveButton = "4", altPositiveButton = "joystick button 5", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 29" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Actuate Slat", positiveButton = "k", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 30" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Spoiler", positiveButton = "g", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 31" }, false);


            Handler.ControlAxis(new Handler.Axis() { name = "---------------- Keyboard Axes", positiveButton = "-", gravity = 0f, sensitivity = 0f, type = 0, descriptiveName = "Key 32" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Throttle", positiveButton = "1", negativeButton = "2", gravity = 0.0f, sensitivity = 0.4f, type = 0, dead = 0.001f, descriptiveName = "Key 33" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Roll", positiveButton = "d", negativeButton = "a", altPositiveButton = "right", altNegativeButton = "left", gravity = 0.2f, sensitivity = 0.65f, type = 0, dead = 0.001f, descriptiveName = "Key 34" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Pitch", positiveButton = "w", negativeButton = "s", altPositiveButton = "up", altNegativeButton = "down", gravity = 0.3f, sensitivity = 0.7f, type = 0, dead = 0.001f, descriptiveName = "Key 35" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Rudder", positiveButton = "q", negativeButton = "e", gravity = 1f, sensitivity = 0.9f, type = 0, dead = 0.001f, descriptiveName = "Key 36" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Collective", positiveButton = "right alt", negativeButton = "left alt", gravity = 0.0f, sensitivity = 0.25f, type = 0, dead = 0.001f, descriptiveName = "Key 37" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Propeller", positiveButton = "]", negativeButton = "[", gravity = 0.0f, sensitivity = 0.35f, type = 0, dead = 0.001f, descriptiveName = "Key 38" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "---------------- Joystick Axes", positiveButton = "-", gravity = 0f, sensitivity = 0f, type = 0, descriptiveName = "Key 39" }, false);
            Handler.ControlAxis(new Handler.Axis() { name = "Throttle", positiveButton = " ", negativeButton = " ", gravity = 1.0f, sensitivity = 1.0f, type = 2, axis = 3, dead = 0.001f, invert = true, descriptiveName = "Key 40" }, true);
            Handler.ControlAxis(new Handler.Axis() { name = "Roll", positiveButton = " ", negativeButton = " ", gravity = 1.0f, sensitivity = 1.0f, type = 2, axis = 0, dead = 0.001f, descriptiveName = "Key 41" }, true);
            Handler.ControlAxis(new Handler.Axis() { name = "Pitch", positiveButton = " ", negativeButton = " ", gravity = 1.0f, sensitivity = 1.0f, type = 2, axis = 1, dead = 0.001f, descriptiveName = "Key 42" }, true);
            Handler.ControlAxis(new Handler.Axis() { name = "Rudder", positiveButton = " ", negativeButton = " ", gravity = 1.0f, sensitivity = 1.0f, type = 2, axis = 2, dead = 0.001f, descriptiveName = "Key 43" }, true);
            Handler.ControlAxis(new Handler.Axis() { name = "Collective", positiveButton = " ", negativeButton = " ", gravity = 1.0f, sensitivity = 1.0f, type = 2, axis = 4, dead = 0.001f, descriptiveName = "Key 44" }, true);
            Handler.ControlAxis(new Handler.Axis() { name = "Propeller", positiveButton = " ", negativeButton = " ", gravity = 1.0f, sensitivity = 1.0f, type = 2, axis = 5, dead = 0.001f, descriptiveName = "Key 45" }, true);

            Debug.Log("Input Setup Successful!");
        }
        catch
        {
            Debug.LogError("Failed to apply input manager bindings.");
        }
    }
}
