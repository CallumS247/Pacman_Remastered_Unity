using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HazardManager : MonoBehaviour
{
    public GameManager gameManager;
    public PlayerController playerController;
    
    public List<Hazard> hazards;
    private Hazard activeHazard;
    [SerializeField] private Image currentHazardLogo;
    [SerializeField] private TextMeshProUGUI currentHazardText;

    [Header("DmgZones")]
    public GameObject damageZonePrefab;
    private int maxDmgZones = 3;
    private List<GameObject> activeDmgZones = new List<GameObject>();
    public bool respawningZones = false;
    public AudioSource warningAudio;

    [Header("Nearsight")]
    public GameObject nearsightMask;
    [SerializeField] private GameObject nearsightMaskPrefab;
    public GameObject FogOfWarEffect;
    public bool nearsightActive = false;

    [Header("Countdown")]
    public TextMeshProUGUI countdownText;
    private float countdownTime = 80f;
    private float currentTime;
    public bool isCountdownActive;
    public AudioSource countdownWarningAudio;
    public bool countdownFailed = false;

    [Header("Power Drain")]
    public GameObject[] powerDrainZones;
    private GameObject activePDZone;
    public bool isPowerDrained = false;
    public bool powerDrainON = false;
    public Image ability1Disabled;
    public Image ability2Disabled;
    public AudioSource pdWarningAudio;

    private void Start()
    {
        ability1Disabled.enabled = false;
        ability2Disabled.enabled = false;
    }

    public void ActivateRandomHazard()
    {
        if (hazards.Count == 0) return;

        //Ensure DmgZones despawned
        respawningZones = false;
        DestroyDmgZones();

        //Ensure Power Drain Zones despawned
        powerDrainON = false;
        if (activePDZone != null)
        {
            Destroy(activePDZone);
        }

        //Disable Countdown text
        countdownText.text = "";

        //Select a random hazard
        activeHazard = hazards[Random.Range(0, hazards.Count)];
        Debug.Log($"Hazard Activated: {activeHazard.hazardName}");

        switch (activeHazard.hazardType)
        {
            case HazardType.DmgZone:
                StartCoroutine(SpawnDmgZones());
                break;
            case HazardType.InvertControls:
                playerController.invertedControls = true;
                break;
            case HazardType.GhostImmune:
                gameManager.redGhostController.ImmuneHazardOn = true;
                gameManager.blueGhostController.ImmuneHazardOn = true;
                gameManager.orangeGhostController.ImmuneHazardOn = true;
                gameManager.pinkGhostController.ImmuneHazardOn = true;
                break;
            case HazardType.Nearsight:
                Nearsight(); 
                break;
            case HazardType.Countdown:
                StartCoroutine(StartCountdown());
                break;
            case HazardType.PowerDrain:
                StartCoroutine(SpawnPowerDrains());
                break;
        }

        //Update hazard UI
        currentHazardLogo.sprite = activeHazard.icon;
        currentHazardText.text = activeHazard.name;
    }

    #region DmgZone Hazard
    private IEnumerator SpawnDmgZones()
    {
        respawningZones = true;
        
        while (respawningZones)
        {
            if (!gameManager.gameIsPaused)
            {
                yield return new WaitForSeconds(6f);
                if (!respawningZones) yield break;
                warningAudio.Play();
                yield return new WaitForSeconds(2f);
                if (!respawningZones) yield break;
                //Destroy old zones
                DestroyDmgZones();
                //Spawn in 3 new zones in different positions
                for (int i = 0; i < maxDmgZones; i++)
                {
                    Vector2 spawnPosition = GetRandomPosition();
                    GameObject dmgZone = Instantiate(damageZonePrefab, spawnPosition, Quaternion.identity);
                    activeDmgZones.Add(dmgZone);
                }
            }
            else
            {
                //Ensure timers dont go down if game is paused
                yield return null;
            }
        }
    }

    private Vector2 GetRandomPosition()
    {
        Vector2 spawnPosition;
        float minSafeDistance = 1.3f; //Safe zone around player
        int maxAttempts = 10; //Prevent infinite loops
        int attempt = 0;

        do
        {
            spawnPosition = new Vector2(
                Random.Range(-4f, 4f),
                Random.Range(-4.5f, 4.5f)
            );
            attempt++;
        }

        while (Vector2.Distance(spawnPosition, gameManager.PacMan.transform.position) < minSafeDistance && attempt < maxAttempts);

        return spawnPosition;
    }

    public void DestroyDmgZones()
    {
        foreach (GameObject zone in activeDmgZones)
        {
            Destroy(zone);
        }

        activeDmgZones.Clear();
    }
    #endregion

    private void Nearsight()
    {
        if (nearsightActive) return;

        nearsightActive = true;

        if (FogOfWarEffect != null)
        {
            FogOfWarEffect.SetActive(true);
        }

        //Create mask if not already created
        if (nearsightMask == null)
        {
            nearsightMask = Instantiate(nearsightMaskPrefab);
        }

        //Make it a child object of the player
        nearsightMask.transform.SetParent(gameManager.PacMan.transform);
        nearsightMask.transform.localPosition = Vector3.zero;
    }

    private IEnumerator StartCountdown()
    {
        isCountdownActive = true;
        currentTime = countdownTime;
        yield return new WaitForSeconds(4f);
        while (isCountdownActive && currentTime >= 0)
        {
            if (!gameManager.gameIsPaused)
            {
                countdownText.text = Mathf.CeilToInt(currentTime).ToString();

                //Play warning audio at 10 seconds
                if (currentTime == 10)
                {
                    countdownWarningAudio.Play();
                }

                yield return new WaitForSeconds(1f);
                currentTime--;
            }
            else
            {
                //Ensure timers dont go down if game is paused
                yield return null;
            }
        }
        
        if (currentTime <= 0)
        {   //Disable hazard, remove a life
            gameManager.StartCoroutine(gameManager.playerEaten());
            countdownFailed = true;
            isCountdownActive = false;
            gameManager.hazardActive = false;
        }
    }

    private IEnumerator SpawnPowerDrains()
    {
        powerDrainON = true;
        while (powerDrainON)
        {
            if (!gameManager.gameIsPaused)
            {
                yield return new WaitForSeconds(8);
                if (!powerDrainON) yield break;
                pdWarningAudio.Play();
                yield return new WaitForSeconds(2);
                if (!powerDrainON) yield break;
                if (activePDZone != null)
                {
                    Destroy(activePDZone); //Remove existing zone
                }
                //Pick a random zone prefab to replace it
                int randomIndex = Random.Range(0, powerDrainZones.Length);
                activePDZone = Instantiate(powerDrainZones[randomIndex]);
            }
            else
            {
                //Ensure timers dont go down if game is paused
                yield return null;
            }
        }
    }
}