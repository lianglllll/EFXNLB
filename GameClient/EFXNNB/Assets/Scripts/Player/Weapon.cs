using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public abstract void GunFire();
    public abstract void Reload();
    public abstract void AimIn();
    public abstract void AimOut();

    //����׼���ϴ�С
    public abstract void ExpaningCrossUpdate(float expandDegree);
    
    //�������
    public abstract void DoReloadAnimation();


}
