using Robust.Shared.Serialization;

namespace Content.Shared.Trade.Events;

/// <summary>
/// Raised on a client request pallet sale
/// </summary>
[Serializable, NetSerializable]
public sealed class TradePalletSellMessage : BoundUserInterfaceMessage
{

}
