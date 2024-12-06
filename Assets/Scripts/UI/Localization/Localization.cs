using Assets.SimpleLocalization.Scripts;
using UnityEngine;

public class Localization : MonoBehaviour
{
    public static Localization Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        LocalizationManager.Read();

        LocalizationManager.Language = Application.systemLanguage switch
        {
            SystemLanguage.Portuguese => "Portuguese",
            _ => "English",
        };
    }

    public void SetLocalization(string localization)
    {
        LocalizationManager.Language = localization;
    }
}
