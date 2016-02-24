using UnityEngine;
using System.Collections;

public class TrailCircles : MonoBehaviour
{
    public Material toLerpTo;
    public Material material1;
    public Material material2;
    public float duration = 2.0F;
    public Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material = material1;
    }

    void Update()
    {
        float lerp = Mathf.PingPong(Time.time, duration) / duration;
        rend.material.Lerp(material1, material2, lerp);
    }

}
