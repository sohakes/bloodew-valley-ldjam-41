using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EvilGrandpa : MonoBehaviour {

    public float speed = 1.5f;
    public float speed2 = 2f;
    private GameObject theTarget;
    private int health = 10;
    public float spawnDistance = 5;
    private bool stage1 = false;
    private bool stage2 = false;
    public static EvilGrandpa instance = null;

    public EvilGrandpa()
    {
        instance = this;
    }

    // Use this for initialization
    void Start () {
        
        this.GetComponent<Animator>().SetTrigger("idle");
	}

    public void startZombie()
    {
        this.GetComponent<Animator>().SetTrigger("zombie");
        stage1 = true;
    }
	
	// Update is called once per frame
	void Update () {
        if (!stage1) return;
        if (theTarget == null)
        {
            theTarget = FirstPersonControllerMine.instance.gameObject;
        }
        //float step = Random.Range(speed - 0.05f, speed + 0.05f) * Time.deltaTime;
        if(health <= 5 && !stage2)
        {
            speed = speed2;
            this.GetComponent<Animator>().SetTrigger("flying");
            stage2 = true;
        }
        if(Game.current.currentLife <= 0) { return;  }
        float step = speed * Time.deltaTime;
        Vector3 tPos = theTarget.transform.position;
        //tPos.x += Random.Range(-0.5f, 0.5f);
        //tPos.z += Random.Range(-0.5f, 0.5f);
        Vector3 pos = Vector3.MoveTowards(transform.position, tPos, step);
        pos.y = transform.position.y;
        transform.position = pos;
        transform.LookAt(theTarget.transform);
    }

    public void resetPosition()
    {
        if (theTarget == null)
        {
            theTarget = GameObject.Find("Player");
        }
        Vector2 randPos = Random.insideUnitCircle * spawnDistance;

        transform.position =  new Vector3(randPos.x, 0, randPos.y) + theTarget.transform.position;
    }

    public void damage()
    {
        health--;
        SoundEffects.sf.playTheyHurt();
        if(health <= 0)
        {
            Game.current.veggiesLeft--;
            Destroy(gameObject);
            return;
        }
        resetPosition();
    }

}
