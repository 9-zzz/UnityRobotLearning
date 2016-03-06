using UnityEngine;
using System.Collections;
using System.IO;
using JSON;

public class JSONParser : MonoBehaviour
{

    [SerializeField]
    private Person person = null;

    private void Start()
    {
        string json = File.ReadAllText(Path.Combine(Application.dataPath, "C:/Users/S/Documents/GitHub/UnityRobotLearning/Assets/_Scripts/btree.json"));

        this.person = (Person)JSONSerialize.Deserialize(typeof(Person), json);

        print(json);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space)) { JSONSerialize.Serialize(Path.Combine(Application.dataPath, "configNew.json"), this.person); }
    }


}
