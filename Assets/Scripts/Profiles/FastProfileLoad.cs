using UnityEngine;
using static ProfileManagerService;

public class FastProfileLoad : MonoBehaviour
{
    public string profileID = "i6wuy1b1";
    void Awake()
    {
        DataManager.ProfileID = profileID;
        InitializeProfileManager(profileID);
    }
}
