using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandler
{
    private string dataDirectoryPath = "";
    private string dataFileName = "";

    private bool useEncryption = false;
    private readonly string encryptionKey = "DfsZC81GfEDqLZlkJWK1a10M6YWc30";

    public FileDataHandler(string dataDirectoryPath, string dataFileName, bool useEncryption)
    {
        this.dataDirectoryPath = dataDirectoryPath;
        this.dataFileName = dataFileName;
        this.useEncryption = useEncryption;

        //Ensure directory is created if it doesnt exist already
        if (!Directory.Exists(dataDirectoryPath))
        {
            Directory.CreateDirectory(dataDirectoryPath);
        }
    }

    public GameData Load(string profileID)
    {
        //File path
        string fullPath = Path.Combine(dataDirectoryPath, profileID, dataFileName);
        GameData loadedData = null;

        if(File.Exists(fullPath))
        {
            try
            { //Load serialized data from file
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                //Decrypt data if necessary
                if (useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                //Deserialize data from JSON
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad); 
            }
            catch (Exception e)
            {
                Debug.LogError("Error while loading data to file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;

    }

    public void Save(GameData data, string profileID)
    {
        //File path
        string fullPath = Path.Combine(dataDirectoryPath, profileID, dataFileName);
        try
        {
            //Directory where the file will be written to
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            //Serialize game data into JSON
            string dataToStore = JsonUtility.ToJson(data, true);

            //Encrypt
            if (useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            //Write data to file
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error while saving data to file: " + fullPath + "\n" + e);
        }

    }

    public Dictionary<string, GameData> LoadAllProfiles()
    {
        Dictionary<string, GameData> profileDictionary = new Dictionary<string, GameData>();

        //Loop over all profiles in directory path
        IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(dataDirectoryPath).EnumerateDirectories();
        foreach (DirectoryInfo dirInfo in dirInfos)
        {
            string profileID = dirInfo.Name;

            //Check if a folder in the directory has the data file needed to store game data
            string fullPath = Path.Combine(dataDirectoryPath, profileID, dataFileName);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning("Skipping directory: " + profileID + " as it doesnt contain data");
                continue;
            }

            //Make sure profile data is not null
            GameData profileData = Load(profileID);
            if (profileData != null)
            {
                profileDictionary.Add(profileID, profileData);
            }
            else
            {
                Debug.LogError("Error while loading profile data: " + profileID);
            }

        }

        return profileDictionary;
    }

    private string EncryptDecrypt(string data)
    {   //XOR encryption
        string modifiedData = "";
        for (int i = 0; i < data.Length; i++)
        {
            modifiedData += (char)(data[i] ^ encryptionKey[i % encryptionKey.Length]);
        }
        return modifiedData;
    }

}
