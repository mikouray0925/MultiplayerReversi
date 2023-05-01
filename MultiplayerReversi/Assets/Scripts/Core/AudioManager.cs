using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance {get; private set;}

    [Range (0f, 1f)]
    [SerializeField] float volumeEffect = 1f;
    public AudioSource musicPlayer;
    public AudioSource audioSourceUI;

    void Awake() {
        instance = this;
    }

    public float VolumeEffect {
        get {
            return volumeEffect;
        }
        set {
            volumeEffect = Mathf.Clamp(value, 0f, 1f);
        }
    }

    public void PlayEffectSFX(SFX sfx, AudioSource audioSource) {
        audioSource.PlayOneShot(sfx.clip, sfx.volume * VolumeEffect);
    }

    public void PlayEffectSFX(SFX sfx, Vector3 pos) {
        AudioSource.PlayClipAtPoint(sfx.clip, pos, sfx.volume * VolumeEffect);
    }

    public float VolumeUI {
        get {
            if (audioSourceUI) {
                return audioSourceUI.volume;
            } else return 0f;
        }
        set {
            audioSourceUI.volume = Mathf.Clamp(value, 0f, 1f);
        }
    }

    public void PlayUserInterfaceSFX(SFX sfx) {
        if (audioSourceUI) audioSourceUI.PlayOneShot(sfx.clip, sfx.volume * VolumeUI);
    }

    public void PlayUserInterfaceSFX(AudioClip clip) {
        if (audioSourceUI) audioSourceUI.PlayOneShot(clip, VolumeUI);
    }

    public float VolumeMusic {
        get {
            if (musicPlayer) {
                return musicPlayer.volume;
            } else return 0f;
        }
        set {
            if (musicPlayer) musicPlayer.volume = Mathf.Clamp(value, 0f, 1f);
        }
    }
}

[Serializable]
public struct SFX {
    public AudioClip clip;
    [Range (0f, 1f)]        
    public float volume;    
}
