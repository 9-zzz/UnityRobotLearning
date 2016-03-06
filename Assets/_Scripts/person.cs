using JSON;
[System.Serializable]
public class Person
{
    [JSONItem("id",typeof(int))]
    public int id = 0;
    [JSONItem("name", typeof(string))]
    public string name = "name";
    [JSONItem("director", typeof(bool))]
    public int age = 0;
    [JSONArray("movie", typeof(Movie))]
    public Movie[] movie;
}
[System.Serializable]
public class Movie
{
    [JSONItem("title", typeof(string))]
    public string title = null;
    [JSONItem("status", typeof(string))]
    public string status = null;
    [JSONItem("originalstory", typeof(string))]
    public string originalstory = null;
    [JSONItem("originalwriter", typeof(string))]
    public string originalwriter = null;
}
