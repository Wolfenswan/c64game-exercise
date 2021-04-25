// PlayerValues is a simple custom collection to store relevant data that other classes might be interested in
// PlayerValues can also easily be stored across scenes by using a Singleton-Manager
public class PlayerValues
{      
    public PlayerValues(int id, int lives, int score)
    {
        ID = id;
        Lives = lives;
        Score = score;
    }
    public int ID{get;private set;}
    public int Lives;
    public int Score;
}