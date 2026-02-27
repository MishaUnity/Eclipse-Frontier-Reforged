using Content.Client.Cargo.UI;
using Content.Shared.Trade.BUI;
using Content.Shared.Trade.Events;
using Robust.Client.UserInterface;

namespace Content.Client.Trade.BUI;

public sealed class TradePalletConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private CargoPalletMenu? _menu;

    public TradePalletConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<CargoPalletMenu>();
        _menu.AppraiseRequested += OnAppraisal;
        _menu.SellRequested += OnSell;
    }

    private void OnAppraisal()
    {
        SendMessage(new TradePalletAppraiseMessage());
    }

    private void OnSell()
    {
        SendMessage(new TradePalletSellMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not TradePalletConsoleInterfaceState palletState)
            return;

        _menu?.SetEnabled(palletState.Enabled);
        _menu?.SetAppraisal(palletState.Appraisal);
        _menu?.SetCount(palletState.Count);
    }
}
