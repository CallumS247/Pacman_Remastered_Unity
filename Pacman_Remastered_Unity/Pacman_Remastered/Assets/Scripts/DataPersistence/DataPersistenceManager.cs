using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;

    private GameData gameData;
    private List<InterfaceDPM> dataPersistenceObjects;
    private FileDataHandler fileDataHandler;

    private string selectedProfileID = "0";

    public static DataPersistenceManager Instance {  get; private set; }

    private void Awake()
    {
        if (Instance != null)
        { //DPM is a singleton - will be carried over between scenes
            Debug.Log("More than one DPM in scene. Duplicate destroyed.");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        //Will store save files in Documents/(Game name)
        string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Pac-Man Remastered");

        this.fileDataHandler = new FileDataHandler(savePath, fileName, useEncryption);
    }

    private void OnEnable()
    { //Subscribe to this method on object enable
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    { //When loading a scene - find all DPO and push player data to them
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    public void ChangeSelectedProfileID(string newProfileID)
    { //Used on save slot menu to select a save file
        this.selectedProfileID = newProfileID;
        LoadGame();
    }

    public void NewGame()
    {
        this.gameData = new GameData();
        SaveGame(); //Ensure blank slate is pushed to all scripts
    }

    public void LoadGame()
    {
        //Load data from data handler
        this.gameData = fileDataHandler.Load(selectedProfileID);

        //If no data is found, do not continue
        if (this.gameData == null)
        {
            Debug.Log("No data found. Save file needs to be created");
            return;
        }

        //Push data to scripts using the interface
        foreach (InterfaceDPM dataPersistenceObj in dataPersistenceObjects)
        { //Each script using the interface will run its respective LoadData method
            dataPersistenceObj.LoadData(gameData);
        }

    }

    public void SaveGame()
    {
        if (this.gameData == null)
        {
            Debug.LogWarning("No data found. Save file needs to be created");
            return;
        }

        //Save game data from all scripts using the interface
        foreach (InterfaceDPM dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(gameData);
        }

        //Save data using data handler
        fileDataHandler.Save(gameData, selectedProfileID);
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private List<InterfaceDPM> FindAllDataPersistenceObjects()
    {
        IEnumerable<InterfaceDPM> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<InterfaceDPM>();

        return new List<InterfaceDPM>(dataPersistenceObjects);
    }

    public Dictionary<string, GameData> GetAllProfileGameData()
    {
        return fileDataHandler.LoadAllProfiles();
    }

    public GameData GetCurrentGameData()
    {
        return this.gameData;
    }

}