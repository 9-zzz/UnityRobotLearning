using UnityEngine;
using System.Collections;
using System.IO;

public class FileWriteTest : MonoBehaviour
{

    string fileName = "log.txt";
    public StreamWriter sw;

    void Start()
    {
        if (File.Exists(fileName))
        {
            Debug.Log(fileName + " already exists. Deleting file...");
            File.Delete(fileName);
            //return;
        }

        sw = File.CreateText(fileName);
    }

    void Update()
    {

    }

}
