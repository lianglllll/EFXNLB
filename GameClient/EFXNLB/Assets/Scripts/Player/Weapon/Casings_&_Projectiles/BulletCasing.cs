using BaseSystem.MyDelayedTaskScheduler;
using BaseSystem.PoolModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCasing : MonoBehaviour
{
    // �׳�ʱ��������ת��
    public float ejectForce = 5f;
    public float ejectTorque = 10f;
    // �ӵ��ǵĴ��ʱ��
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

        // ���һ�������Ϸ����׳���
        // ����ejectPoint��right�����Ҳࣩ��up�����Ϸ���
        Vector3 forceDirection = transform.right * 0.7f + transform.up * 0.3f;  // �����Һ��ϵı���

        // Ϊ��������ʵ�У��������׳���������΢���һЩ�����
        forceDirection += new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(0f, 0.2f), Random.Range(-0.1f, 0.1f));

        // ʩ���׳���
        myRigidbody.AddForce(forceDirection * ejectForce, ForceMode.Impulse);

        // ���һ���������ת����ģ���ӵ��ǵ���ת
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
