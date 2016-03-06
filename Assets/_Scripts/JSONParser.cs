using UnityEngine;
using System.Collections;
using System.IO;
using JSON;

public class JSONParser : MonoBehaviour
{

 

    [SerializeField]
    private Brain brain = null;

    private void Start()
    {
        string json = File.ReadAllText(Path.Combine(Application.dataPath, "btree.json"));

        this.brain = (Brain)JSONSerialize.Deserialize(typeof(Brain), json);
        //print(json);

        for (int i = 0; i < 4; i++)
        {
            print(this.brain.behaviors[i].behavior);
            print(this.brain.behaviors[i].likelihood);
            print(this.brain.behaviors[i].next);
        }
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space)) { JSONSerialize.Serialize(Path.Combine(Application.dataPath, "configNew.json"), this.person); }
    }


}
