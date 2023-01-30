using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// まるちゃんモデル
/// </summary>
public class PolygonModel : A3DCharacterModel
{
    protected override Vector3 DefaultNeckRotation { get; set; } = new Vector3(2.508f, -0.454f, 10.416f);
    protected override Vector3 DefaultBodyRotation { get; set; } = new Vector3(0.0f, -0.204f, -4.902f);

    public override int LeftEyeKey { get => 7; }

    public override int RightEyeKey { get => 7; }

    public override int MouthKey { get => 2; }

    protected override int[] SmileKey { get; } = { 8 };

    protected override int[] ShyKey { get; } = { };

    protected override int[] AngryKey { get; } = { };

    protected override int[] SulkyKey { get; } = { };

    protected override int[] SadKey { get; } = { };

    protected override int[] CryKey { get; } = { };

    protected override int[] SurprisedKey { get; } = { };

    protected override int[] PanickedKey { get; } = { };

    protected override int[] PuzzledKey { get; } = { 11 };

    protected override int[] AngryAnimeKey { get; } = { };

    protected override int[] CryAnimeKey { get; } = { };

    protected override int[] SurprisedAnimeKey { get; } = { };

    public PolygonModel(GameObject parent, Animator animator, RuntimeAnimatorController motionController, RuntimeAnimatorController facialController, SkinnedMeshRenderer facial, Transform neck, Transform body) : base(parent, animator, motionController, facialController, facial, neck, body)
    {
    }
}
