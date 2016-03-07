using UnityEngine;
using System.Collections;
using System.IO;

public class UnitMovingByFileInput : MonoBehaviour
{
    public int unitsToMove = 20;
    public float waitTime = 1.0f;
    public string[] lines;

    public int rot = 0;
    public Vector3 moveVector;
    public Vector3 trailVector = new Vector3(0.5f, 8.0f, 1.0f);
    public float xmove;
    public float zmove;

    public GameObject trailCubePF;

    void Start()
    {
        lines = System.IO.File.ReadAllLines("input.txt");
        print("Length of input lines array: " + lines.Length);

        for (int i = 1; i < lines.Length; i++)
        {
            xmove = int.Parse(lines[i].Substring(0, 5));
            zmove = int.Parse(lines[i].Substring(5, 5));
            rot = int.Parse(lines[i].Substring(11, 3));

            xmove += 0.5f;
            
            trailVector = new Vector3(xmove, 8, zmove);

            Instantiate(trailCubePF, trailVector, Quaternion.Euler(0, rot, 0));
        }

        StartCoroutine(WaitAndMove());
    }

    void Update()
    {

    }

    IEnumerator WaitAndMove()
    {
        for (int i = 0; i < lines.Length; i++)
        {
            yield return new WaitForSeconds(waitTime);

            xmove = int.Parse(lines[i].Substring(0, 5));
            zmove = int.Parse(lines[i].Substring(5, 5));
            rot = int.Parse(lines[i].Substring(11, 3));

            xmove += 0.5f;
            zmove += 0.5f;

            moveVector = new Vector3(xmove, 8, zmove);
            //transform.Translate(moveVector);             // Moves along local axes.
            transform.position = moveVector;

            //transform.Rotate(0, rot, 0);
            transform.rotation = Quaternion.Euler(0, rot, 0);
        }
    }
}
