using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum AchievementType
{
    PelletsEaten,
    GhostsEaten,
    LevelsCleared,
    TotalDeaths,
    AbilitiesUsed,
    FruitEaten
}

[System.Serializable]
public class Achievement
{
    public string name;
    public string description;
    public AchievementType type;
    public int requiredAmount;
    public int rewardPoints;
    public bool isCompleted = false;
}

public class AchievementManager : MonoBehaviour, InterfaceDPM
{
    public static AchievementManager AchvManager;
    public MainMenuController mainMenuController;
    
    public List<Achievement> achievements = new List<Achievement>();

    [Header("Achievement Popup Settings")]
    public GameObject achievementPopupPrefab;
    public Transform popupParent;
    public AudioSource achvAudio;

    private Queue<Achievement> achievementQueue = new Queue<Achievement>();
    private bool isDisplaying = false;

    void Awake()
    {
        if (AchvManager == null)
        {
            AchvManager = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject popupParentObj = GameObject.Find("AchievementPopUpParent");

        if (popupParentObj != null)
        {
            popupParent = popupParentObj.transform;
        }
        else
        {
            popupParent = null;
        }
    }

    public void LoadData(GameData gameData)
    {
        foreach (Achievement achievement in achievements)
        {
            if (gameData.unlockedAchievements.Contains(achievement.name))
            {
                achievement.isCompleted = true;
            }
            else
            {
                achievement.isCompleted = false;
            }
        }
    }

    public void SaveData(GameData gameData)
    {   
        if (gameData.unlockedAchievements == null)
        {
            gameData.unlockedAchievements = new List<string>();
        }
        gameData.unlockedAchievements.Clear();
        foreach (Achievement achievement in achievements)
        {
            if (achievement.isCompleted && !gameData.unlockedAchievements.Contains(achievement.name))
            {
                gameData.unlockedAchievements.Add(achievement.name);
            }
        }
    }

    public void CheckAchievements(GameData gameData)
    {
        foreach (Achievement achievement in achievements)
        {
            if (!achievement.isCompleted)
            {
                bool achievementCompleted = false;

                switch (achievement.type)
                {//Make sure to use correct stat to track achievement
                    case AchievementType.PelletsEaten:
                        if (gameData.totalPelletsEaten >= achievement.requiredAmount)
                            achievementCompleted = true;
                        break;

                    case AchievementType.GhostsEaten:
                        if (gameData.totalGhostsEaten >= achievement.requiredAmount)
                            achievementCompleted = true;
                        break;

                    case AchievementType.LevelsCleared:
                        if (gameData.LevelsCleared >= achievement.requiredAmount)
                            achievementCompleted = true;
                        break;

                    case AchievementType.TotalDeaths:
                        if (gameData.totalDeaths >= achievement.requiredAmount)
                            achievementCompleted = true;
                        break;

                    case AchievementType.AbilitiesUsed:
                        if (gameData.abilitiesUsed >= achievement.requiredAmount)
                            achievementCompleted = true;
                        break;
                    case AchievementType.FruitEaten:
                        if (gameData.fruitEaten >= achievement.requiredAmount)
                            achievementCompleted = true;
                        break;

                }

                if (achievementCompleted)
                {
                    achievement.isCompleted = true;
                    gameData.CurrentLevelPoints += achievement.rewardPoints;

                    GameManager gameManager = FindObjectOfType<GameManager>();
                    if (gameManager != null)
                    {
                        gameManager.CheckForLevelUp(gameData); 
                    }
                    else
                    {
                        Debug.LogWarning("GameManager not found in the scene.");
                    }

                    DataPersistenceManager.Instance.SaveGame();
                    if (mainMenuController != null)
                    {
                        mainMenuController.UpdateAchievementDisplay();
                    }
                    Debug.Log($"Achievement Unlocked: {achievement.name}! +{achievement.rewardPoints} XP");

                    QueueAchievementPopup(achievement);
                }
            }
        }
    }

    private void QueueAchievementPopup(Achievement achievement)
    {
        achievementQueue.Enqueue(achievement);
        if (!isDisplaying)
        {
            StartCoroutine(DisplayNextAchievement());
        }
    }

    private IEnumerator DisplayNextAchievement()
    {
        while (achievementQueue.Count > 0)
        {
            isDisplaying = true;
            Achievement achievement = achievementQueue.Dequeue();
            achvAudio.Play();
            GameObject popup = Instantiate(achievementPopupPrefab, popupParent);
            TMP_Text titleText = popup.transform.Find("AchvName").GetComponent<TMP_Text>();
            TMP_Text xpText = popup.transform.Find("AchvReward").GetComponent<TMP_Text>();

            titleText.text = achievement.name;
            xpText.text = $"+{achievement.rewardPoints} XP";

            popup.SetActive(true);

            yield return new WaitForSeconds(3f);
            Destroy(popup);
        }

        isDisplaying = false;
    }

    public void ResetAchievements()
    {
        foreach (Achievement achievement in achievements)
        {
            achievement.isCompleted = false;
        }
    }
}

