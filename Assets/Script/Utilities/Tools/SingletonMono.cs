using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
    private static object _instance = new object();
    private static T instance;
    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                lock (_instance)
                {
                    if (instance == null) 
                    {
                        instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);
                    }
                }
            }

            return instance;
        }
    }

    private bool isFirstFrameInUpdate = false;

    public virtual void LateStart() { }

    public virtual void Update()
    {
        if (!isFirstFrameInUpdate)
        {
            isFirstFrameInUpdate = true;
            LateStart();
        }
    }
}
