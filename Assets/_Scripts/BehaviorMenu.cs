using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BehaviorMenu : MonoBehaviour {
    public GameObject prefab;

    public int offset = 2;

    // Use this for initialization
    void Start () {

        for (int i = 0; i < 3; i++)
        {
            Transform offset = this.transform;

            GameObject myButton = (GameObject)Instantiate(prefab, this.transform.position - ((transform.up)*i*100), this.transform.rotation);
            myButton.transform.SetParent(this.transform);
            //myButton.gameObject.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = "" ;
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
