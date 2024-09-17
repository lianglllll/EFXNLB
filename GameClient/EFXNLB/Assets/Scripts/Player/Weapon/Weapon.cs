using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public abstract void GunFire();
    public abstract void Reload();

    /// 动画里面调用。
    public abstract void AimIn();
    public abstract void AimOut();

    //控制准许开合大小
    public abstract void ExpaningCrossUpdate(float expandDegree);
    
    //不同的换弹动画
    public abstract void ReloadBegin();

    public abstract void Init();

    public abstract void Close();
}
