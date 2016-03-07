using UnityEngine;
using System.Collections;
using System.IO;

public class UnitMovingByFileInput : MonoBehaviour
{
    public static UnitMovingByFileInput S;

    public int unitsToMove = 20;
    public float waitTime = 1.0f;
    public string[] lines;
    public string fileName = "input.txt";

    public int rot = 0;
    public Vector3 moveVector;
    public Vector3 trailVector = new Vector3(0.5f, 8.0f, 1.0f);
    public float xmove;
    public float zmove;

    public GameObject trailCubePF;
    public int i = 0;
     
    void Awake()
    {
        S = this;
    }

    void Start()
    {
        lines = System.IO.File.ReadAllLines(fileName);
        //print("Length of input lines array: " + lines.Length);
        MakeTrailCubes();

        StartCoroutine(WaitAndMove());

    }

    void Update()
    {

    }

    public void DestroyAllGameObjectsWithTag(string tag)
    {
        GameObject[] gameObjects;
        gameObjects = GameObject.FindGameObjectsWithTag(tag);

        for (int j = 0; j < gameObjects.Length; j++)
        {
            Destroy(gameObjects[j]);
        }
    }

    public void MakeTrailCubes()
    {
        for (int k=i; k < lines.Length; k++)
        {
            //yield return new WaitForSeconds(waitTime/2.0f);
            lines = System.IO.File.ReadAllLines(fileName);
            xmove = int.Parse(lines[k].Substring(0, 5));
            zmove = int.Parse(lines[k].Substring(5, 5));
            rot = int.Parse(lines[k].Substring(11, 3));
            xmove += 0.5f;
            trailVector = new Vector3(xmove, 8, zmove);
            Instantiate(trailCubePF, trailVector, Quaternion.Euler(0, rot, 0));
        }
    }

    IEnumerator WaitAndMove()
    {
        for (i = 0; i < lines.Length; i++)
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
