using BaseSystem.MyDelayedTaskScheduler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    //武器库
    public List<GameObject> weapons = new();
    private int curWeaponID;


    private void Start()
    {
        foreach (var item in weapons)
        {
            item.SetActive(false);
        }
        curWeaponID = -1;
        //初始化第一把枪
        ChangeCurrentWeapon(0);
    }

    private void Update()
    {
        ChangeCurrentWeaponId();
    }

    public void ChangeCurrentWeaponId()
    {
        if(Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            ChangeCurrentWeapon(curWeaponID + 1);
        }
        else if(Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            ChangeCurrentWeapon(curWeaponID - 1);
        }
    }
    public void ChangeCurrentWeapon(int weaponId)
    {
        int weaponsCount = weapons.Count;
        if (weaponsCount == 0) return;
        weaponId = (weaponId + weaponsCount) % weaponsCount;
        if(curWeaponID != -1)
        {
            weapons[curWeaponID].SetActive(false);
            weapons[curWeaponID].GetComponent<Weapon>().Close();

        }
        curWeaponID = weaponId;
        weapons[curWeaponID].SetActive(true);
        weapons[curWeaponID].GetComponent<Weapon>().Init();

    }

}
