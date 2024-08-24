using Game.Tool.Singleton;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// �������еļ�ʱ��
/// </summary>
public class GameTimerManager : Singleton<GameTimerManager>
{
    //note:��ʼ��ʱ������Ҫ����һЩ��ʱ�����������ǵĿ��м�ʱ����һ����ʱ����û��
    //1.��һ�����������������еĿ��м�ʱ��
    //2.��һ�������������浱ǰ���ڹ����ļ�ʱ��
    //3.���µ�ǰ�����еļ�ʱ��
    //4.��ĳ����ʱ��������ɺ�������Ҫ�������յ����м�ʱ��������

    [SerializeField] private int _initMaxTimerCount;

    private Queue<GameTimer> _notWorkerTimer = new Queue<GameTimer>(); 
    private List<GameTimer> _workeringTimer = new List<GameTimer>();

    private void Start()
    {
        InitTimerManager();
    }

    private void Update()
    {
        UpdateWorkeringTimer();
    }

    private void InitTimerManager()
    {
        for(int i = 0; i < _initMaxTimerCount; i++)
        {
            CreateTimer();
        }
    }

    private void CreateTimer()
    {
        var timer = new GameTimer();
        _notWorkerTimer.Enqueue(timer);
    }

    /// <summary>
    /// ʹ��
    /// </summary>
    /// <param name="time"></param>
    /// <param name="task"></param>
    public void TryUseOneTimer(float time,Action task)
    {
        if(_notWorkerTimer.Count == 0)
        {
            CreateTimer();
            var timer = _notWorkerTimer.Dequeue();
            timer.StartTimer(time, task);
            _workeringTimer.Add(timer);
        }
        else
        {
            var timer = _notWorkerTimer.Dequeue();
            timer.StartTimer(time, task);
            _workeringTimer.Add(timer);
        }
    }

    private void UpdateWorkeringTimer()
    {
        if (_workeringTimer.Count == 0) return;
        for(int i = 0; i < _workeringTimer.Count; i++)
        {
            if(_workeringTimer[i].GetTimerState() == TimerState.WORKERING)
            {
                _workeringTimer[i].UpdateTimer();
            }
            else
            {
                //���������
                _notWorkerTimer.Enqueue(_workeringTimer[i]);
                _workeringTimer[i].ResetTimer();
                _workeringTimer.Remove(_workeringTimer[i]);
            }
        }
    }

}
