using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Slider volumeSlider;

    private void Start()
    {
        LoadVolume();
    }

    public void setFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;

        if (DataPersistenceManager.Instance != null)
        {
            DataPersistenceManager.Instance.GetCurrentGameData().masterVolume = volume;
            DataPersistenceManager.Instance.SaveGame();
        }
    }

    private void LoadVolume()
    {
        if (DataPersistenceManager.Instance != null)
        {
            float savedVolume = DataPersistenceManager.Instance.GetCurrentGameData().masterVolume;
            AudioListener.volume = savedVolume;

            if (volumeSlider != null)
            {
                volumeSlider.value = savedVolume;
            }
        }
    }

}
