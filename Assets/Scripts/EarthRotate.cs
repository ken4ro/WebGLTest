using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EarthRotate : MonoBehaviour
{
    [SerializeField]
    GameObject Earth = null;

    private Transform _earthTransform = null;
    private Vector3 _rotateVector = new Vector3(0, 0, -0.05f);

    void Awake()
    {
        _earthTransform = Earth.transform;
    }

    // Update is called once per frame
    void Update()
    {
        _earthTransform.Rotate(_rotateVector);
    }
}
