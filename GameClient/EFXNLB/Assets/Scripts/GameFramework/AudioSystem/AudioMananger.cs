using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// ��Ƶ���������洢������Ƶ���ҿ��Բ��ź�ֹͣ
/// </summary>
public class AudioMananger : MonoBehaviour
{

    /// <summary>
    /// �洢������Ƶ����Ϣ
    /// </summary>
    public class Sound
    {
        public AudioClip clip;
        public AudioMixerGroup outputGroup;
        public float volume;
        public bool playOnAwake;
        public bool loop;
    }

}
