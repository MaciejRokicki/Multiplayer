using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
#if !UNITY_DISABLE_MANAGED_COMPONENTS
using Unity.NetCode.Hybrid;
#endif

public struct StateAnimationData : IComponentData
{
    [GhostField]
    public int Value;
}

#if !UNITY_DISABLE_MANAGED_COMPONENTS
public class StateSelectorGhostPlayableBehaviour : GhostPlayableBehaviour
{
    private GhostAnimationController _controller;
    private AnimationMixerPlayable _mixer;
    private float[] _transitions;

    public override void PreparePredictedData(NetworkTick serverTick, float deltaTime, bool isRollback)
    {
        ref StateAnimationData stateData = ref _controller.GetPlayableDataRef<StateAnimationData>();

        PlayerInputComponent playerInput = _controller.GetEntityComponentData<PlayerInputComponent>();
        PhysicsVelocity physicsVelocity = _controller.GetEntityComponentData<PhysicsVelocity>();

        if (playerInput.IsGrounded)
        {
            stateData.Value = (physicsVelocity.Linear.x != 0 || physicsVelocity.Linear.z != 0) ? 1 : 0;
        }
        else
        {
            if (physicsVelocity.Linear.y > 0.0f)
            {
                stateData.Value = 2;
            }
            else if (physicsVelocity.Linear.y < 0.0f)
            {
                stateData.Value = 3;
            }
        }
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        StateAnimationData stateData = _controller.GetPlayableData<StateAnimationData>();

        StateTransition(stateData.Value, info.deltaTime);
        base.PrepareFrame(playable, info);
    }
    void StateTransition(int active, float deltaTime)
    {
        float currentWeight = _mixer.GetInputWeight(active);

        if (currentWeight == 1)
            return;

        float transitionTime = _transitions[active];
        currentWeight += deltaTime / transitionTime;
        float remainingWeight = 1 - currentWeight;

        if (currentWeight >= 1)
        {
            currentWeight = 1;
            remainingWeight = 0;
        }

        float weightSum = 0;

        for (int i = 0; i < _transitions.Length; ++i)
        {
            if (i != active)
                weightSum += _mixer.GetInputWeight(i);
        }

        float scale = remainingWeight / weightSum;

        for (int i = 0; i < _transitions.Length; ++i)
        {
            if (i != active)
                _mixer.SetInputWeight(i, scale * _mixer.GetInputWeight(i));
        }

        _mixer.SetInputWeight(active, currentWeight);
    }
    public void Initialize(GhostAnimationController controller, PlayableGraph graph, List<GhostPlayableBehaviour> behaviours, Playable owner,
        GhostAnimationGraph_StateSelector.ControllerDefinition[] controllers)
    {
        _controller = controller;
        _mixer = AnimationMixerPlayable.Create(graph, controllers.Length);
        _transitions = new float[controllers.Length];

        for (int i = 0; i < controllers.Length; ++i)
        {
            Playable playable = controllers[i].Template.CreatePlayable(controller, graph, behaviours);
            graph.Connect(playable, 0, _mixer, i);
            _mixer.SetInputWeight(i, 0);
            _transitions[i] = controllers[i].TransitionTime;
        }

        _mixer.SetInputWeight(0, 1);

        owner.SetInputCount(1);
        graph.Connect(_mixer, 0, owner, 0);
        owner.SetInputWeight(0, 1);
    }
}

[CreateAssetMenu(fileName = "StateSelector", menuName = "NetCode/Animation/StateSelector")]
public class GhostAnimationGraph_StateSelector : GhostAnimationGraphAsset
{
    public enum CharacterAnimationState
    {
        Idle,
        Run,
        Jump,
        InAir
    }

    [Serializable]
    public struct TransitionDefinition
    {
        public CharacterAnimationState SourceState;
        public float TranstionTime;
    }

    [Serializable]
    public struct ControllerDefinition
    {
        public CharacterAnimationState AnimationState;

        public GhostAnimationGraphAsset Template;
        [Tooltip("Default transition time from any other state (unless overwritten)")]
        public float TransitionTime;
        [Tooltip("Custom transition times from specific states")]
        public TransitionDefinition[] CustomTransitions;
    }

    public ControllerDefinition[] Controllers;

    public override Playable CreatePlayable(GhostAnimationController controller, PlayableGraph graph, List<GhostPlayableBehaviour> behaviours)
    {
        ScriptPlayable<StateSelectorGhostPlayableBehaviour> behaviourPlayable = ScriptPlayable<StateSelectorGhostPlayableBehaviour>.Create(graph);
        StateSelectorGhostPlayableBehaviour behaviour = behaviourPlayable.GetBehaviour();

        behaviours.Add(behaviour);

        behaviour.Initialize(controller, graph, behaviours, behaviourPlayable, Controllers);
        return behaviourPlayable;
    }

    public override void RegisterPlayableData(IRegisterPlayableData register)
    {
        register.RegisterPlayableData<StateAnimationData>();

        for (int i = 0; i < Controllers.Length; ++i)
            Controllers[i].Template.RegisterPlayableData(register);
    }
}
#endif