using UnityEngine;
using System.Collections;

public class AffordanceUI : MonoBehaviour
{
    bool clicked = false;

    public GameObject brainRef;
    public GameObject prefab;
    private GameObject brainNode;


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseDown()
    {
        if (!clicked)
        {
            for (int i = 0; i < 3; i++)
            {
                //Transform offset = this.transform;

                brainNode = (GameObject)Instantiate(prefab, brainRef.transform.position - ((brainRef.transform.up) * i * 100), this.transform.rotation);
                brainNode.transform.SetParent(brainRef.transform);
            }
            clicked = true;
        }
        else
        {
            DestroyChildren(brainRef);
            clicked = false;
        }
    }

    void DestroyChildren(GameObject parent)
    {
        for(int i = 0; i < parent.transform.childCount; i++)
        {
            Destroy(parent.transform.GetChild(i).gameObject);

        }
    }

}
