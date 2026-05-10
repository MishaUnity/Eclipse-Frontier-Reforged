using Content.Client.Ame.UI;
using Content.Shared._Eclipse.Maps.Sectors;
using Content.Shared.Ame.Components;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.Client._Eclipse.Map.Sectors.UI;

[UsedImplicitly]
public sealed class SectorsConsoleBoundUserInterface : BoundUserInterface
{
    private SectorsMapWindow? _window;

    public SectorsConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {

    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<SectorsMapWindow>();
        _window.OnStargateJumpButton += OnStargateJumpRequest;
    }

    public void OnStargateJumpRequest()
    {
        SendMessage(new RequestStargateJumpMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not SectorsMapBoundUserInterfaceState)
            return;

        var castState = (SectorsMapBoundUserInterfaceState)state;
        _window?.UpdateState(castState);
        _window?.SetConsole(Owner);
    }
}
