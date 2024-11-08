using System;
using NaughtyAttributes;

public class MonoBehaviorTests : Singleton<MonoBehaviorTests>
{

    public event EventHandler OnRemoveCards;
    public event EventHandler OnDealCards;

    [Button]
    public void RedealCards()
    {
        OnRemoveCards?.Invoke(this, EventArgs.Empty);
        OnDealCards?.Invoke(this, EventArgs.Empty);
    }
}
