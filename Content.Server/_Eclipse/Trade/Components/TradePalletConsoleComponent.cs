using Content.Server.Trade.Systems;
using Robust.Shared.Audio;

namespace Content.Server.Trade.Components;

[RegisterComponent]
[Access(typeof(PalletSystem))]
public sealed partial class TradePalletConsoleComponent : Component
{
    [DataField]
    public SoundSpecifier DenySound = new SoundPathSpecifier("/Audio/Effects/Cargo/buzz_two.ogg");

    [DataField]
    public SoundSpecifier ApproveSound = new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg");
}
