using Content.Server.Solar.Components;
using Content.Server.UserInterface;
using Content.Shared.Solar;
using JetBrains.Annotations;
using Robust.Server.GameObjects;

namespace Content.Server.Solar.EntitySystems
{
    /// <summary>
    /// Responsible for updating solar control consoles.
    /// </summary>
    [UsedImplicitly]
    internal sealed class PowerSolarControlConsoleSystem : EntitySystem
    {
        [Dependency] private readonly PowerSolarSystem _powerSolarSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!; // Eclipse

        /// <summary>
        /// Timer used to avoid updating the UI state every frame (which would be overkill)
        /// </summary>
        private float _updateTimer;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SolarControlConsoleComponent, SolarControlConsoleAdjustMessage>(OnUIMessage);
        }

        public override void Update(float frameTime)
        {
            _updateTimer += frameTime;
            if (_updateTimer >= 1)
            {
                _updateTimer -= 1;
                // Eclipse-Start
                var query = EntityQueryEnumerator<SolarControlConsoleComponent, UserInterfaceComponent, TransformComponent>();
                while (query.MoveNext(out var uid, out _, out var uiComp, out var xform))
                {
                    var gridUid = _transform.GetGrid((uid, xform));

                    if (!gridUid.HasValue)
                        continue;

                    if (TryComp<SolarTargetingComponent>(gridUid.Value, out var targetComp))
                    {
                        var state = new SolarControlConsoleBoundInterfaceState(targetComp.TargetPanelRotation, targetComp.TargetPanelVelocity, targetComp.TotalPanelPower, _powerSolarSystem.TowardsSun);
                        _uiSystem.SetUiState((uid, uiComp), SolarControlConsoleUiKey.Key, state);
                    }
                    else
                    {
                        var state = new SolarControlConsoleBoundInterfaceState(0, 0, 0, _powerSolarSystem.TowardsSun);
                        _uiSystem.SetUiState((uid, uiComp), SolarControlConsoleUiKey.Key, state);
                    }
                }
                // Eclipse-End
            }
        }

        private void OnUIMessage(EntityUid uid, SolarControlConsoleComponent component, SolarControlConsoleAdjustMessage msg)
        {
            // Eclipse-Start
            var gridUid = _transform.GetGrid(uid);

            if (!gridUid.HasValue)
                return;

            if (!TryComp<SolarTargetingComponent>(gridUid.Value, out var targetComp))
                return;

            if (double.IsFinite(msg.Rotation))
            {
                targetComp.TargetPanelRotation = msg.Rotation.Reduced();
            }
            if (double.IsFinite(msg.AngularVelocity))
            {
                var degrees = msg.AngularVelocity.Degrees;
                degrees = Math.Clamp(degrees, -PowerSolarSystem.MaxPanelVelocityDegrees, PowerSolarSystem.MaxPanelVelocityDegrees);
                targetComp.TargetPanelVelocity = Angle.FromDegrees(degrees);
            }
            // Eclipse-End
        }

    }
}
