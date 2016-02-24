using UnityEngine;
using System.Collections;

public class AgentTrailMaker : MonoBehaviour
{

    public GameObject flatCircleMesh;
    public GameObject ground;
    public float waitTime;
    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        StartCoroutine(SpawnWhileMoving());

    }

    void Update()
    {

    }

    IEnumerator SpawnWhileMoving()
    {
        while (true)
        {
            while (agent.velocity.magnitude > 0)
            {
                print("poop");
                Instantiate(flatCircleMesh, new Vector3(transform.position.x, ground.transform.position.y + 0.01f, transform.position.z), transform.rotation);
                yield return new WaitForSeconds(waitTime);
            }
            yield return new WaitForSeconds(0.0f);
        }
    }

}
