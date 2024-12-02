using Assets.SimpleLocalization.Scripts;
using UnityEngine;

public class Localization : MonoBehaviour
{
    void Awake()
    {
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
