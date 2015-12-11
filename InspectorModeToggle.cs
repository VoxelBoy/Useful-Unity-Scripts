using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class InspectorModeToggle
{

    public static BindingFlags fullBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    public static StringComparison ignoreCase = System.StringComparison.CurrentCultureIgnoreCase;

    private enum InspectorModesToUse
    {
        Normal = 0,
        Debug = 1,
        //DebugInternal = 2,
    }

    [MenuItem("Window/Toggle Inspector Mode %&d")]
    public static void ToggleInspectorMode()
    {
        Assembly unityEngineAssembly = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies()).Find(x => x.GetName().ToString().StartsWith("UnityEditor,"));
        if (unityEngineAssembly == null)
        {
            return;
        }

        var inspectorWindowType = GetTypeFromAssembly("UnityEditor.InspectorWindow", unityEngineAssembly, true);
        var inspectorModeType = GetTypeFromAssembly("UnityEditor.InspectorMode", unityEngineAssembly, true);
        if (inspectorWindowType == null || inspectorModeType == null)
        {
            return;
        }

        var allInspectorsFieldInfo = inspectorWindowType.GetField("m_AllInspectors", fullBinding);
        var setModeMethodInfo = inspectorWindowType.GetMethod("SetMode", fullBinding);
        var inspectorModeFieldInfo = inspectorWindowType.GetField("m_InspectorMode", fullBinding);
        if (allInspectorsFieldInfo == null || inspectorModeFieldInfo == null || setModeMethodInfo == null)
        {
            return;
        }
        IList allInspectorsList = (IList)allInspectorsFieldInfo.GetValue(null);

        foreach (object inspectorWindow in allInspectorsList)
        {
            int currentMode = (int)inspectorModeFieldInfo.GetValue(inspectorWindow);
            int newMode = (currentMode + 1) % System.Enum.GetValues(typeof(InspectorModesToUse)).Length;
            setModeMethodInfo.Invoke(inspectorWindow, new object[] { newMode });
        }
    }

    public static Type GetTypeFromAssembly(string typeName, Assembly assembly, bool exactNameMatch = false)
    {
        Type[] types = assembly.GetTypes();
        foreach (Type type in types)
        {
            if (exactNameMatch && type.FullName == typeName)
                return type;

            if (type.Name.Equals(typeName, ignoreCase) || type.Name.Contains('+' + typeName)) //+ checks for inline classes
                return type;
        }
        return null;
    }
}
