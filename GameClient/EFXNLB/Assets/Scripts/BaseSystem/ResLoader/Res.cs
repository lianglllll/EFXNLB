using BaseSystem.Singleton;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Res
{
    // ���ͷ����������κ����͵���Դ
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
