using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUIManager : MonoBehaviour
{
    [SerializeField] private Image ability1Image;
    [SerializeField] private Image ability2Image;
    [SerializeField] private TextMeshProUGUI ability1CooldownText;
    [SerializeField] private TextMeshProUGUI ability2CooldownText;

    private LoadoutManager loadoutManager;
    private Dictionary<Ability, float> cooldownTimers = new Dictionary<Ability, float>();

    void Start()
    {
        loadoutManager = LoadoutManager.Instance;

        UpdateAbilityUI();
    }

    void Update()
    {
        UpdateCooldownUI();
    }

    public void UpdateAbilityUI()
    {
        Ability ability1 = loadoutManager.selectedAbilities[0];
        Ability ability2 = loadoutManager.selectedAbilities[1];

        if (ability1 != null)
        {
            ability1Image.sprite = ability1.icon;
            ability1Image.enabled = true;
            ability1CooldownText.text = "";
        }
        else
        {
            ability1Image.enabled = false;
            ability1CooldownText.text = "";
        }

        
        if (ability2 != null)
        {
            ability2Image.sprite = ability2.icon; 
            ability2Image.enabled = true;   
            ability2CooldownText.text = ""; 
        }
        else
        {
            ability2Image.enabled = false;
            ability2CooldownText.text = ""; 
        }

        if (ability1 != null) cooldownTimers[ability1] = 0f;
        if (ability2 != null) cooldownTimers[ability2] = 0f;
    }

    private void UpdateCooldownUI()
    {
        foreach (var ability in cooldownTimers.Keys)
        {
            if (cooldownTimers[ability] > Time.time)
            {
                float remainingTime = cooldownTimers[ability] - Time.time;

                if (ability == loadoutManager.selectedAbilities[0])
                {
                    ability1CooldownText.text = Mathf.CeilToInt(remainingTime).ToString();
                    ability1Image.color = new Color(1, 1, 1, 0.5f);
                }
                else if (ability == loadoutManager.selectedAbilities[1])
                {
                    ability2CooldownText.text = Mathf.CeilToInt(remainingTime).ToString();
                    ability2Image.color = new Color(1, 1, 1, 0.5f);
                }
            }
            else
            {
                if (ability == loadoutManager.selectedAbilities[0])
                {
                    ability1CooldownText.text = "";
                    ability1Image.color = Color.white;
                }
                else if (ability == loadoutManager.selectedAbilities[1])
                {
                    ability2CooldownText.text = "";
                    ability2Image.color = Color.white;
                }
            }
        }
    }

    public void StartCooldown(Ability ability)
    {
        if (!cooldownTimers.ContainsKey(ability))
        {
            cooldownTimers[ability] = 0f;
        }

        cooldownTimers[ability] = Time.time + ability.cooldownTime;
    }

    public void ResetCooldownUI()
    {
        cooldownTimers.Clear();
        ability1CooldownText.text = "";
        ability2CooldownText.text = "";
        ability1Image.color = Color.white;
        ability2Image.color = Color.white;
    }

}


