using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    //������
    public List<GameObject> weapons = new();
    public int curWeaponID;

    private void Start()
    {
        curWeaponID = -1;
    }

    private void Update()
    {
        ChangeCurrentWeapon();
    }

    public void ChangeCurrentWeapon()
    {
        Debug.Log(Input.GetAxis("Mouse ScrollWheel"));
    }

}
