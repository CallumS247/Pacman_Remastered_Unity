using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyController;

public class AbilityManager : MonoBehaviour
{
    private Dictionary<Ability, float> abilityCooldowns = new Dictionary<Ability, float>();

    public GameManager gameManager;
    public MovementController movementController;
    public bool isSpeedBoostActive;

    public GameObject explosionPrefab;
    private Vector3 explosionPosition;

    public Transform player;
    public GameObject teleportNodes;

    public GameObject extraLifeAlert;

    [Header("AudioEffects")]
    public AudioSource nukeAudio;
    public AudioSource freezeAudio;
    public AudioSource teleportAudio;
    public AudioSource speedBoostAudio;
    public AudioSource doublePointsAudio;
    public AudioSource extraLifeAudio;
    public AudioSource frightenAudio;

    private void Start()
    {
        extraLifeAlert.SetActive(false);
    }

    public void UseAbility(Ability ability)
    {
        if (abilityCooldowns.ContainsKey(ability) && Time.time < abilityCooldowns[ability])
        {
            Debug.Log("Ability on cooldown!");
            return;
        }

        switch (ability.abilityType)
        {
            case AbilityType.SpeedBoost:
                StartCoroutine(SpeedBoost());
                break;
            case AbilityType.Freeze:
                StartCoroutine(Freeze());
                break;
            case AbilityType.DoublePoints:
                StartCoroutine(DoublePoints());
                break;
            case AbilityType.Frighten:
                StartCoroutine(Frighten());
                break;
            case AbilityType.Nuke:
                Nuke();
                break;
            case AbilityType.Teleport:
                Teleport();
                break;
            case AbilityType.ExtraLife:
                StartCoroutine(ExtraLife());
                break;
        }

        abilityCooldowns[ability] = Time.time + ability.cooldownTime;
        FindObjectOfType<AbilityUIManager>().StartCooldown(ability);
    }

    public void ResetAbilityCooldowns()
    {
        abilityCooldowns.Clear();
        FindObjectOfType<AbilityUIManager>().ResetCooldownUI();
    }

    private IEnumerator SpeedBoost()
    {
        speedBoostAudio.Play();
        isSpeedBoostActive = true;
        MovementController[] controllers = FindObjectsOfType<MovementController>();
        foreach (MovementController controller in controllers)
        {
            if (controller.CompareTag("Player") && isSpeedBoostActive) //Make sure only the player is affected
            {
                float originalSpeed = controller.speed;
                controller.SetSpeed(originalSpeed * 1.75f);

                yield return new WaitForSeconds(4f);

                controller.SetSpeed(originalSpeed);
                isSpeedBoostActive = false;
            }
        }
    }

    private IEnumerator Freeze()
    {
        freezeAudio.Play();
        MovementController[] controllers = FindObjectsOfType<MovementController>();
        
        //Loop through all controllers to freeze all ghosts at once
        foreach (MovementController controller in controllers)
        {
            if (controller.CompareTag("Enemy"))
            {
                EnemyController enemyController = controller.GetComponent<EnemyController>();
                if (enemyController.ghostState != GhostNodeStates.startNode &&
                    enemyController.ghostState != GhostNodeStates.leftNode &&
                    enemyController.ghostState != GhostNodeStates.rightNode &&
                    enemyController.ghostState != GhostNodeStates.centerNode &&
                    enemyController.ghostState != GhostNodeStates.respawning)
                {
                    enemyController.isFrozen = true; //Freeze the ghost
                }
            }
        }

        yield return new WaitForSeconds(5f);

        //After freeze duration, restore original speed to all ghosts simultaneously
        foreach (MovementController controller in controllers)
        {
            if (controller.CompareTag("Enemy"))
            {
                EnemyController enemyController = controller.GetComponent<EnemyController>();
                if (enemyController.ghostState != GhostNodeStates.startNode &&
                    enemyController.ghostState != GhostNodeStates.leftNode &&
                    enemyController.ghostState != GhostNodeStates.rightNode &&
                    enemyController.ghostState != GhostNodeStates.centerNode &&
                    enemyController.ghostState != GhostNodeStates.respawning)
                {
                    enemyController.isFrozen = false;
                }
            }
        }
    }

    private IEnumerator DoublePoints()
    {
        doublePointsAudio.Play();
        gameManager.SetScoreMultiplier(2f);
        
        yield return new WaitForSeconds(10f);

        gameManager.SetScoreMultiplier(1f);
    }

    private IEnumerator Frighten()
    {
        frightenAudio.Play();
        gameManager.isPowerPelletRunning = true;
        gameManager.currentPowerPelletTime = 0;

        gameManager.redGhostController.isFrightened = true;
        gameManager.pinkGhostController.isFrightened = true;
        gameManager.blueGhostController.isFrightened = true;
        gameManager.orangeGhostController.isFrightened = true;

        yield return new WaitForSeconds(8f);

        gameManager.isPowerPelletRunning = false;
        gameManager.redGhostController.isFrightened = false;
        gameManager.pinkGhostController.isFrightened = false;
        gameManager.blueGhostController.isFrightened = false;
        gameManager.orangeGhostController.isFrightened = false;
    }

    private void Nuke()
    {
        nukeAudio.Play();
        explosionPosition = gameManager.PacMan.transform.position;
        GameObject explosion = Instantiate(explosionPrefab, explosionPosition, Quaternion.identity);

        // Destroy after animation duration
        Destroy(explosion, 1f);

        foreach (EnemyController ghost in FindObjectsOfType<EnemyController>())
        {
            if (ghost.ghostState == GhostNodeStates.movingNodes)
            {
                ghost.ghostState = GhostNodeStates.respawning;
                gameManager.AddtoScore(1000);
            }
        }
    }

    private void Teleport()
    {
        Transform[] allNodes = teleportNodes.GetComponentsInChildren<Transform>();
        List<Transform> validNodes = new List<Transform>();

        foreach (Transform node in allNodes)
        {
            if (node.parent == teleportNodes.transform)  
            {
                validNodes.Add(node);
            }
        }

        if (validNodes.Count == 0) return;

        Transform randomNode;
        bool isSafe;
        float safeDistance = 1.5f;
        do
        {
            randomNode = validNodes[Random.Range(1, validNodes.Count)];
            foreach (EnemyController ghost in FindObjectsOfType<EnemyController>())
            {
                float distance = Vector2.Distance(randomNode.position, ghost.transform.position);
                if (distance < safeDistance)
                {
                    isSafe = false;
                }
            }
            isSafe = true;
        } while (!isSafe || randomNode.position == player.position); //Avoid teleporting to the same position

        if (isSafe)
        {
            teleportAudio.Play();
            StartCoroutine(SlowTime());
            player.position = randomNode.position;
            movementController.currentNode = randomNode.gameObject;
            movementController.direction = "";
        }
    }

    private IEnumerator SlowTime()
    {
        Time.timeScale = 0.4f;
        yield return new WaitForSecondsRealtime(2f);
        Time.timeScale = 1f;
    }

    public IEnumerator ExtraLife()
    {
        extraLifeAudio.Play();
        int playerLives = gameManager.playerLives;
        gameManager.setLives(playerLives += 1);
        extraLifeAlert.SetActive(true);
        yield return new WaitForSeconds(1f);
        extraLifeAlert.SetActive(false);
    }
}

