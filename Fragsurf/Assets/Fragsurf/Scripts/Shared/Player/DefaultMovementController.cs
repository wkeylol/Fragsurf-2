using Fragsurf.Actors;
using Fragsurf.Movement;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Packets;
using SurfaceConfigurator;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared.Player
{
    public class DefaultMovementController : MovementController, ISurfControllable
    {

        //private MovementConfig _moveConfig = new MovementConfig();
        private SurfController _surfController = new SurfController();
        private List<FSMTrigger> _touchingTriggers = new List<FSMTrigger>();
        private List<FSMTrigger> _lastTickTriggers = new List<FSMTrigger>();

        public MoveType MoveType { get; set; } = MoveType.Walk;
        public MoveData MoveData { get; } = new MoveData();
        public BoxCollider Collider => Human.HumanGameObject.BoundsCollider;
        public Quaternion Orientation => Quaternion.identity;
        public Vector3 Forward { get; set; }
        public Vector3 Right { get; set; }
        public Vector3 Up { get; set; }
        public Vector3 StandingExtents => new Vector3(.858f, 1.83f, .858f) * .5f;
        public new GameObject GroundObject { get; set; }
        public override bool MouseControlsRotation => true;

        public DefaultMovementController(Human human) 
            : base(human)
        {
        }

        public override void ExecuteMovement(UserCmd.CmdFields cmd)
        {
            MoveData.OldButtons = MoveData.Buttons;

            Right = Quaternion.Euler(cmd.Angles) * Vector3.right;
            Forward = Quaternion.Euler(new Vector3(0, cmd.Angles.y, 0)) * Vector3.forward;
            Up = Quaternion.Euler(cmd.Angles) * Vector3.up;

            MoveData.Buttons = cmd.Buttons;
            MoveData.ViewAngles = cmd.Angles;
            MoveData.Origin = Human.Origin;
            MoveData.Velocity = Human.Velocity;
            MoveData.BaseVelocity = Human.BaseVelocity;
            MoveData.ForwardMove = 0;
            MoveData.SideMove = 0;
            MoveData.UpMove = 0;

            if (cmd.Buttons.HasFlag(InputActions.MoveBack))
            {
                MoveData.ForwardMove = -Human.Game.GameMovement.Config.ForwardSpeed;
            }

            if (cmd.Buttons.HasFlag(InputActions.MoveForward))
            {
                MoveData.ForwardMove = Human.Game.GameMovement.Config.ForwardSpeed;
            }

            if (cmd.Buttons.HasFlag(InputActions.MoveRight))
            {
                MoveData.SideMove = Human.Game.GameMovement.Config.SideSpeed;
            }

            if (cmd.Buttons.HasFlag(InputActions.MoveLeft))
            {
                MoveData.SideMove = -Human.Game.GameMovement.Config.SideSpeed;
            }

            _surfController.CalculateMovement(this, Human.Game.GameMovement.Config, Time.fixedDeltaTime);

            if (Human.Game.IsHost || Human.Local == Human)
            {
                Human.Velocity = MoveData.Velocity;
                Human.BaseVelocity = MoveData.BaseVelocity;
                Human.Origin = MoveData.Origin;
                Human.Angles = MoveData.ViewAngles;
                Human.Ducked = MoveData.Ducked;

                DetectTouchingTriggers();
                TouchTriggers();

                if(Human.Local == Human)
                {
                    TickFootstep();
                }
            }
        }

        private static RaycastHit[] _touchBuffer = new RaycastHit[32];
        private static Collider[] _touchBuffer2 = new Collider[32];
        private void DetectTouchingTriggers()
        {
            var prevOrigin = MoveData.PreviousOrigin;
            var newOrigin = MoveData.Origin;
            var dist = Vector3.Distance(prevOrigin, newOrigin);
            var extents = Collider.bounds.extents;
            extents.x *= .99f;
            extents.z *= .99f;

            if (dist <= .1f)
            {
                var center = newOrigin;
                center.y += Collider.bounds.extents.y;
                var hitCount = Physics.OverlapBoxNonAlloc(center: center,
                   halfExtents: extents,
                   orientation: Quaternion.identity,
                   mask: 1 << 0,
                   results: _touchBuffer2,
                   queryTriggerInteraction: QueryTriggerInteraction.Collide);

                for (int i = 0; i < hitCount; i++)
                {
                    if (!_touchBuffer2[i].isTrigger)
                    {
                        continue;
                    }
                    var itrig = _touchBuffer2[i].GetComponentInParent<FSMTrigger>();
                    if (itrig != null)
                    {
                        _touchingTriggers.Add(itrig);
                    }
                }
            }
            else
            {
                var dir = (newOrigin - prevOrigin).normalized;
                var center = prevOrigin;
                center.y += Collider.bounds.extents.y;

                var hitCount = Physics.BoxCastNonAlloc(center: center,
                    halfExtents: extents,
                    direction: dir,
                    orientation: Quaternion.identity,
                    maxDistance: dist,
                    layerMask: 1 << 0,
                    results: _touchBuffer,
                    queryTriggerInteraction: QueryTriggerInteraction.Collide);

                for (int i = 0; i < hitCount; i++)
                {
                    if (!_touchBuffer[i].collider.isTrigger)
                    {
                        continue;
                    }
                    var itrig = _touchBuffer[i].collider.GetComponentInParent<FSMTrigger>();
                    if (itrig != null)
                    {
                        _touchingTriggers.Add(itrig);
                    }
                }
            }
        }

        private void TouchTriggers()
        {
            foreach (var trigger in _touchingTriggers)
            {
                if (!_lastTickTriggers.Contains(trigger))
                {
                    trigger.OnStartTouch(Human.EntityId, Human.Game.IsHost);
                }
                else
                {
                    trigger.OnTouch(Human.EntityId, Human.Game.IsHost);
                }
            }

            foreach (var trigger in _lastTickTriggers)
            {
                if (!_touchingTriggers.Contains(trigger))
                {
                    trigger.OnEndTouch(Human.EntityId, Human.Game.IsHost);
                }
            }

            _lastTickTriggers.Clear();
            foreach (var t in _touchingTriggers)
            {
                _lastTickTriggers.Add(t);
            }
            _touchingTriggers.Clear();
        }

        private const float _stepDistance = 3.0f;
        private float _traveled;
        private float _stepRand;
        protected void TickFootstep()
        {
            _traveled += (MoveData.Origin - MoveData.PreviousOrigin).magnitude;
            if (_traveled >= _stepDistance + _stepRand)
            {
                var spd = Human.HammerVelocity();
                var maxSpd = Human.Game.GameMovement.MaxSpeed;
                var vol = Mathf.Lerp(0f, 1f, (float)spd / maxSpd);
                PlayFootstepSound(vol);
                _stepRand = Random.Range(0.0f, 0.5f);
                _traveled = 0.0f;
            }
            else if (MoveData.JustGrounded || MoveData.JustJumped)
            {
                PlayFootstepSound(Random.Range(.7f, 1f));
            }
        }

        private void PlayFootstepSound(float vol)
        {
            if(GroundObject == null
                || Human.HumanGameObject == null
                || Human.HumanGameObject.FeetAudioSource == null)
            {
                return;
            }

            if (Physics.Raycast(MoveData.Origin + Vector3.up * .1f, Vector3.down, out RaycastHit hit, .2f, 1 << Layers.Fidelity))
            {
                if (!hit.collider.TryGetComponent(out SurfaceTypeIdentifier surfId))
                {
                    return;
                }

                var cfg = GameData.Instance.Surfaces != null 
                    ? GameData.Instance.Surfaces.GetSurfaceTypeConfig(surfId.SurfaceType) 
                    : null;

                if(cfg == null)
                {
                    return;
                }

                var audioClip = cfg.GetFootstepSound();
                if(audioClip == null)
                {
                    return;
                }

                vol = Mathf.Clamp(vol, 0f, 1f);

                Human.HumanGameObject.FeetAudioSource.PlayOneShot(audioClip, vol);
            }
        }

    }
}

