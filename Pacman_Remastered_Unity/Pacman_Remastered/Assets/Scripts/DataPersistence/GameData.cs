using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public int highestScore;
    public int highestRound;
    public int totalPelletsEaten;
    public int totalGhostsEaten;
    public int totalDeaths;
    public int LevelsCleared;
    public int abilitiesUsed;
    public int fruitEaten;

    public int PlayerLevel;
    public int CurrentLevelPoints;
    public int PointsToNextLevel;

    public List<string> unlockedAchievements = new List<string>();

    public float masterVolume;
    public int chosenColourScheme;


    public GameData()
    {
        this.highestScore = 0;
        this.highestRound = 0;
        this.totalPelletsEaten = 0;
        this.totalGhostsEaten = 0;
        this.PlayerLevel = 0;
        this.totalDeaths = 0;
        this.LevelsCleared = 0;
        this.abilitiesUsed = 0;
        this.fruitEaten = 0;
        this.CurrentLevelPoints = 0;
        this.PointsToNextLevel = 5000;
        unlockedAchievements = new List<string>();
        this.masterVolume = 1.0f;
        this.chosenColourScheme = 0;
    }
}
