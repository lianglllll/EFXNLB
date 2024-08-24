using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public abstract void GunFire();
    public abstract void Reload();
    public abstract void AimIn();
    public abstract void AimOut();

    //控制准许开合大小
    public abstract void ExpaningCrossUpdate(float expandDegree);
    
    //动画相关
    public abstract void DoReloadAnimation();


}
