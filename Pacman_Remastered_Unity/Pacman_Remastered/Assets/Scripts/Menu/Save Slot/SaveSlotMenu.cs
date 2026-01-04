using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSlotMenu : MonoBehaviour
{
    private SaveSlot[] saveSlots;

    private void Awake()
    {
        saveSlots = this.GetComponentsInChildren<SaveSlot>();
    }

    private void Start()
    {
        ActivateMenu();
    }

    public void OnSaveSlotClicked(SaveSlot saveSlot)
    {
        //Update selected profile ID 
        DataPersistenceManager.Instance.ChangeSelectedProfileID(saveSlot.GetProfileID());

        //Check if save slot has data already
        Dictionary<string, GameData> profilesGameData = DataPersistenceManager.Instance.GetAllProfileGameData();
        if (profilesGameData.TryGetValue(saveSlot.GetProfileID(), out GameData profileData) && profileData != null)
        {
            //Load data from file
            DataPersistenceManager.Instance.LoadGame();
        }
        else
        {
            //Create new save slot - data is a clean slate
            DataPersistenceManager.Instance.NewGame();
        }
        SceneManager.LoadScene("MainMenu");
    }

    public void ActivateMenu()
    {
        //Load all profiles that exist
        Dictionary<string, GameData> profilesGameData = DataPersistenceManager.Instance.GetAllProfileGameData();

        //Loop through each save slot in the menu and set content
        foreach (SaveSlot saveSlot in saveSlots)
        {
            GameData profileData = null;
            profilesGameData.TryGetValue(saveSlot.GetProfileID(), out profileData);
            saveSlot.SetData(profileData);
        }
    }
}
