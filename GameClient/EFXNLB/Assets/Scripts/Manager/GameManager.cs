using BaseSystem.PoolModule;
using BaseSystem.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{

    private void Start()
    {
        UnityObjectPoolFactory.Instance.LoadFuncDelegate = Res.LoadAssetSync<UnityEngine.Object>;
    }

    void Update()
    {
        Kaiyun.Event.Tick();
    }
}
