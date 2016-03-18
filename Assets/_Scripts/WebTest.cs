using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class WebTest : MonoBehaviour
{

    public MovieTexture movieTexture;
    protected bool streamReady = false;
    float movevar = 0;
    // Use this for initialization
    void Start()
    {
        //StartCoroutine(TestParseDotCom());
        StartCoroutine(Check());
        //StartCoroutine(StartStream(@"file:///Users/ray.pendergraph/test.ogv"));

        StartCoroutine(StartStream("http://9.github.io/videos/heads.mp4"));
    }

    // Update is called once per frame
    void Update()
    {

    }

    protected IEnumerator StartStream (string url)
    {
        WWW videoStreamer = new WWW (url);
        movieTexture = videoStreamer.movie;
        //audio.clip = movieTexture.audioClip;
 
        while (!movieTexture.isReadyToPlay) {
            yield return 0;
        }
 
        //audio.Play ();
        movieTexture.Play ();
        streamReady = true;
    }

    private IEnumerator Check()
    {
        WWW w = new WWW("http://localhost:8000/Desktop/test.txt");
        yield return w;
        if (w.error != null)
        {
            Debug.Log("Error .. " + w.error);
            // for example, often 'Error .. 404 Not Found'
        }
        else
        {
            Debug.Log("Found ... ==>" + w.text + "<==");
            Debug.Log("Found ... ==>" + w.text.Substring(0,2)+ "<==");
            Debug.Log("Size ==>" + w.size + "<==");
             

            movevar = int.Parse(w.text.Substring(0,2));

            /*
            transform.Translate(0, 0, movevar);

            for(float i = 0; i < movevar; i+= 2)
            {
                Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), new Vector3(0, 0, i), transform.rotation);
            }
            */

            // don't forget to look in the 'bottom section'
            // of Unity console to see the full text of
            // multiline console messages.
        }

        /* example code to separate all that text in to lines:
        longStringFromFile = w.text
        List<string> lines = new List<string>(
            longStringFromFile
            .Split(new string[] { "\r","\n" },
            StringSplitOptions.RemoveEmptyEntries) );
        // remove comment lines...
        lines = lines
            .Where(line => !(line.StartsWith("//")
                            || line.StartsWith("#")))
            .ToList();
        */
    }


    IEnumerator TestParseDotCom()
    {
        Debug.Log("Testing Parse.com");
        Security.PrefetchSocketPolicy("api.parse.com", 843);

        var r = new HTTP.Request("GET", "http://localhost:8000/Desktop/test.txt");
        yield return r.Send();

        if (r.exception == null)
        {
            Debug.Log(r.response.status);
            Debug.Log(r.response.Text);
        }
        else
        {
            Debug.LogError(r.exception);
        }
    }

}
