using JSON;
[System.Serializable]
public class Brain
{
    [JSONItem("id",typeof(int))]
    public int id = 0;
    [JSONItem("smart_object", typeof(string))]
    public string smart_object= "smart_object";
    [JSONArray("behaviors", typeof(Behaviors))]
    public Behaviors[] behaviors;
}
[System.Serializable]
public class Behaviors
{
    [JSONItem("behavior", typeof(string))]
    public string behavior = null;
    [JSONItem("likelihood", typeof(string))]
    public string likelihood = null;
    [JSONItem("next", typeof(string))]
    public string next = null;
}
