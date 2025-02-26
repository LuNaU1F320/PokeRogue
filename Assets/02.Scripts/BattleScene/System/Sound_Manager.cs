using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound_Manager : G_Singleton<Sound_Manager>
{
    [HideInInspector] public AudioSource AudioSrc = null;
    Dictionary<string, AudioClip> AudioClipList = new Dictionary<string, AudioClip>();

    float bgmVolume = 0.2f;
    [HideInInspector] public bool a_SoundOnOff = true;
    [HideInInspector] public float a_SoundVolume = 1.0f;

    //효과음 최적화를 위한 버퍼 변수
    int EffSdCount = 5;
    int SoundCount = 0;    //최대 5개 재생(렉방지)
    GameObject[] SoundObjectList = new GameObject[10];
    AudioSource[] SoundSourceList = new AudioSource[10];
    float[] EffVolume = new float[10];


    protected override void Init()       //Awake 대신 사용
    {
        base.Init();         //부모쪽의 Init함수 호출
        LoadChildGameObj();
    }
    void Start()
    {
        //Sounds

        //사운드 리로스 미리 로딩
        AudioClip GAudioClip = null;
        object[] temp = Resources.LoadAll("Sounds");
        for (int i = 0; i < temp.Length; i++)
        {
            GAudioClip = temp[i] as AudioClip;
            if (AudioClipList.ContainsKey(GAudioClip.name) == true)
            {
                continue;
            }
            AudioClipList.Add(GAudioClip.name, GAudioClip);
        }
    }

    private void LoadChildGameObj()
    {
        AudioSrc = gameObject.AddComponent<AudioSource>();
        //게임 효과음 플레이를 위한 5개의 레이어 생성 코드
        for (int i = 0; i < EffSdCount; i++)
        {
            GameObject newSoundObj = new GameObject();
            newSoundObj.transform.SetParent(transform);
            newSoundObj.transform.localPosition = Vector3.zero;
            AudioSource aAudioSrc = newSoundObj.AddComponent<AudioSource>();
            aAudioSrc.playOnAwake = false;
            aAudioSrc.loop = false;
            newSoundObj.name = "SoundEffObj";

            SoundSourceList[i] = aAudioSrc;
            SoundObjectList[i] = newSoundObj;
        }
        int m_SoundOnOFf = PlayerPrefs.GetInt("SoundOnOff", 1);
        if (m_SoundOnOFf == 1)
        {
            SoundOnoff(true);
        }
        else
        {
            SoundOnoff(false);
        }
        float VolumeValue = PlayerPrefs.GetFloat("SoundVolume", 1.0f);
        SoundVolume(VolumeValue);
    }

    public void SoundVolume(float Volume)
    {
        if (AudioSrc != null)
        {
            AudioSrc.volume = bgmVolume * Volume;
        }
        a_SoundVolume = Volume;
    }

    public void SoundOnoff(bool OnOff = true)
    {
        bool MuteOnOff = !OnOff;

        if (AudioSrc != null)
        {
            AudioSrc.mute = MuteOnOff;
            //if (MuteOnOff == false)
            //{
            //    AudioSrc.time = 0;        //처음부터 플레이
            //}
        }
        for (int i = 0; i < EffSdCount; i++)
        {
            if (SoundSourceList[i] != null)
            {
                SoundSourceList[i].mute = MuteOnOff;
                if (MuteOnOff == false)
                {
                    SoundSourceList[i].time = 0;
                }
            }
        }

    }

    public void PlayBGM(string FileName, float Volume = 0.2f)
    {
        AudioClip GAdudioClip = null;
        if (AudioClipList.ContainsKey(FileName) == true)
        {
            GAdudioClip = AudioClipList[FileName];
        }
        else
        {
            GAdudioClip = Resources.Load("Sounds/" + FileName) as AudioClip;
            AudioClipList.Add(FileName, GAdudioClip);
        }

        if (AudioSrc == null)
            return;

        if (AudioSrc.clip != null && AudioSrc.clip.name == FileName)
            return;

        AudioSrc.clip = GAdudioClip;
        AudioSrc.volume = Volume * a_SoundVolume;
        bgmVolume = Volume;
        AudioSrc.loop = true;
        AudioSrc.Play();
    }

    public void PlayGUISound(string FileName, float volume = 0.2f)
    {
        if (a_SoundOnOff == false)
        {
            return;
        }
        AudioClip GAudioClip = null;

        if (AudioClipList.ContainsKey(FileName) == true)
        {
            GAudioClip = AudioClipList[FileName];
        }
        else
        {
            GAudioClip = Resources.Load("Sounds/" + FileName) as AudioClip;
            AudioClipList.Add(FileName, GAudioClip);
        }

        if (GAudioClip == null) return;

        AudioSrc.PlayOneShot(GAudioClip, volume * a_SoundVolume);
    }

    public void PlayEffSound(string FileName, float Volume = 0.2f)
    {
        if (a_SoundOnOff == false)
        {
            return;
        }
        AudioClip GAudioClip = null;

        if (AudioClipList.ContainsKey(FileName) == true)
        {
            GAudioClip = AudioClipList[FileName];
        }
        else
        {
            GAudioClip = Resources.Load("Sounds/" + FileName) as AudioClip;
            AudioClipList.Add(FileName, GAudioClip);
        }

        if (GAudioClip == null) return;

        if (SoundSourceList[SoundCount] != null)
        {
            SoundSourceList[SoundCount].volume = 1.0f;
            SoundSourceList[SoundCount].PlayOneShot(GAudioClip, Volume * a_SoundVolume);
            EffVolume[SoundCount] = Volume;

            SoundCount++;
            if (EffSdCount <= SoundCount)
            {
                SoundCount = 0;
            }
        }
    }
}
