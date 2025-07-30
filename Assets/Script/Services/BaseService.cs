using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IService
{
    public abstract void Update();
    public abstract void Kill();
}

public abstract class BaseService<T> : IService where T : DataClass
{
    public T data;

    public virtual void Initialize(T data)
    {
        this.data = data;
    }
}
