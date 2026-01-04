using UnityEngine;

public class PowerDrainHazard : MonoBehaviour
{
    public HazardManager hazardManager;
    private MovementController playerMovementController;
    private float originalSpeed;

    private void Awake()
    {
        hazardManager = FindObjectOfType<HazardManager>();
        MovementController[] controllers = FindObjectsOfType<MovementController>();
        foreach (MovementController controller in controllers)
        { //Find the controller for the player, filter out ghosts MCs
            if (controller.CompareTag("Player"))
            {
                playerMovementController = controller;
                originalSpeed = playerMovementController.speed;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        { //Slow on entering zone + disable abilities
            hazardManager.isPowerDrained = true;
            playerMovementController.SetSpeed(originalSpeed * 0.9f);
            hazardManager.ability1Disabled.enabled = true;
            hazardManager.ability2Disabled.enabled = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        { //Enable abilities, return to normal speed
            hazardManager.isPowerDrained = false;
            playerMovementController.SetSpeed(originalSpeed);
            hazardManager.ability1Disabled.enabled = false;
            hazardManager.ability2Disabled.enabled = false;
        }
    }
}
