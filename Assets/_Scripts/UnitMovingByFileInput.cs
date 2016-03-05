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
    public int xmove;
    public int zmove;

    void Start()
    {
        lines = System.IO.File.ReadAllLines("input.txt");
        StartCoroutine(WaitAndMove());
        print("Length of input lines array: " + lines.Length);
    }

    void Update()
    {

    }

    IEnumerator WaitAndMove()
    {
        for (int i = 0; i < lines.Length; i++)
        {
            yield return new WaitForSeconds(waitTime);

            xmove = int.Parse(lines[i].Substring(0, 2));
            zmove = int.Parse(lines[i].Substring(2, 2));
            rot = int.Parse(lines[i].Substring(5, 3));

            moveVector = new Vector3(xmove, 0, zmove);
            transform.Translate(moveVector);             // Moves along local axes.
            //transform.position += moveVector;

            transform.Rotate(0, rot, 0);
        }
    }

}
