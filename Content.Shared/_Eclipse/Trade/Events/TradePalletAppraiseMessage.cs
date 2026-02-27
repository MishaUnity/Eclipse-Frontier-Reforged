using Robust.Shared.Serialization;

namespace Content.Shared.Trade.Events;

/// <summary>
/// Raised on a client request to refresh the pallet console
/// </summary>
[Serializable, NetSerializable]
public sealed class TradePalletAppraiseMessage : BoundUserInterfaceMessage
{

}
