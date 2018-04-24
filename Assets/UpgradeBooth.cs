using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeBooth : MonoBehaviour {
    public GameObject junimo;
    public GameObject boot;
    public GameObject hand;

    // Use this for initialization
    void Start () {
        if (Game.current.hasJunimoHut) junimo.SetActive(false);
        if (Game.current.hasHermesBoot) boot.SetActive(false);
        if (Game.current.hasLightGloves) hand.SetActive(false);

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
