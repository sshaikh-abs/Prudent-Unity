#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#endif
using UnityEngine;

public class CommuncationFunctionsBinder : MonoBehaviour
{
#if UNITY_EDITOR
    public List<CommunicationKeyboardEvent> keyboardBinds = new List<CommunicationKeyboardEvent>();

    public List<string> functionListReference;

    private CommunicationManager _communicationManager;
    public CommunicationManager CommunicationManager
    {
        get
        {
            if (_communicationManager == null)
            {
                _communicationManager = GetComponent<CommunicationManager>();
            }
            return _communicationManager;
        }
    }

    private void Update()
    {
        if (CommunicationManager == null) return;
        foreach (var keyboardEvent in keyboardBinds)
        {
            if (Input.GetKeyDown(keyboardEvent.keyCode))
            {
                CallFunctionByName(CommunicationManager, keyboardEvent.functionName, keyboardEvent.parameter);
            }
        }
    }

    [ContextMenu(nameof(ValidateKeyboardBinds))]
    public void ValidateKeyboardBinds()
    {
        List<CommunicationKeyboardEvent> identifiedBinds = new List<CommunicationKeyboardEvent>();
        foreach (var item in keyboardBinds)
        {
            if(item.keyCode != KeyCode.None)
            {
                if (identifiedBinds.Exists(x => x.keyCode == item.keyCode))
                {
                    Debug.LogWarning($"Key {item.keyCode} is already bound to {identifiedBinds.Where(x => x.keyCode == item.keyCode).FirstOrDefault().functionName} but trying to add to {item.functionName}");
                }
                else
                {
                    identifiedBinds.Add(item);
                }
            }
        }

        foreach (var item in identifiedBinds)
        {
            Debug.Log(item.keyCode + " is bound to " + item.functionName);
        }
    }

    [ContextMenu(nameof(RefreshFunctionList))]
    public void RefreshFunctionList()
    {
        functionListReference = GetExternMethodNames(CommunicationManager);
        functionListReference.Sort();
    }

    private void CallFunctionByName(MonoBehaviour targetScript, string methodName, string param = null)
    {
        if (targetScript == null) return;

        MethodInfo method = targetScript.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (method != null)
        {
            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.Length == 0)
            {
                method.Invoke(targetScript, null);
            }
            else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
            {
                method.Invoke(targetScript, new object[] { param });
            }
            else
            {
                Debug.LogWarning($"Method {methodName} found, but parameter signature is unsupported.");
            }
        }
        else
        {
            Debug.LogWarning($"Method {methodName} not found on {targetScript.GetType().Name}");
        }
    }

    private List<string> GetExternMethodNames(MonoBehaviour targetScript)
    {
        List<string> externMethods = new List<string>();

        if (targetScript == null) return externMethods;

        MethodInfo[] methods = targetScript.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (MethodInfo method in methods)
        {
            if (method.Name.EndsWith("_Extern"))
            {
                externMethods.Add(method.Name);
            }
        }

        return externMethods;
    }
#endif
}

#if UNITY_EDITOR
[Serializable]
public class CommunicationKeyboardEvent
{
    public string functionName;
    public KeyCode keyCode;
    public string parameter;
}
#endif