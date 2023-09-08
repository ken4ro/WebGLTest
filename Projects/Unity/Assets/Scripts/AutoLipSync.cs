using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoLipSync : MonoBehaviour
{
    public const float Timescale = 10f;

    public SkinnedMeshRenderer _skinnedMeshRenderer;

    private float _currentTime { get; set; }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // Progress time.
        _currentTime += (Time.deltaTime * Timescale);

        var mouth = Mathf.Abs(Mathf.Sin(_currentTime));
        //Debug.Log($"mouth = {mouth}");

        _skinnedMeshRenderer.SetBlendShapeWeight(39, mouth * 100);
    }

    public void ResetLipSync()
    {
        _skinnedMeshRenderer.SetBlendShapeWeight(39, 0);
    }
}
