using System;
using System.Collections.Generic;
using UnityEngine;

namespace CookOrPanic.AudioManager
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [Serializable]
        public class Sound
        {
            public string name;
            public AudioClip audioClip;
            public AudioSource source; // SEKARANG WAJIB DIISI

            [Range(0f, 1f)] public float volume = 1f;
            [Range(.1f, 3f)] public float pitch = 1f;
            public bool loop = false;
        }

        [Header("Daftar Semua Suara")]
        public List<Sound> _sfxSounds;

        private Dictionary<string, Sound> _sfxDict = new Dictionary<string, Sound>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitDictionaries();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitDictionaries()
        {
            foreach (var s in _sfxSounds) _sfxDict[s.name] = s;
        }

        public void PlaySFX(string name)
        {
            if (_sfxDict.ContainsKey(name))
            {
                Sound s = _sfxDict[name];

                if (s.source == null)
                {
                    Debug.LogError($"AudioManager: Suara '{name}' tidak punya AudioSource!");
                    return;
                }

                // Proteksi agar suara Loop tidak restart terus menerus
                if (s.loop && s.source.isPlaying && s.source.clip == s.audioClip) return;

                s.source.clip = s.audioClip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;

                s.source.Play();
            }
            else Debug.LogWarning($"SFX {name} tidak ketemu!");
        }

        public void StopSFX(string name)
        {
            if (_sfxDict.ContainsKey(name) && _sfxDict[name].source != null)
            {
                _sfxDict[name].source.Stop();
            }
        }

        public bool IsPlaying(string name)
        {
            if (_sfxDict.ContainsKey(name))
            {
                Sound s = _sfxDict[name];
                return s.source != null && s.source.isPlaying && s.source.clip == s.audioClip;
            }
            return false;
        }
    }
}