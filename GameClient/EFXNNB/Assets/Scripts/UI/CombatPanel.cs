using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatPanel : MonoBehaviour
{
    private List<Image> crossQuarterImgs = new List<Image>();
    private Image dot;

    private float curCrossExpandDegree;                   //当前准星开合度
    private float perFrameCrossExpandDegree = 5f;         //每帧准星开合度
    private bool isExpandCross;
    private float targetCrossExpandDegree;

    private Text AmmoTextUI;
    private Text ShootModeTextUI;

    private void Awake()
    {
        Transform Crosshair = transform.Find("Crosshair/Holder");
        for (int i = 0; i < Crosshair.childCount; ++i)
        {
            crossQuarterImgs.Add(Crosshair.GetChild(i).GetComponent<Image>());
        }
        dot = transform.Find("Crosshair/NoHolder/dot").GetComponent<Image>();

        AmmoTextUI = transform.Find("AmmoTextUI").GetComponent<Text>();
        ShootModeTextUI = transform.Find("ShootModeTextUI").GetComponent<Text>();
    }

    private void Start()
    {
        isExpandCross = false;
    }

    private void Update()
    {
        if (isExpandCross)
        {
            Debug.Log("debug:" + curCrossExpandDegree);
            if (Mathf.Abs(targetCrossExpandDegree - curCrossExpandDegree) <= 0.01f)
            {
                curCrossExpandDegree = targetCrossExpandDegree;
                isExpandCross = false;
            }else  if (curCrossExpandDegree < targetCrossExpandDegree)
            {
                ExpandCross(perFrameCrossExpandDegree);
            }
            else
            {
                ExpandCross(-perFrameCrossExpandDegree);
            }


        }
    }

    public void AmmoTextUIUpdate(int curBulletNum, int reserveBulletNum)
    {
        AmmoTextUI.text = curBulletNum + "/" + reserveBulletNum;
    }

    public void ShootModeTextUIUpdate(int flag)
    {
        ShootModeTextUI.text = flag == 0 ? "全自动" : "半自动";
    }

    private void CrossColorChange(Color color)
    {
        foreach (var item in crossQuarterImgs) {
            item.color = color;
        }
        dot.color = color;
    }

    //平滑改变准星开合度
    public void BeginExpandCross(float targetDegree)
    {
        targetCrossExpandDegree = targetDegree;
        isExpandCross = true;
    }

    public void ShootExpandCross(float initDegree)
    {
        targetCrossExpandDegree += perFrameCrossExpandDegree * 3;
        isExpandCross = true;

        GameTimerManager.Instance.TryUseOneTimer(0.1f, () =>
        {
            targetCrossExpandDegree -= perFrameCrossExpandDegree * 3;
            if(targetCrossExpandDegree < initDegree)
            {
                targetCrossExpandDegree = initDegree;
            }
            isExpandCross = true;
        });
    }

    /// <summary>
    /// 改变准星开合度
    /// </summary>
    public void ExpandCross(float add)
    {
        curCrossExpandDegree += add;
        crossQuarterImgs[0].transform.localPosition += new Vector3(-add, 0, 0); //左
        crossQuarterImgs[1].transform.localPosition += new Vector3(add, 0, 0);  //右
        crossQuarterImgs[2].transform.localPosition += new Vector3(0, add, 0); //上
        crossQuarterImgs[3].transform.localPosition += new Vector3(0, -add, 0);  //下
    }



}
