using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// うなちゃんモデル
/// </summary>
public class UnaChanModel : A3DCharacterModel
{
    protected override Vector3 DefaultNeckRotation { get; set; } = new Vector3(0.0f, 0.0f, -10.71f);

    protected override Vector3 DefaultBodyRotation { get; set; } = new Vector3(-0.009000001f, 0.002f, -4.437f);

    public override int LeftEyeKey { get => 20; }

    public override int RightEyeKey { get => 21; }

    public override int MouthKey { get => 27; }

    protected override int[] SmileKey { get; } = { 1, 12 };

    protected override int[] ShyKey { get; } = { 2, 14 };

    protected override int[] AngryKey { get; } = { 3 };

    protected override int[] SulkyKey { get; } = { 4 };

    protected override int[] SadKey { get; } = { 5, 15 };

    protected override int[] CryKey { get; } = { 10, 16 };

    protected override int[] SurprisedKey { get; } = { 6, 17 };

    protected override int[] PanickedKey { get; } = { 7, 18 };

    protected override int[] PuzzledKey { get; } = { 9, 26 };

    protected override int[] AngryAnimeKey { get; } = { 8, 20, 21, 29 };

    protected override int[] CryAnimeKey { get; } = { 10, 24, 31 };

    protected override int[] SurprisedAnimeKey { get; } = { 6, 25, 37 };

    public UnaChanModel(GameObject parent, Animator animator, RuntimeAnimatorController motionController, RuntimeAnimatorController facialController, SkinnedMeshRenderer facial, Transform neck, Transform body) : base(parent, animator, motionController, facialController, facial, neck, body)
    {
    }
}
