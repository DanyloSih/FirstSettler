using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

public class JobsSettingsEditor : MonoBehaviour
{

    const string kLeakOff = "Jobs/Leak Detection/Off";

    const string kLeakOn = "Jobs/Leak Detection/On";

    const string kLeakDetectionFull = "Jobs/Leak Detection/Full Stack Traces (Expensive)";



    [MenuItem(kLeakOff)]

    static void SwitchLeaksOff()

    {

        // In the case where someone enables, disables, then re-enables leak checking, we might miss some frees

        // while disabled. So to avoid spurious leak warnings, just forgive the leaks every time someone disables

        // leak checking through the menu.

        NativeLeakDetection.Mode = NativeLeakDetectionMode.Disabled;

        Debug.LogWarning("Leak detection has been disabled. Leak warnings will not be generated, and all leaks up to now are forgotten.");

    }



    [MenuItem(kLeakOn)]

    static void SwitchLeaksOn()

    {

        NativeLeakDetection.Mode = NativeLeakDetectionMode.Enabled;

        Debug.Log("Leak detection has been enabled. Leak warnings will be generated upon exiting play mode.");

    }



    [MenuItem(kLeakDetectionFull)]

    static void SwitchLeaksFull()

    {

        NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;

        Debug.Log("Leak detection with stack traces has been enabled. Leak warnings will be generated upon exiting play mode.");

    }
}
