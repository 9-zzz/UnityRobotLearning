using UnityEngine;
using System.Collections;

public class camtest : MonoBehaviour {

    WebCamTexture it;
	// Use this for initialization
	void Start () {

        it = new WebCamTexture();
        GetComponent<Renderer>().material.mainTexture = it;
        it.Play();
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
