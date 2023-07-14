using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
#if !UNITY_DISABLE_MANAGED_COMPONENTS
using Unity.NetCode.Hybrid;
#endif

public struct InAirAnimationData : IComponentData
{
    [GhostField(Quantization = 1000)] public float Time;
}

#if !UNITY_DISABLE_MANAGED_COMPONENTS
public class InAirGhostPlayableBehaviour : GhostPlayableBehaviour
{
    private GhostAnimationController _animationController;
    private AnimationClipPlayable _animationClip;

    public override void PreparePredictedData(NetworkTick serverTick, float deltaTime, bool isRollback)
    {
        ref InAirAnimationData animationData = ref _animationController.GetPlayableDataRef<InAirAnimationData>();

        animationData.Time += deltaTime / (float)_animationClip.GetDuration();
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        InAirAnimationData locoData = _animationController.GetPlayableData<InAirAnimationData>();

        _animationClip.SetTime(locoData.Time * _animationClip.GetDuration());

        base.PrepareFrame(playable, info);
    }

    public void Initialize(GhostAnimationController controller, PlayableGraph graph, Playable owner, AnimationClip animClip)
    {
        _animationController = controller;

        _animationClip = AnimationClipPlayable.Create(graph, animClip);
        _animationClip.SetDuration(animClip.length);

        AnimationLayerMixerPlayable additiveMixer = AnimationLayerMixerPlayable.Create(graph);

        int aimMixerPort = additiveMixer.AddInput(_animationClip, 0);
        additiveMixer.SetInputWeight(aimMixerPort, 1);

        owner.SetInputCount(1);
        graph.Connect(additiveMixer, 0, owner, 0);
        owner.SetInputWeight(0, 1);
    }
}

[CreateAssetMenu(fileName = "InAir", menuName = "NetCode/Animation/InAir")]
public class GhostAnimationGraph_InAir : GhostAnimationGraphAsset
{
    public AnimationClip InAirClip;

    public override Playable CreatePlayable(GhostAnimationController controller, PlayableGraph graph, List<GhostPlayableBehaviour> behaviours)
    {
        ScriptPlayable<InAirGhostPlayableBehaviour> behaviourPlayable = ScriptPlayable<InAirGhostPlayableBehaviour>.Create(graph);
        InAirGhostPlayableBehaviour behaviour = behaviourPlayable.GetBehaviour();

        behaviours.Add(behaviour);

        behaviour.Initialize(controller, graph, behaviourPlayable, InAirClip);

        return behaviourPlayable;
    }

    public override void RegisterPlayableData(IRegisterPlayableData register)
    {
        register.RegisterPlayableData<InAirAnimationData>();
    }
}
#endif