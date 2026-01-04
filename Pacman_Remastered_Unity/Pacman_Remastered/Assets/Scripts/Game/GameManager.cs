using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, InterfaceDPM
{
    public GameObject PacMan;

    [Header("Audio")]
    private int currentMunch = 0;
    public AudioSource siren;
    public AudioSource munch1;
    public AudioSource munch2;
    public AudioSource startAudio;
    public AudioSource deathAudio;
    public AudioSource respawnAudio;
    public AudioSource ghostEatenAudio;
    public AudioSource levelupAudio;
    public AudioSource fruitEatenAudio;

    [Header("Power Pellet")]
    public AudioSource powerPelletAudio;
    public bool isPowerPelletRunning = false;
    public float currentPowerPelletTime;
    public float powerPelletTimer = 8f;
    private int powerPelletMultiplier = 1;

    [Header("UI/Text")]
    public GameObject levelUpPopupPrefab;
    public Transform LvlUpPopupParent;
    private int score;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI roundText;
    public Image BlkBackground;
    public GameObject pauseMenu;

    [Header("Nodes")]
    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeCentre;
    public GameObject ghostNodeStart;
    public GameObject leftWarpNode;
    public GameObject rightWarpNode;
    public List<nodeController> nodeControllers = new List<nodeController>();

    [Header("Ghosts/Controllers")]
    public GameObject redGhost;
    public GameObject pinkGhost;
    public GameObject blueGhost;
    public GameObject orangeGhost;
    public EnemyController redGhostController;
    public EnemyController pinkGhostController;
    public EnemyController blueGhostController;
    public EnemyController orangeGhostController;

    [Header("Stats/Info")]
    private int totalPellets;
    private int pelletsRemaining;
    private int pelletsEatenInLife;
    private int highestScore = 0;
    private int highestRound = 0;
    private int totalPelletsEaten = 0;
    private int totalGhostsEaten = 0;
    private int totalDeaths = 0;
    private int LevelsCleared = 0;
    private int abilitiesUsed = 0;
    private int fruitEaten = 0;
    private int chosenColourScheme = 0;
    private bool hadDeathOnLevel = false;
    public bool gameIsRunning;
    private bool newGame;
    private bool clearedLevel;
    public int playerLives;
    private int currentLevel;
    public bool gameIsPaused;
    private bool isRemasteredMode;
    private int scoreForExtraLife = 20000;

    public enum GhostMode
    { 
        Chase,
        Scatter
    }

    [Header("Ghost behaviour")]
    public GhostMode currentGhostMode;
    private int[] ghostModeTimers = new int[] {7,20,7,20,5,20,5};
    private int ghostModeTimerIndex;
    private float ghostModeTimer;
    private bool runningTimer;
    private bool completedTimer;

    [Header("Abilities")]
    public AbilityManager abilityManager;
    public AbilityUIManager abilityUIManager;
    private float scoreMultiplier = 1f;

    [Header("Hazards")]
    public HazardManager hazardManager;
    public PlayerController playerController;
    public bool hazardActive = false;
    public bool isInvincible = false;

    [Header("Fruit Spawn")]
    public GameObject cherryPrefab;
    public GameObject SpawnNodes;
    private float fruitSpawnInterval = 7f;
    private float fruitLifetime = 10f;
    private List<Transform> validNodes = new List<Transform>();

    [Header("Colour Schemes")]
    [SerializeField] private PostProcessVolume[] colourSchemes;

    void Awake()
    {
        newGame = true;
        gameIsPaused = false;
        clearedLevel = false;

        BlkBackground.enabled = false;
        PostProcessVolume postProcessVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();
        postProcessVolume.enabled = false;

        redGhostController = redGhost.GetComponent<EnemyController>();
        pinkGhostController = pinkGhost.GetComponent<EnemyController>();
        blueGhostController = blueGhost.GetComponent<EnemyController>();
        orangeGhostController = orangeGhost.GetComponent<EnemyController>();
        
        ghostNodeStart.GetComponent<nodeController>().isGhostStartNode = true;
        StartCoroutine(Setup());
    }

    void Start()
    {
        for (int i = 0; i < colourSchemes.Length; i++)
        {
            //Enable only the chosen scheme
            colourSchemes[i].enabled = (i == chosenColourScheme);
        }

        isRemasteredMode = SceneManager.GetActiveScene().name == "RMGame1" ||
                           SceneManager.GetActiveScene().name == "RMGame2";

        if (isRemasteredMode && hazardActive == false)
        {
            hazardManager.FogOfWarEffect.SetActive(false);
            hazardManager.ActivateRandomHazard();
            hazardActive = true;
            
        }

        GetSpawnNodes();
        StartCoroutine(SpawnCherries());
    }

    #region Load and Save game data
    public void LoadData(GameData data)
    {
        this.highestScore = data.highestScore;
        this.highestRound = data.highestRound;
        this.totalGhostsEaten = data.totalGhostsEaten;
        this.totalPelletsEaten = data.totalPelletsEaten;
        this.totalDeaths = data.totalDeaths;
        this.LevelsCleared = data.LevelsCleared;
        this.abilitiesUsed = data.abilitiesUsed;
        this.fruitEaten = data.fruitEaten;
        this.chosenColourScheme = data.chosenColourScheme;
    }

    public void SaveData(GameData data)
    {
        data.highestScore = this.highestScore;
        data.highestRound = this.highestRound;
        data.totalPelletsEaten = this.totalPelletsEaten;
        data.totalGhostsEaten = this.totalGhostsEaten;
        data.totalDeaths = this.totalDeaths;
        data.LevelsCleared = this.LevelsCleared;
        data.abilitiesUsed = this.abilitiesUsed;
        data.fruitEaten = this.fruitEaten;
    }
    #endregion

    void Update()
    {
        if (!gameIsRunning || gameIsPaused)
        {
            return;
        }

        #region Pausing
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            PostProcessVolume postProcessVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();
            postProcessVolume.enabled = !postProcessVolume.enabled;
            StartCoroutine(pauseGame());
        }

        if (gameIsPaused)
        {
            PauseAllAudios();
        }
        #endregion

        #region Ghost respawn
        if (redGhostController.ghostState == EnemyController.GhostNodeStates.respawning
            || pinkGhostController.ghostState == EnemyController.GhostNodeStates.respawning
            || blueGhostController.ghostState == EnemyController.GhostNodeStates.respawning
            || orangeGhostController.ghostState == EnemyController.GhostNodeStates.respawning)
        {
            if (!respawnAudio.isPlaying)
            {
                if (!gameIsPaused)
                {
                    respawnAudio.Play();
                }
            }
        }
        else
        {
            if (respawnAudio.isPlaying)
            {
                respawnAudio.Stop();
            }
        }
        #endregion

        #region Ghost behaviour change
        if (!completedTimer && runningTimer)
        { //Go through timer index alternating between chase and scatter
            ghostModeTimer += Time.deltaTime;
            if (ghostModeTimer >= ghostModeTimers[ghostModeTimerIndex])
            {
                ghostModeTimer = 0;
                ghostModeTimerIndex++;
                if (currentGhostMode == GhostMode.Chase)
                {
                    currentGhostMode = GhostMode.Scatter;
                }
                else
                {
                    currentGhostMode = GhostMode.Chase;
                }

                if (ghostModeTimerIndex == ghostModeTimers.Length)
                {
                    completedTimer = true;
                    runningTimer = false;
                    currentGhostMode = GhostMode.Chase;
                }
            }
        }
        #endregion

        #region Power pellet
        if (isPowerPelletRunning)
        {
            currentPowerPelletTime += Time.deltaTime;
            if (currentPowerPelletTime >= powerPelletTimer)
            {
                isPowerPelletRunning = false;
                currentPowerPelletTime = 0;
                powerPelletAudio.Stop();
                siren.Play();
                powerPelletMultiplier = 1;
            }
        }
        #endregion

        #region Ability Usage
        if (isRemasteredMode)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                UseAbilitySlot(0); //Q = Slot 1
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                UseAbilitySlot(1); //E = Slot 2
            }
        }
        #endregion
    }

    //Update pellet counts
    public void gotPelletFromNodeController(nodeController nodeController)
    {
        nodeControllers.Add(nodeController);
        totalPellets++;
        pelletsRemaining++;
    }

    public IEnumerator Setup()
    {
        ghostModeTimerIndex = 0;
        ghostModeTimer = 0;
        completedTimer = false;
        runningTimer = true;
        gameOverText.enabled = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;

        float waitTimer = 1f;

        if (isRemasteredMode)
        {
            abilityManager.isSpeedBoostActive = false;
            isInvincible = true; //Invincibilty to stop death on spawn

            if (hazardActive == false)
            {
                hazardManager.ActivateRandomHazard();
                hazardActive = true;
            }
            if (hazardManager.countdownFailed)
            {
                //Countdown fail acts as new round
                startAudio.Play();
                pelletsRemaining = totalPellets;
                waitTimer = 4;
                for (int i = 0; i < nodeControllers.Count; i++)
                {
                    nodeControllers[i].RespawnPellet();
                }
                //Reset countdown fail check
                hazardManager.countdownFailed = false;
            }
        }
        
        //Player clears a level
        if (clearedLevel)
        {
            //Give bonus level points for beating a level
            GameData gameData = DataPersistenceManager.Instance.GetCurrentGameData();
            AddtoScore(1500);
            CheckForLevelUp(gameData);

            //Activate background
            BlkBackground.enabled = true;
            yield return new WaitForSeconds(0.1f);
            startAudio.Play();
        }

        BlkBackground.enabled = false;
        pelletsEatenInLife = 0;
        currentGhostMode = GhostMode.Scatter;
        gameIsRunning = false;
        currentMunch = 0;

        //Respawn pellets if new level or level is cleared
        if (clearedLevel || newGame)
        {
            pelletsRemaining = totalPellets;
            waitTimer = 4;
            for (int i = 0; i < nodeControllers.Count; i++)
            {
                nodeControllers[i].RespawnPellet();
            }
        }
        
        //Player starts a new game
        if (newGame)
        {
            startAudio.Play();
            score = 0;
            scoreText.text = "Score: " + score.ToString();
            setLives(3);
            currentLevel = 1;
            roundText.text = "Round: " + currentLevel.ToString();
        }

        //Reset player and ghosts
        PacMan.GetComponent<PlayerController>().Setup();
        redGhostController.Setup();
        pinkGhostController.Setup(); 
        blueGhostController.Setup();
        orangeGhostController.Setup();

        newGame = false;
        clearedLevel = false;

        yield return new WaitForSeconds(waitTimer);

        StartGame();

        //Prevent player from being instantly killed by dmgZone
        yield return new WaitForSeconds(2);
        isInvincible = false;

    }

    #region Player Info - Lives, Score, Level
    public void setLives(int newlives)
    {
        playerLives = newlives;
        livesText.text = "Lives: " + playerLives.ToString();
    }

    public void SetScoreMultiplier(float multiplier)
    {
        scoreMultiplier = multiplier;
    }

    public void AddtoScore(int amount)
    {
        int finalScore = Mathf.RoundToInt(amount * scoreMultiplier);
        score += finalScore;
        scoreText.text = "Score: " + score.ToString();
        //Player level
        GameData gameData = DataPersistenceManager.Instance.GetCurrentGameData();
        if (gameData != null)
        {
            //Update level points and check for level up
            gameData.CurrentLevelPoints += finalScore;
            CheckForLevelUp(gameData);
            //Check for completed achievements
            CheckAchievementsAndSave();
        }

        //Extra life every 20k score
        if (score >= scoreForExtraLife)
        {
            StartCoroutine(abilityManager.ExtraLife());
            scoreForExtraLife += 20000;
        }
        
    }

    public void CheckForLevelUp(GameData gameData)
    {
        while (gameData.CurrentLevelPoints >= gameData.PointsToNextLevel)
        {
            //Deduct points required for level-up
            gameData.CurrentLevelPoints -= gameData.PointsToNextLevel;

            //Level up and adjust points required for next level
            gameData.PlayerLevel++;
            gameData.PointsToNextLevel += 5000;
            levelupAudio.Play();
            GameObject popup = Instantiate(levelUpPopupPrefab, LvlUpPopupParent);
            int playerLvl = gameData.PlayerLevel;
            TextMeshProUGUI levelText = popup.transform.Find("LevelUnlocked").GetComponent<TextMeshProUGUI>();
            if (levelText != null)
            {
                levelText.text = "Level " + playerLvl.ToString() + " Unlocked";
            }
            Destroy(popup, 3f);
        }
    }

    private void CheckAchievementsAndSave()
    {
        DataPersistenceManager.Instance.SaveGame();
        AchievementManager.AchvManager.CheckAchievements(DataPersistenceManager.Instance.GetCurrentGameData());
    }
    #endregion

    #region Start,Stop,Play,Pause
    void StartGame()
    {
        gameIsRunning = true;
        siren.Play();
        PostProcessVolume postProcessVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();
        postProcessVolume.enabled = false;
    }

    void StopGame()
    {
        gameIsRunning = false;
        siren.Stop();
        powerPelletAudio.Stop();
        respawnAudio.Stop();
        PacMan.GetComponent<PlayerController>().Stop();
    }

    public IEnumerator pauseGame(float timeToPause = -1f)
    {
        gameIsPaused = true;
        gameIsRunning = false;
        Time.timeScale = 0;

        if (timeToPause < 0f)
        { //Display pause menu when player presses P or Esc
            pauseMenu.SetActive(true);
        }

        if (timeToPause > 0f)
        { //Activates only when player eats a ghost
            yield return new WaitForSecondsRealtime(timeToPause);
            ResumeGame();
        }
    }

    public void ResumeGame()
    {
        gameIsPaused = false;
        gameIsRunning = true;
        Time.timeScale = 1;

        //Unpause all audios if running 
        UnPauseAllAudios();

        //Deactivate menu and camera blur
        pauseMenu.SetActive(false);
        PostProcessVolume postProcessVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();
        postProcessVolume.enabled = false;
    }

    public void ExitToMenu()
    {
        //Update high score/round stat if necessary
        if (score > highestScore)
        {
            highestScore = score;
        }
        if (currentLevel > highestRound)
        {
            highestRound = currentLevel;
        }
        hazardActive = false;
        DataPersistenceManager.Instance.SaveGame();
        gameIsPaused = false;
        SceneManager.LoadScene("MainMenu");
    }
    #endregion

    #region Eating pellets, ghosts, players, fruit
    public IEnumerator eatenPellet(nodeController nodeController)
    { //Alternate between munch sounds
        if (currentMunch == 0)
        {
            munch1.Play();
            currentMunch = 1;
        }
        else if (currentMunch == 1)
        {
            munch2.Play();
            currentMunch = 0;
        }

        pelletsRemaining--;
        pelletsEatenInLife++;
        totalPelletsEaten++;

        int requiredBluePellets = 0;
        int requiredOrangePellets = 0;

        if (hadDeathOnLevel)
        { //Pellets required to spawn ghosts in if player has respawned
            requiredBluePellets = 12;
            requiredOrangePellets = 32;
        }
        else
        { //Requird pellets to spawn ghosts in by default
            requiredBluePellets = 30;
            requiredOrangePellets = 60;
        }

        if (pelletsEatenInLife >= requiredBluePellets && !blueGhost.GetComponent<EnemyController>().leftHomeBefore)
        {
            blueGhost.GetComponent<EnemyController>().readyToLeaveHome = true; 
        }

        if (pelletsEatenInLife >= requiredOrangePellets && !orangeGhost.GetComponent<EnemyController>().leftHomeBefore)
        {
            orangeGhost.GetComponent<EnemyController>().readyToLeaveHome = true;
        }

        CheckAchievementsAndSave();

        AddtoScore(10);

        //Check if theres pellets left to end level
        if (pelletsRemaining == 0)
        {
            if (isRemasteredMode)
            {
                DisableAllHazards();
            }
            currentLevel++;
            roundText.text = "Round: " + currentLevel.ToString();
            LevelsCleared++;
            clearedLevel = true;
            CheckAchievementsAndSave();
            StopGame();
            yield return new WaitForSeconds(1);
            StartCoroutine(Setup());
        }
        
        //Power Pellet
        if (nodeController.isPowerPellet)
        {
            siren.Stop();
            powerPelletAudio.Play();
            isPowerPelletRunning = true;
            currentPowerPelletTime = 0;

            if (gameIsPaused)
            {
                powerPelletAudio.Pause();
            }

            redGhostController.SetFrightened(true);
            pinkGhostController.SetFrightened(true);
            blueGhostController.SetFrightened(true);
            orangeGhostController.SetFrightened(true);
        }
    }

    public void ghostEaten()
    {
        totalGhostsEaten++;

        CheckAchievementsAndSave();

        ghostEatenAudio.Play();
        AddtoScore(400 * powerPelletMultiplier);
        powerPelletMultiplier++;
        StartCoroutine(pauseGame(1));
    }

    public IEnumerator playerEaten()
    {
        hadDeathOnLevel = true;
        totalDeaths++;

        CheckAchievementsAndSave();

        StopGame();
        yield return new WaitForSeconds(1);

        redGhostController.SetVisible(false);
        pinkGhostController.SetVisible(false);
        blueGhostController.SetVisible(false);
        orangeGhostController.SetVisible(false);

        PacMan.GetComponent<PlayerController>().Death();
        deathAudio.Play();
        yield return new WaitForSeconds(3);

        setLives(playerLives - 1);
        //Player runs out of lives
        if (playerLives <= 0)
        {
            if (isRemasteredMode)
            {
                DisableAllHazards();
                abilityManager.ResetAbilityCooldowns();
            }
            newGame = true;
            gameOverText.enabled = true;
            //Update high score/round stat if neccessary
            if (score > highestScore)
            {
                highestScore = score;
            }
            if (currentLevel > highestRound)
            {
                highestRound = currentLevel;
            }
            yield return new WaitForSeconds(3);
        }

        StartCoroutine(Setup());
    }

    public void FruitEaten()
    {
        fruitEatenAudio.Play();
        fruitEaten++;

        CheckAchievementsAndSave();

        AddtoScore(500 * powerPelletMultiplier);
    }
    #endregion

    #region Abilities and Hazards
    public void UseAbilitySlot(int slotIndex)
    {
        if (!isRemasteredMode) return; //Prevent ability usage in Classic Mode

        Ability selectedAbility = LoadoutManager.Instance.selectedAbilities[slotIndex];

        if (selectedAbility != null)
        {
            if (!hazardManager.isPowerDrained)
            {
                abilityManager.UseAbility(selectedAbility);
                abilitiesUsed++;
                CheckAchievementsAndSave();
            }
            else
            {
                Debug.Log("Cannot use abilities while power drained.");
            }
        }
    }

    private void DisableAllHazards()
    {
        hazardActive = false;

        //Disable invert controls
        playerController.invertedControls = false;

        //Disable Ghost Immunity
        redGhostController.ImmuneHazardOn = false;
        blueGhostController.ImmuneHazardOn = false;
        orangeGhostController.ImmuneHazardOn = false;
        pinkGhostController.ImmuneHazardOn = false;

        //Disable DmgZones respawning
        hazardManager.DestroyDmgZones();
        hazardManager.respawningZones = false;

        //Disable Nearsight
        if (hazardManager.nearsightMask != null)
        {
            Destroy(hazardManager.nearsightMask);
        }
        if (hazardManager.FogOfWarEffect != null)
        {
            hazardManager.FogOfWarEffect.SetActive(false);
        }
        hazardManager.nearsightActive = false;

        //Disable Power Drain
        hazardManager.powerDrainON = false;
        hazardManager.ability1Disabled.enabled = false;
        hazardManager.ability2Disabled.enabled = false;

        //Disable Countdown
        hazardManager.isCountdownActive = false;
    }
    #endregion

    #region Audio
    private void PauseAllAudios()
    {
        siren.Pause();
        deathAudio.Pause();
        powerPelletAudio.Pause();
        ghostEatenAudio.Pause();
        munch1.Pause();
        munch2.Pause();
        startAudio.Pause();
        respawnAudio.Pause();
        levelupAudio.Pause();
        fruitEatenAudio.Pause();

        if (isRemasteredMode)
        {
            abilityManager.frightenAudio.Pause();
            abilityManager.freezeAudio.Pause();
            abilityManager.speedBoostAudio.Pause();
            abilityManager.doublePointsAudio.Pause();
            abilityManager.extraLifeAudio.Pause();
            abilityManager.nukeAudio.Pause();
            abilityManager.teleportAudio.Pause();
            hazardManager.warningAudio.Pause();
            hazardManager.pdWarningAudio.Pause();
            hazardManager.countdownWarningAudio.Pause();
        }
    }

    private void UnPauseAllAudios()
    {
        siren.UnPause();
        deathAudio.UnPause();
        powerPelletAudio.UnPause();
        ghostEatenAudio.UnPause();
        munch1.UnPause();
        munch2.UnPause();
        startAudio.UnPause();
        respawnAudio.UnPause();
        levelupAudio.UnPause();
        fruitEatenAudio.UnPause();

        if (isRemasteredMode)
        {
            abilityManager.frightenAudio.UnPause();
            abilityManager.freezeAudio.UnPause();
            abilityManager.speedBoostAudio.UnPause();
            abilityManager.doublePointsAudio.UnPause();
            abilityManager.extraLifeAudio.UnPause();
            abilityManager.nukeAudio.UnPause();
            abilityManager.teleportAudio.UnPause();
            hazardManager.warningAudio.UnPause();
            hazardManager.pdWarningAudio.UnPause();
            hazardManager.countdownWarningAudio.UnPause();
        }
    }
    #endregion

    #region Fruit Spawning
    private void GetSpawnNodes()
    {
        foreach (Transform node in SpawnNodes.GetComponentsInChildren<Transform>())
        {
            if (node.parent == SpawnNodes.transform)
            {
                validNodes.Add(node);
            }
        }
    }

    private Transform GetRandomSpawnNode()
    {
        if (validNodes.Count == 0) return null;
        return validNodes[Random.Range(0, validNodes.Count)];
    }

    public IEnumerator SpawnCherries()
    {
        while (!gameIsPaused)
        {
            yield return new WaitForSeconds(fruitSpawnInterval);
            Transform spawnPoint = GetRandomSpawnNode();
            if (spawnPoint != null)
            {
                GameObject cherry = Instantiate(cherryPrefab, spawnPoint.position, Quaternion.identity);
                Cherry cherryScript = cherry.GetComponent<Cherry>();

                float elapsedTime = 0f;
                while (elapsedTime < fruitLifetime)
                {
                    if (cherryScript.IsEaten) //Check if cherry has been eaten
                    {
                        Destroy(cherry);
                        break;
                    }

                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                if (!cherryScript.IsEaten) //If it wasn't eaten, destroy after lifetime remaining
                {
                    yield return new WaitForSeconds(fruitLifetime - elapsedTime);
                    Destroy(cherry);
                }

                
                
            }
                
        }
    }
    #endregion

}