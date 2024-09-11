using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 音频管理器，存储所有音频并且可以播放和停止
/// </summary>
public class AudioMananger : MonoBehaviour
{

    /// <summary>
    /// 存储单个音频的信息
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
