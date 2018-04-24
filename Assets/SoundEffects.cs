using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffects : MonoBehaviour {

    public AudioClip gold;
    public AudioClip mehurt;
    public AudioClip menuclick;
    public AudioClip powerup;
    public AudioClip theyhurt;
    public AudioClip hoetile;
    public AudioClip water;
    private AudioSource audioSource;
    public static SoundEffects sf;

    private void Awake()
    {
        sf = this;
    }

    // Use this for initialization
    void Start () {
        audioSource = this.GetComponent<AudioSource>();
    }

    public void playGold()
    {
        audioSource.PlayOneShot(gold);
    }
    public void playHoeTile()
    {
        audioSource.PlayOneShot(hoetile);
    }
    public void playWater()
    {
        audioSource.PlayOneShot(water);
    }

    public void playMeHurt()
    {
        audioSource.PlayOneShot(mehurt);
    }

    public void playMenuClick()
    {
        audioSource.PlayOneShot(menuclick);
    }

    public void playPowerUp()
    {
        audioSource.PlayOneShot(powerup);
    }

    public void playTheyHurt()
    {
        audioSource.PlayOneShot(theyhurt);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
