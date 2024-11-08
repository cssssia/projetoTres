using Unity.Netcode;

public class BetManager : NetworkBehaviour
{
    public static BetManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

}