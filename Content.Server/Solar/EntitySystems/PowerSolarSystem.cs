using System.Linq;
using Content.Server.Power.Components;
using Content.Server.Solar.Components;
using Content.Shared.GameTicking;
using Content.Shared.Maps;
using Content.Shared.Physics;
using JetBrains.Annotations;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;

namespace Content.Server.Solar.EntitySystems
{
    /// <summary>
    ///     Responsible for maintaining the solar-panel sun angle and updating <see cref='SolarPanelComponent'/> coverage.
    /// </summary>
    [UsedImplicitly]
    internal sealed class PowerSolarSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _robustRandom = default!;
        [Dependency] private readonly SharedPhysicsSystem _physicsSystem = default!;
        [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

        /// <summary>
        /// Maximum panel angular velocity range - used to stop people rotating panels fast enough that the lag prevention becomes noticable
        /// </summary>
        public const float MaxPanelVelocityDegrees = 1f;

        /// <summary>
        /// The current sun angle.
        /// </summary>
        public Angle TowardsSun = Angle.Zero;

        /// <summary>
        /// The current sun angular velocity. (This is changed in Initialize)
        /// </summary>
        public Angle SunAngularVelocity = Angle.Zero;

        /// <summary>
        /// The distance before the sun is considered to have been 'visible anyway'.
        /// This value, like the occlusion semantics, is borrowed from all the other SS13 stations with solars.
        /// </summary>
        public float SunOcclusionCheckDistance = 20;

        // Eclipse: moved solar targeting to SolarTargetingComponent

        /// <summary>
        /// Queue of panels to update each cycle.
        /// </summary>
        private readonly Queue<Entity<SolarPanelComponent>> _updateQueue = new();

        public override void Initialize()
        {
            SubscribeLocalEvent<SolarPanelComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<SolarPanelComponent, PostMapInitEvent>(OnPostMapInit); // Eclipse
            SubscribeLocalEvent<RoundRestartCleanupEvent>(Reset);
            RandomizeSun();
        }

        public void Reset(RoundRestartCleanupEvent ev)
        {
            RandomizeSun();
            // Eclipse-Start
            var query = EntityQueryEnumerator<SolarTargetingComponent>();
            while (query.MoveNext(out _, out var panel))
            {
                panel.TargetPanelRotation = Angle.Zero;
                panel.TargetPanelVelocity = Angle.Zero;
                panel.TotalPanelPower = 0;
            }
            // Eclipse-End
        }

        private void RandomizeSun()
        {
            // Initialize the sun to something random
            TowardsSun = MathHelper.TwoPi * _robustRandom.NextDouble();
            SunAngularVelocity = Angle.FromDegrees(0.1 + ((_robustRandom.NextDouble() - 0.5) * 0.05));
        }

        private void OnMapInit(EntityUid uid, SolarPanelComponent component, MapInitEvent args)
        {
            Init(uid, component); // Eclipse
        }

        // Eclipse-Start
        private void OnPostMapInit(EntityUid uid, SolarPanelComponent component, PostMapInitEvent args)
        {
            Init(uid, component);
        }

        private void Init(EntityUid uid, SolarPanelComponent component)
        {
            UpdateSupply(uid, component);
        }
        // Eclipse-End

        public override void Update(float frameTime)
        {
            TowardsSun += SunAngularVelocity * frameTime;
            TowardsSun = TowardsSun.Reduced();

            // Eclipse-Start
            var targetingQuery = EntityQueryEnumerator<SolarTargetingComponent>();
            while (targetingQuery.MoveNext(out _, out var comp))
            {
                comp.TargetPanelRotation += comp.TargetPanelVelocity * frameTime;
                comp.TargetPanelRotation = comp.TargetPanelRotation.Reduced();
            }
            // Eclipse-End

            if (_updateQueue.Count > 0)
            {
                var panel = _updateQueue.Dequeue();
                if (panel.Comp.Running)
                    UpdatePanelCoverage(panel);
            }
            else
            {
                // Eclipse-Start
                targetingQuery = EntityQueryEnumerator<SolarTargetingComponent>();
                while (targetingQuery.MoveNext(out _, out var comp))
                {
                    comp.TotalPanelPower = 0;
                }
                // Eclipse-End
                var query = EntityQueryEnumerator<SolarPanelComponent, TransformComponent>();
                while (query.MoveNext(out var uid, out var panel, out var xform))
                {
                    // Eclipse-Start
                    var gridUid = _transformSystem.GetGrid((uid, xform));

                    if (!gridUid.HasValue)
                        continue;

                    if (EnsureComp<SolarTargetingComponent>(gridUid.Value, out var targetComp))
                    {
                        _transformSystem.SetWorldRotation(xform, targetComp.TargetPanelRotation);
                    }
                    else
                    {
                        targetComp.TargetPanelRotation = _transformSystem.GetWorldRotation(xform);
                    }

                    targetComp.TotalPanelPower += panel.MaxSupply * panel.Coverage;
                    targetComp.NumPanels += 1;
                    // Eclipse-End

                    _updateQueue.Enqueue((uid, panel));
                }

                // Eclipse-Start
                // Remove all SolarTargetingComponent's that are not needed
                targetingQuery = EntityQueryEnumerator<SolarTargetingComponent>();
                while (targetingQuery.MoveNext(out var uid, out var comp))
                {
                    if (comp.NumPanels == 0)
                        RemComp<SolarTargetingComponent>(uid);
                }
                // Eclipse-End
            }
        }

        private void UpdatePanelCoverage(Entity<SolarPanelComponent> panel)
        {
            var entity = panel.Owner;
            var xform = Comp<TransformComponent>(entity);

            // So apparently, and yes, I *did* only find this out later,
            // this is just a really fancy way of saying "Lambert's law of cosines".
            // ...I still think this explaination makes more sense.

            // In the 'sunRelative' coordinate system:
            // the sun is considered to be an infinite distance directly up.
            // this is the rotation of the panel relative to that.
            // directly upwards (theta = 0) = coverage 1
            // left/right 90 degrees (abs(theta) = (pi / 2)) = coverage 0
            // directly downwards (abs(theta) = pi) = coverage -1
            // as TowardsSun + = CCW,
            // panelRelativeToSun should - = CW
            var panelRelativeToSun = _transformSystem.GetWorldRotation(xform) - TowardsSun;
            // essentially, given cos = X & sin = Y & Y is 'downwards',
            // then for the first 90 degrees of rotation in either direction,
            // this plots the lower-right quadrant of a circle.
            // now basically assume a line going from the negated X/Y to there,
            // and that's the hypothetical solar panel.
            //
            // since, again, the sun is considered to be an infinite distance upwards,
            // this essentially means Cos(panelRelativeToSun) is half of the cross-section,
            // and since the full cross-section has a max of 2, effectively-halving it is fine.
            //
            // as for when it goes negative, it only does that when (abs(theta) > pi)
            // and that's expected behavior.
            float coverage = (float)Math.Max(0, Math.Cos(panelRelativeToSun));

            if (coverage > 0)
            {
                // Determine if the solar panel is occluded, and zero out coverage if so.
                var ray = new CollisionRay(_transformSystem.GetWorldPosition(xform), TowardsSun.ToWorldVec(), (int)CollisionGroup.Opaque);
                var rayCastResults = _physicsSystem.IntersectRayWithPredicate(
                    xform.MapID,
                    ray,
                    SunOcclusionCheckDistance,
                    e => !xform.Anchored || e == entity);
                if (rayCastResults.Any())
                    coverage = 0;
            }

            // Total coverage calculated; apply it to the panel.
            panel.Comp.Coverage = coverage;
            UpdateSupply(panel, panel);
        }

        public void UpdateSupply(
            EntityUid uid,
            SolarPanelComponent? solar = null,
            PowerSupplierComponent? supplier = null)
        {
            if (!Resolve(uid, ref solar, ref supplier, false))
                return;

            supplier.MaxSupply = (int)(solar.MaxSupply * solar.Coverage);
        }
    }
}
