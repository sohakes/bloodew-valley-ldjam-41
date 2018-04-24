using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//[System.Serializable]
public class EvilCarrot : MonoBehaviour {
    public GameObject theTarget;
    public float speed = 1;

	// Use this for initialization
	void Start () {
        speed = Random.Range(0.7f * speed, 1.1f * speed);
    }
	
	// Update is called once per frame
	void Update () {
        if (theTarget == null)
        {
            if(SceneManager.GetActiveScene().name.Equals("scene1")) theTarget = GameObject.Find("Player");
            return;
        }
        float step = Random.Range(speed - 0.05f, speed + 0.05f) * Time.deltaTime;
        Vector3 tPos = theTarget.transform.position;
        tPos.x += Random.Range(-0.5f, 0.5f);
        tPos.z += Random.Range(-0.5f, 0.5f);
        Vector3 pos = Vector3.MoveTowards(transform.position, tPos, step);
        pos.y = transform.position.y;
        transform.position = pos;
        transform.LookAt(theTarget.transform);
    }
}
