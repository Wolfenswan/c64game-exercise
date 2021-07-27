using System;

// PlayerValues is a custom container to store relevant data that other classes might be interested in
// PlayerValues can also easily be stored across scenes by using a Singleton-Manager
public class PlayerValues
{   
    public static event Action<PlayerID, int> ScoreUpdatedEvent; // Sends ID + current score
    public static event Action<PlayerID, int> LivesUpdatedEvent; // Sends ID + current lives

    public PlayerValues(PlayerID id, int lives, int score, int bonusLiveTreshold)
    {
        ID = id;
        Lives = lives;
        Score = score;
        NextBonusLiveAt = bonusLiveTreshold;
        _bonusLiveTreshold = bonusLiveTreshold;

        LivesUpdatedEvent?.Invoke(id, Lives);
        ScoreUpdatedEvent?.Invoke(id, Score);
    }

    public PlayerID ID{get;private set;}
    public int IDInt{get => (int) ID - 1;} // IDInt is exclusively used to select list-elements according to their index, thus why it's reduced by 1 to avoid index missing errors
    public int Lives{get; private set;}
    public int Score{get; private set;}
    public int NextBonusLiveAt{get; private set;}

    public int _bonusLiveTreshold;

    public void UpdateScore(int points)
    {
        Score += points;

        ScoreUpdatedEvent?.Invoke(ID, Score);

        if (Score >= NextBonusLiveAt)
        {
            AddOrRemoveLive(1);
            NextBonusLiveAt += _bonusLiveTreshold;
        }
    }

    public void AddOrRemoveLive(int count)
    {
        Lives += count;
        LivesUpdatedEvent?.Invoke(ID, Lives);
    }
}