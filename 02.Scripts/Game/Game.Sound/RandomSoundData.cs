using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "Audio/SoundData")]
public class RandomSoundData : ScriptableObject
{
    [Header("Sound setting")]
    public string soundName;
    public List<AudioClip> clip; // 사운드 소스
    public float volume = 1.0f; // 볼륨
    public bool loop = false; // 반복 여부

}
