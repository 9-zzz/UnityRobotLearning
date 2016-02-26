using UnityEngine;
using System.Collections;

public class TargetMarkerSmoothFollow : MonoBehaviour
{

    public float speed = 10.0f;
    public GameObject target;
    public GameObject robotAvater;
    public Vector3 verticalOffset;

    // Use this for initialization
    void Start()
    {
        //target = GameObject.Find("TargetMarker");
        //robotAvater = GameObject.Find("RobotAvatar");
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position + (verticalOffset), Time.deltaTime * speed);

        if (Vector3.Distance(target.transform.position, robotAvater.transform.position) < 4.0f)
        {
            verticalOffset = new Vector3(0, 6, 0);
        }
        else
        {
            verticalOffset = new Vector3(0, 1, 0);
        }
    }

}
