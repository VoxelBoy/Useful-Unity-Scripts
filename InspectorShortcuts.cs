using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class InspectorShortcuts
{

    public static BindingFlags fullBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    public static StringComparison ignoreCase = System.StringComparison.CurrentCultureIgnoreCase;

    private enum InspectorModesToUse
    {
        Normal = 0,
        Debug = 1,
        //DebugInternal = 2,
    }

	static Assembly unityEngineAssembly;
	static Type inspectorWindowType;
	static Type inspectorModeType;
	static FieldInfo allInspectorsFieldInfo;
	static FieldInfo inspectorModeFieldInfo;
	static MethodInfo setModeMethodInfo;
	static MethodInfo flipLockedMethodInfo;
	static MethodInfo repaintAllInspectorsMethodInfo;

	static bool Init()
	{
		unityEngineAssembly = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies()).Find(x => x.GetName().ToString().StartsWith("UnityEditor,"));
		if (unityEngineAssembly == null)
		{
			return false;
		}

		inspectorWindowType = GetTypeFromAssembly("UnityEditor.InspectorWindow", unityEngineAssembly, true);
		inspectorModeType = GetTypeFromAssembly("UnityEditor.InspectorMode", unityEngineAssembly, true);
		if (inspectorWindowType == null || inspectorModeType == null)
		{
			return false;
		}

		allInspectorsFieldInfo = inspectorWindowType.GetField("m_AllInspectors", fullBinding);
		inspectorModeFieldInfo = inspectorWindowType.GetField("m_InspectorMode", fullBinding);
		setModeMethodInfo = inspectorWindowType.GetMethod("SetMode", fullBinding);
		flipLockedMethodInfo = inspectorWindowType.GetMethod("FlipLocked", fullBinding);
		repaintAllInspectorsMethodInfo = inspectorWindowType.GetMethod("RepaintAllInspectors", fullBinding);
		if (allInspectorsFieldInfo == null || inspectorModeFieldInfo == null || setModeMethodInfo == null || flipLockedMethodInfo == null || repaintAllInspectorsMethodInfo == null)
		{
			return false;
		}

		return true;
	}

    [MenuItem("Window/Toggle Inspector Mode #&d")]
    public static void ToggleInspectorMode()
    {
		bool init = Init();
		if(!init)
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

	[MenuItem("Window/Toggle Inspector Lock #&l")]
	public static void ToggleInspectorLock()
	{
		bool init = Init();
		if(!init)
		{
			return;
		}

		IList allInspectorsList = (IList)allInspectorsFieldInfo.GetValue(null);

		foreach (object inspectorWindow in allInspectorsList)
		{
			flipLockedMethodInfo.Invoke(inspectorWindow, null);
		}
		repaintAllInspectorsMethodInfo.Invoke(null, null);
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
