using Robust.Shared.Serialization;

namespace Content.Shared.Trade.BUI;

[NetSerializable, Serializable]
public sealed class TradePalletConsoleInterfaceState : BoundUserInterfaceState
{
    /// <summary>
    /// estimated apraised value of all the entities on top of pallets on the same grid as the console
    /// </summary>
    public int Appraisal;

    /// <summary>
    /// number of entities on top of pallets on the same grid as the console
    /// </summary>
    public int Count;

    /// <summary>
    /// are the buttons enabled
    /// </summary>
    public bool Enabled;

    public TradePalletConsoleInterfaceState(int appraisal, int count, bool enabled)
    {
        Appraisal = appraisal;
        Count = count;
        Enabled = enabled;
    }
}
