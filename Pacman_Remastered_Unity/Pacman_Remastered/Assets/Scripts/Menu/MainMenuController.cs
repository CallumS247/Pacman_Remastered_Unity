using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Menus")]
    public GameObject MainMenu;
    public GameObject gameModeMenu;
    public GameObject settingsMenu;
    public GameObject colourSchemeMenu;
    public GameObject profileMenu;
    public GameObject achievementMenu;
    public GameObject deleteConfirmationPopup;
    public GameObject loadoutMenu;
    public GameObject infoMenu;
    public GameObject HazardInfoMenu;

    [Header("Ability Info Displays")]
    public GameObject speedBoostInfo;
    public GameObject freezeInfo;
    public GameObject doublePointsInfo;
    public GameObject frightenInfo;
    public GameObject nukeInfo;
    public GameObject teleportInfo;
    public GameObject extraLifeInfo;

    [Header("Hazard Info Displays")]
    public GameObject nearsightInfo;
    public GameObject dmgZoneInfo;
    public GameObject ghostImmuneInfo;
    public GameObject invertControlsInfo;
    public GameObject powerDrainInfo;
    public GameObject countdownInfo;

    [Header("Profile Stats")]
    [SerializeField] private TextMeshProUGUI playerLevelText;
    [SerializeField] private TextMeshProUGUI levelProgressText;
    [SerializeField] private TextMeshProUGUI progessPercentageText;
    [SerializeField] private TextMeshProUGUI highestScoreText;
    [SerializeField] private TextMeshProUGUI highestRoundText;
    [SerializeField] private TextMeshProUGUI totalPelletsEatenText;
    [SerializeField] private TextMeshProUGUI totalGhostsEatenText;
    [SerializeField] private TextMeshProUGUI totalDeathsText;
    [SerializeField] private TextMeshProUGUI levelsClearedText;
    [SerializeField] private TextMeshProUGUI abilitiesUsedText;
    [SerializeField] private TextMeshProUGUI fruitEatenText;
    [SerializeField] private TextMeshProUGUI completedAchievements;

    [Header("Achievement Menu")]
    public GameObject achievementPrefab;
    public Transform achievementContainer;
    public AchievementManager achvManager;

    [Header("Loadout")]
    public LoadoutManager loadoutManager;
    public Button slot1Button;
    public Button slot2Button;
    private int selectedSlot = 0;

    [Header("Colour Schemes")]
    public Image[] colourSchemePreviews;
    public Toggle[] colourSchemeToggles;
    

    void Start()
    {
        //Activate only the Main Menu at the start
        MainMenu.SetActive(true);
        gameModeMenu.SetActive(false);
        profileMenu.SetActive(false);
        settingsMenu.SetActive(false);
        colourSchemeMenu.SetActive(false);
        achievementMenu.SetActive(false);
        deleteConfirmationPopup.SetActive(false);
        loadoutMenu.SetActive(false);
        infoMenu.SetActive(false);
        HazardInfoMenu.SetActive(false);

        //Reset time scale for menu background
        Time.timeScale = 1;

        //Adjust volume to saved settings
        float savedVolume = DataPersistenceManager.Instance.GetCurrentGameData().masterVolume;
        AudioListener.volume = savedVolume;

        //Make sure the loadout manager is the same one from the game scene if returning to menu
        if (LoadoutManager.Instance != null)
        {
            LoadoutManager menuLoadoutManager = FindObjectOfType<LoadoutManager>();
            if (menuLoadoutManager != null)
            {
                Destroy(menuLoadoutManager.gameObject);
            }
        }

        ActivateColourSchemeSelection();
    }

    public void StartClassicGame()
    {
        SceneManager.LoadScene("ClassicGame");
    }

    public void StartRemasteredGame()
    {
        switch (Random.Range(0, 2))
        { 
            case 0:
                SceneManager.LoadScene("RMGame1");
                break;
            case 1:
                SceneManager.LoadScene("RMGame2");
                break;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    #region Show Menus
    public void ShowInfoMenu(string menuName)
    {
        //Disable all menus
        speedBoostInfo.SetActive(false);
        freezeInfo.SetActive(false);
        doublePointsInfo.SetActive(false);
        frightenInfo.SetActive(false);
        nukeInfo.SetActive(false);
        teleportInfo.SetActive(false);
        extraLifeInfo.SetActive(false);

        //Enable the requested menu
        switch (menuName)
        {
            case "SpeedBoost":
                speedBoostInfo.SetActive(true);
                break;
            case "Freeze":
                freezeInfo.SetActive(true);
                break;
            case "DoublePoints":
                doublePointsInfo.SetActive(true);
                break;
            case "Frighten":
                frightenInfo.SetActive(true);
                break;
            case "Nuke":
                nukeInfo.SetActive(true);
                break;
            case "Teleport":
                teleportInfo.SetActive(true);
                break;
            case "ExtraLife":
                extraLifeInfo.SetActive(true);
                break;
        }
    }

    public void ShowHazardInfoMenu(string menuName)
    {
        //Disable all menus
        nearsightInfo.SetActive(false);
        ghostImmuneInfo.SetActive(false);
        dmgZoneInfo.SetActive(false);
        invertControlsInfo.SetActive(false);
        powerDrainInfo.SetActive(false);
        countdownInfo.SetActive(false);

        //Enable the requested menu
        switch (menuName)
        {
            case "Nearsight":
                nearsightInfo.SetActive(true);
                break;
            case "GhostImmune":
                ghostImmuneInfo.SetActive(true);
                break;
            case "DmgZone":
                dmgZoneInfo.SetActive(true);
                break;
            case "InvertControls":
                invertControlsInfo.SetActive(true);
                break;
            case "PowerDrain":
                powerDrainInfo.SetActive(true);
                break;
            case "Countdown":
                countdownInfo.SetActive(true);
                break;
        }
    }

    public void ShowMenu(string menuName)

    {
        //Disable all menus
        MainMenu.SetActive(false);
        gameModeMenu.SetActive(false);
        profileMenu.SetActive(false);
        settingsMenu.SetActive(false);
        colourSchemeMenu.SetActive(false);
        achievementMenu.SetActive(false);
        deleteConfirmationPopup.SetActive(false);
        loadoutMenu.SetActive(false);
        infoMenu.SetActive(false);
        HazardInfoMenu.SetActive(false);

        //Make sure any info pop ups are also disabled
        speedBoostInfo.SetActive(false);
        freezeInfo.SetActive(false);
        doublePointsInfo.SetActive(false);
        frightenInfo.SetActive(false);
        nukeInfo.SetActive(false);
        teleportInfo.SetActive(false);
        extraLifeInfo.SetActive(false);

        nearsightInfo.SetActive(false);
        ghostImmuneInfo.SetActive(false);
        dmgZoneInfo.SetActive(false);
        invertControlsInfo.SetActive(false);
        powerDrainInfo.SetActive(false);
        countdownInfo.SetActive(false);

        //Enable the requested menu
        switch (menuName)
        {
            case "MainMenu":
                MainMenu.SetActive(true);
                break;
            case "GameModeMenu":
                gameModeMenu.SetActive(true);
                break;
            case "ProfileMenu":
                profileMenu.SetActive(true);
                UpdateProfileDisplay();
                break;
            case "SettingsMenu":
                settingsMenu.SetActive(true);
                break;
            case "ColourSchemeMenu":
                colourSchemeMenu.SetActive(true);
                break;
            case "AchievementMenu":
                achievementMenu.SetActive(true);
                UpdateAchievementDisplay();
                break;
            case "LoadoutMenu":
                loadoutMenu.SetActive(true);
                loadoutManager.UpdateUnlockedAbilities(DataPersistenceManager.Instance.GetCurrentGameData().PlayerLevel);
                UpdateSlotUI();
                break;
            case "InfoMenu":
                infoMenu.SetActive(true);
                break;
            case "HazardInfoMenu":
                HazardInfoMenu.SetActive(true);
                break;
        }
    }

    #endregion

    #region Update Displays
    //Updating profile screen info
    private void UpdateProfileDisplay()
    {
        //Get current GameData
        GameData currentGameData = DataPersistenceManager.Instance.GetCurrentGameData();

        //Check if data exists
        if (currentGameData == null)
        {
            Debug.LogWarning("No game data found for the current profile.");
            playerLevelText.text = "Player Level: ";
            highestScoreText.text = "Score: ";
            highestRoundText.text = "Round: ";
            totalPelletsEatenText.text = "Pellets Eaten: ";
            totalGhostsEatenText.text = "Ghosts Eaten: ";
            totalDeathsText.text = "Deaths: ";
            levelsClearedText.text = "Levels Cleared: ";
            abilitiesUsedText.text = "Abilities Used: ";
            fruitEatenText.text = "Fruit Eaten: ";
            return;
        }

        //Update UI with current game data
        playerLevelText.text = "Player Level: " + currentGameData.PlayerLevel;
        float progress = ((float)currentGameData.CurrentLevelPoints / currentGameData.PointsToNextLevel) * 100;
        int progressInt = Mathf.RoundToInt(progress);
        levelProgressText.text = $"Progress: {currentGameData.CurrentLevelPoints}/{currentGameData.PointsToNextLevel}";
        progessPercentageText.text = progressInt.ToString() + "%";
        highestScoreText.text = "Score: " + currentGameData.highestScore;
        highestRoundText.text = "Round: " + currentGameData.highestRound;
        totalPelletsEatenText.text = "Pellets Eaten: " + currentGameData.totalPelletsEaten;
        totalGhostsEatenText.text = "Ghosts Eaten: " + currentGameData.totalGhostsEaten;
        totalDeathsText.text = "Deaths: " + currentGameData.totalDeaths;
        levelsClearedText.text = "Levels Cleared: " + currentGameData.LevelsCleared;
        abilitiesUsedText.text = "Abilities Used: " + currentGameData.abilitiesUsed;
        fruitEatenText.text = "Fruit Eaten: " + currentGameData.fruitEaten;

    }

    //Updating achievement screen info
    public void UpdateAchievementDisplay()
    {
        foreach (Transform child in achievementContainer)
        {
            Destroy(child.gameObject);
        }

        int completedAchvCount = 0;

        foreach (Achievement achievement in AchievementManager.AchvManager.achievements)
        {

            GameObject newAchievement = Instantiate(achievementPrefab, achievementContainer);
            newAchievement.transform.Find("AchvName").GetComponent<TextMeshProUGUI>().text = achievement.name;
            newAchievement.transform.Find("AchvDesc").GetComponent<TextMeshProUGUI>().text = achievement.description;
            newAchievement.transform.Find("AchvReward").GetComponent<TextMeshProUGUI>().text = achievement.rewardPoints.ToString() + " XP";

            if (achievement.isCompleted)
            {
                newAchievement.transform.Find("AchvStatus").GetComponent<TextMeshProUGUI>().text = "Completed!";
                newAchievement.transform.Find("AchvStatus").GetComponent<TextMeshProUGUI>().color = Color.green;
                completedAchvCount++;
            }
            else
            {
                newAchievement.transform.Find("AchvStatus").GetComponent<TextMeshProUGUI>().text = "In Progress";
            }
        }

        completedAchievements.text = $"{completedAchvCount} / {AchievementManager.AchvManager.achievements.Count}";
    }

    //Updating loadout screen info
    #region LoadoutDisplay

    public void SetSelectedSlot(int slot)
    {
        selectedSlot = slot;
    }

    public void SelectAbilityForSlot(Ability ability)
    {
        LoadoutManager.Instance.SelectAbility(ability, selectedSlot);
        UpdateSlotUI();
    }

    public void UpdateSlotUI()
    {
        
        if (loadoutManager.selectedAbilities[0] != null)
        {
            slot1Button.GetComponentInChildren<TextMeshProUGUI>().text = LoadoutManager.Instance.selectedAbilities[0].abilityName;
        }
        else
        {
            slot1Button.GetComponentInChildren<TextMeshProUGUI>().text = "Select Ability";
        }

        if (loadoutManager.selectedAbilities[1] != null)
        {
            slot2Button.GetComponentInChildren<TextMeshProUGUI>().text = LoadoutManager.Instance.selectedAbilities[1].abilityName;
        }
        else
        {
            slot2Button.GetComponentInChildren<TextMeshProUGUI>().text = "Select Ability";
        }
    }
    #endregion

    #endregion

    #region Profile Reset
    public void ShowResetConfirmation()
    {
        deleteConfirmationPopup.SetActive(true);
    }

    public void ConfirmResetProfile()
    {
        //Reset the current profile
        DataPersistenceManager.Instance.NewGame();

        achvManager.ResetAchievements();
        GameData currentData = DataPersistenceManager.Instance.GetCurrentGameData();
        if (currentData != null)
        {
            currentData.unlockedAchievements.Clear(); //Clear memory of comepleted achievements
        }

        DataPersistenceManager.Instance.SaveGame(); //Save the resetted game data
        DataPersistenceManager.Instance.LoadGame(); //Load data to scripts that need it

        UpdateProfileDisplay();
        UpdateAchievementDisplay();

        //Close the confirmation popup
        deleteConfirmationPopup.SetActive(false);
    }

    public void CancelResetProfile()
    {
        deleteConfirmationPopup.SetActive(false);
    }
    #endregion

    #region Colour Schemes

    public void setColourScheme(int selectedIndex)
    {
        GameData currentGameData = DataPersistenceManager.Instance.GetCurrentGameData();
        
        currentGameData.chosenColourScheme = selectedIndex;
        DataPersistenceManager.Instance.SaveGame();

        for (int i = 0; i < colourSchemePreviews.Length; i++)
        {
            //Enable only the relevant image
            colourSchemePreviews[i].enabled = (i == selectedIndex);
        }
    }

    private void ActivateColourSchemeSelection()
    {
        GameData currentGameData = DataPersistenceManager.Instance.GetCurrentGameData();
        int selectedIndex = currentGameData.chosenColourScheme;

        //Ensure valid index
        if (selectedIndex >= 0 && selectedIndex < colourSchemeToggles.Length)
        {
            //Set the correct toggle on
            colourSchemeToggles[selectedIndex].isOn = true;

            //Set the correct preview image
            for (int i = 0; i < colourSchemePreviews.Length; i++)
            {
                colourSchemePreviews[i].enabled = (i == selectedIndex);
            }
        }
    }

    #endregion

}