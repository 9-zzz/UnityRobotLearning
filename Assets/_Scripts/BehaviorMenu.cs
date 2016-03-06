using UnityEngine;
using System.Collections;

public class BehaviorMenu : MonoBehaviour {
    public GameObject prefab;


    // Use this for initialization
    void Start () {

        for (int i = 0; i < 3; i++)
        {
            Transform offset = this.transform;

            GameObject myButton = (GameObject)Instantiate(prefab, this.transform.position, this.transform.rotation);
            myButton.transform.SetParent(this.transform);
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
