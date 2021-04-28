using System;

// PlayerValues is a simple custom collection to store relevant data that other classes might be interested in
// PlayerValues can also easily be stored across scenes by using a Singleton-Manager
public class PlayerValues
{   
    public static event Action<int, int> ScoreUpdatedEvent;
    public static event Action<int, int> LivesUpdatedEvent;

    public PlayerValues(int id, int lives, int score)
    {
        ID = id;
        Lives = lives;
        Score = score;
        NextLiveTreshold = 20000; // TODO use data

        LivesUpdatedEvent?.Invoke(id, Lives);
        ScoreUpdatedEvent?.Invoke(id, Score);
    }

    public int ID{get;private set;}
    public int Lives{get; private set;}
    public int Score{get; private set;}
    public int NextLiveTreshold{get; private set;}

    public void UpdateScore(int points)
    {
        Score += points;

        ScoreUpdatedEvent.Invoke(ID, Score);

        if (Score >= NextLiveTreshold)
        {
            AddOrRemoveLive(1);
            NextLiveTreshold += 20000; // TODO use data
        }
    }

    public void AddOrRemoveLive(int count)
    {
        Lives += count;
        LivesUpdatedEvent?.Invoke(ID, Lives);
    }
}