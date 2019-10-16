using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class BackgroundMusicScript : MonoBehaviour
{
    public static BackgroundMusicScript instance;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //load volume levels from the player prefs
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume", 0);
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume", 0);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("musicVolume", volume);
    }

    public void SetSfxVolume(float volume)
    {
        audioMixer.SetFloat("sfxVolume", volume);
    }

    private void OnDisable()
    {
        //create variables with max volume levels as defaults
        float musicVolume = 0;
        float sfxVolume = 0;

        //replace default max values with set values in game
        audioMixer.GetFloat("musicVolume", out musicVolume);
        audioMixer.GetFloat("sfxVolume", out sfxVolume);

        //store volume values in the player prefs
        PlayerPrefs.SetFloat("musicVolume", musicVolume);
        PlayerPrefs.SetFloat("sfxVolume", sfxVolume);
        PlayerPrefs.Save();
    }
}
