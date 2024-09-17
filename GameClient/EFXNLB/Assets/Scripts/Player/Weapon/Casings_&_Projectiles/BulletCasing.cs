using BaseSystem.MyDelayedTaskScheduler;
using BaseSystem.PoolModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCasing : MonoBehaviour
{
    // 抛出时的力和旋转力
    public float ejectForce = 5f;
    public float ejectTorque = 10f;
    // 子弹壳的存活时间
    public float shellLifeTime = 3f;
    //rigdbody
    private Rigidbody myRigidbody;
    //audio
    public AudioClip[] casingSounds;
    public AudioSource audioSource;

    private void Awake()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    private string RecycleItemId;
    public void Init(string RecycleItemId)
    {
        this.RecycleItemId = RecycleItemId;

        // 添加一个向右上方的抛出力
        // 基于ejectPoint的right方向（右侧）和up方向（上方）
        Vector3 forceDirection = transform.right * 0.7f + transform.up * 0.3f;  // 调整右和上的比例

        // 为了增加真实感，可以在抛出的力上稍微添加一些随机性
        forceDirection += new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(0f, 0.2f), Random.Range(-0.1f, 0.1f));

        // 施加抛出力
        myRigidbody.AddForce(forceDirection * ejectForce, ForceMode.Impulse);

        // 添加一个随机的旋转力，模拟子弹壳的旋转
        Vector3 randomTorque = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * ejectTorque;
        myRigidbody.AddTorque(randomTorque, ForceMode.Impulse);

        //Start the remove/destroy coroutine
        DelayedTaskScheduler.Instance.AddDelayedTask(shellLifeTime, () => {
            RemoveCasing();
        });

        //Start play sound coroutine
        DelayedTaskScheduler.Instance.AddDelayedTask(Random.Range(0.25f, 0.85f), () => {
            PlaySound();
        });
    }

    private void PlaySound()
    {
        audioSource.clip = casingSounds[Random.Range(0, casingSounds.Length)];
        audioSource.Play();
    }

    private void RemoveCasing()
    {
        UnityObjectPoolFactory.Instance.RecycleItem(RecycleItemId, gameObject);
        gameObject.SetActive(false);
    }

}
