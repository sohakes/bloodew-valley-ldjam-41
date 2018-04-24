using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JunimoHut : MonoBehaviour {
    public JunimoDefender defender;
    private float lastJunimo = 0;
    public float spawnRate = 4;
    public Transform spawnLocation;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Game.grandpaBattle) spawnRate = 6;
        if (Game.current.hasJunimoHut && !Game.stopped && JunimoDefender.quantity == 0)
        {
            if(lastJunimo + spawnRate < Game.current.currentTime)
            {
                lastJunimo = Game.current.currentTime;
                JunimoDefender aDefender = GameObject.Instantiate(defender);
                aDefender.transform.position = spawnLocation.position;
                aDefender.transform.rotation = spawnLocation.rotation;
            }
        }
		
	}
}
