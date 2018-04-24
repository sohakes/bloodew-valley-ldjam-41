using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JunimoDefender : MonoBehaviour {

    public static int quantity = 0;

    public float speed = 1.5f;

	// Use this for initialization
	void Start () {
        quantity++;


    }
	
	// Update is called once per frame
	void Update () {
        GameObject[] evilVeggies = GameObject.FindGameObjectsWithTag("EvilVeggie");
        GameObject closest = GameObject.FindGameObjectWithTag("EvilGrandpa");
        float closestDist = Mathf.Infinity;
        foreach (GameObject ev in evilVeggies)
        {
            float dist = Vector3.Distance(transform.position, ev.transform.position);
            if (dist < closestDist) {
                closestDist = dist;
                closest = ev;
            }
        }

        if (closest == null) {
            quantity--;
            Destroy(gameObject);
            return;
        }

        float step = speed * Time.deltaTime;
        Vector3 tPos = closest.transform.position;
        //Vector3 pos = transform.position + Vector3.Normalize(tPos - transform.position)*step;
        Vector3 pos = Vector3.MoveTowards(transform.position, tPos, step);
        pos.y = transform.position.y;
        transform.position = pos;
        Vector3 clospos = closest.transform.position;
        clospos.y = transform.position.y;
        transform.LookAt(clospos);

    }

    void OnTriggerEnter(Collider col)
    {
        if (Game.grandpaBattle) speed = 1.2f;
        if (col.gameObject.tag == "EvilVeggie")
        {
            quantity--;
            Destroy(col.gameObject);
            Destroy(gameObject);
            Game.current.veggiesLeft--;
        }
        if (col.gameObject.tag == "EvilGrandpa")
        {
            quantity--;
            Destroy(gameObject);
            col.gameObject.GetComponent<EvilGrandpa>().damage();
        }
    }
}
