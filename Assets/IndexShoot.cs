using UnityEngine;
using System.Collections;

public class IndexShoot : MonoBehaviour {

    public GameObject b;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKey(KeyCode.X))
        {
            var a = Instantiate(b, transform.position, transform.rotation) as GameObject;
            a.GetComponent<Rigidbody>().AddRelativeForce(0, 0, -2, ForceMode.Impulse);
        }

    }


}
