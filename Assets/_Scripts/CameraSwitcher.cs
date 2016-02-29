using UnityEngine;
using System.Collections;

public class CameraSwitcher : MonoBehaviour
{

    public Camera fpCam;
    public Camera regularCam;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            fpCam.gameObject.SetActive(!fpCam.isActiveAndEnabled);
            regularCam.gameObject.SetActive(!regularCam.isActiveAndEnabled);
        }
    }

}
