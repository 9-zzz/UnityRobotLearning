using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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
            if (this.gameObject.tag == "2")
            {
                UnitMovingByFileInput.S.DestroyAllGameObjectsWithTag("trailCube");
                UnitMovingByFileInput.S.fileName = "input2.txt";
                UnitMovingByFileInput.S.lines = System.IO.File.ReadAllLines("input2.txt");
                UnitMovingByFileInput.S.MakeTrailCubes();
            }

            if (this.gameObject.tag == "1")
            {
                UnitMovingByFileInput.S.DestroyAllGameObjectsWithTag("trailCube");
                UnitMovingByFileInput.S.fileName = "input.txt";
                UnitMovingByFileInput.S.lines = System.IO.File.ReadAllLines("input.txt");
                UnitMovingByFileInput.S.MakeTrailCubes();
            }

            for (int i = 0; i < 4; i++)
            {
                //Transform offset = this.transform;

                brainNode = (GameObject)Instantiate(prefab, brainRef.transform.position - ((brainRef.transform.up) * i * 100), this.transform.rotation);
                brainNode.transform.SetParent(brainRef.transform);

                brainNode.transform.GetChild(1).GetComponent<Text>().text = JSONParser.S.brain.behaviors[i].behavior;
                brainNode.transform.GetChild(2).GetComponent<Text>().text = JSONParser.S.brain.behaviors[i].likelihood;
                brainNode.transform.GetChild(3).GetComponent<Text>().text = JSONParser.S.brain.behaviors[i].next;
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
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            Destroy(parent.transform.GetChild(i).gameObject);

        }
    }

}
