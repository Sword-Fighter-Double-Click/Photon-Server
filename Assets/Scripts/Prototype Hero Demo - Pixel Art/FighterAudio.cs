using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string m_name;
    public AudioClip[] m_clips;

    [Range(0f, 1f)]
    public float volume = 1.0f;
    [Range(0f, 1.5f)]
    public float pitch = 1.0f;

    public void Play(AudioSource m_source)
    {
		if (m_clips.Length == 0) return;

		m_source.clip = m_clips.Length > 1 ? m_clips[Random.Range(0, m_clips.Length - 1)] : m_clips[0];
        m_source.volume = volume;
        m_source.pitch = pitch;
        m_source.PlayOneShot(m_source.clip);
    }
}

public class FighterAudio : MonoBehaviour
{
	private AudioSource m_source;

    [SerializeField]
    Sound[] m_sounds;

	private void Awake()
	{
		m_source = GameObject.Find("Main Camera").GetComponent<AudioSource>();
	}

    public void PlaySound (string name)
    {
        for(int i = 0; i < m_sounds.Length; i++)
        {
            if(m_sounds[i].m_name == name)
            {
                m_sounds[i].Play(m_source);
                return;
            }
        }

        Debug.LogWarning("AudioManager: Sound name not found in list: " + name);
    }
}
