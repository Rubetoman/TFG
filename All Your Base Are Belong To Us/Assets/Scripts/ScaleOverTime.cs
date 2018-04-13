﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleOverTime : MonoBehaviour {
    public Vector3 goalScale = Vector3.one;
    public float time = 60.0f;
    private Vector3 initialScale;

	// Use this for initialization
	void Start () {
        StartCoroutine("ScaleCoroutine");
	}
	
    IEnumerator ScaleCoroutine()
    {
        float t = 0.0f;

        while(t < time)
        {
            transform.localScale = Vector3.Lerp(initialScale, goalScale, t / time);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localScale = goalScale;
    }
}
