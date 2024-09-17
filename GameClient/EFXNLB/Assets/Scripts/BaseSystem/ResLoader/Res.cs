using BaseSystem.Singleton;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Res
{
    // 泛型方法，加载任何类型的资源
    public static T LoadAssetSync<T>(string path) where T : UnityEngine.Object
    {
        T resource = Resources.Load<T>(path);
        if (resource == null)
        {
            Debug.Log($"Resource of type {typeof(T)} not found at {path}!");
        }
        return resource;
    }
}
