using System;

[Serializable]
public class ShuttleState
{
    public int dockedOre;
    public int loadingOre;
    public int loadingTargetOre;
    public bool sendAfterLoading;
    public int deliveringOre;
    public float loadingCooldownRemaining;
    public float sendCooldownRemaining;

    public ShuttleState Clone()
    {
        return new ShuttleState
        {
            dockedOre = dockedOre,
            loadingOre = loadingOre,
            loadingTargetOre = loadingTargetOre,
            sendAfterLoading = sendAfterLoading,
            deliveringOre = deliveringOre,
            loadingCooldownRemaining = loadingCooldownRemaining,
            sendCooldownRemaining = sendCooldownRemaining
        };
    }

    public void CopyFrom(ShuttleState source)
    {
        if (source == null)
        {
            Reset();
            return;
        }

        dockedOre = Math.Max(0, source.dockedOre);
        loadingOre = Math.Max(0, source.loadingOre);
        loadingTargetOre = Math.Max(loadingOre, source.loadingTargetOre);
        sendAfterLoading = source.sendAfterLoading;
        deliveringOre = Math.Max(0, source.deliveringOre);
        loadingCooldownRemaining = Math.Max(0f, source.loadingCooldownRemaining);
        sendCooldownRemaining = Math.Max(0f, source.sendCooldownRemaining);
    }

    public void Reset()
    {
        dockedOre = 0;
        loadingOre = 0;
        loadingTargetOre = 0;
        sendAfterLoading = false;
        deliveringOre = 0;
        loadingCooldownRemaining = 0f;
        sendCooldownRemaining = 0f;
    }
}
