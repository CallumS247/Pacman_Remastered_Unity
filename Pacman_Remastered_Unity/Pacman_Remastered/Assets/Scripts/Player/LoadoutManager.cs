using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutManager : MonoBehaviour
{
    public static LoadoutManager Instance;

    public List<Ability> availableAbilities = new List<Ability>();
    public Ability[] selectedAbilities = new Ability[2];

    public List<Button> abilityButtons = new List<Button>();
    public List<Ability> allAbilities = new List<Ability>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UnlockAbility(Ability ability)
    {
        if (!availableAbilities.Contains(ability))
        {
            availableAbilities.Add(ability);
        }
    }

    public void SelectAbility(Ability ability, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= selectedAbilities.Length) return;
        if (!availableAbilities.Contains(ability)) return;

        //Prevent selecting the same ability in both slots
        int otherSlot = (slotIndex == 0) ? 1 : 0;
        if (selectedAbilities[otherSlot] == ability)
        {
            Debug.Log("You cannot select the same ability in both slots!");
            return;
        }

        selectedAbilities[slotIndex] = ability;
        Debug.Log($"Selected {ability.abilityName} in slot {slotIndex + 1}");

        MainMenuController mainMenuController = FindObjectOfType<MainMenuController>();
        if (mainMenuController != null)
        {
            mainMenuController.UpdateSlotUI();
        }
    }

    public void UpdateUnlockedAbilities(int playerLevel)
    {
        foreach (Ability ability in Resources.LoadAll<Ability>("Abilities"))
        {
            if (playerLevel >= ability.requiredLevel && !availableAbilities.Contains(ability))
            {
                UnlockAbility(ability);
            }
        }

        UpdateAbilityButtonsUI(playerLevel);
    }

    public void UpdateAbilityButtonsUI(int playerLevel)
    {
        for (int i = 0; i < abilityButtons.Count; i++)
        {
            Button abilityButton = abilityButtons[i];
            Ability ability = availableAbilities[i]; 

            bool isUnlocked = playerLevel >= ability.requiredLevel;

            //Enable/disable button interaction
            abilityButton.interactable = isUnlocked;

            //Change color (gray if locked, white if unlocked)
            abilityButton.GetComponent<Image>().color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.7f);

            TextMeshProUGUI levelText = abilityButton.transform.Find("RequiredLevelText")?.GetComponent<TextMeshProUGUI>();
            if (levelText != null)
            {
                levelText.text = isUnlocked ? "" : $"Req LVL: {ability.requiredLevel}";
            }
        }
    }

}
