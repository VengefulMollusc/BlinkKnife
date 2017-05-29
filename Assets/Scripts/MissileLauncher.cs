﻿using UnityEngine;
using System.Collections;
using System;

public class MissileLauncher : MonoBehaviour, WeaponController {

    private MissileController[] missiles;

    [SerializeField]
    private int maxMissiles = 6;

    [SerializeField]
	private float circleWidth = 0.9f;
	[SerializeField]
	private float circleHeight = 0.5f;

    [SerializeField]
    private float loadTime = 0.3f;

    [SerializeField]
    private float launchTime = 0.05f;

    private IEnumerator loadingCoroutine;
    private IEnumerator launchCoroutine;

    private int missileCount = 0;

    [SerializeField]
    private GameObject missilePrefab;

    private PlayerKnifeController knifeController;

    public void Setup(PlayerKnifeController _knifeController)
    {
        knifeController = _knifeController;
    }

	public bool ClickMouse(GameObject _target, Transform _camera, Collider _playerCol)
    {
		if (_target == null || missiles != null) return false;
        missiles = new MissileController[maxMissiles];
        missileCount = 0;

		loadingCoroutine = LoadMissiles(_target.transform, _camera, _playerCol);

        StartCoroutine(loadingCoroutine);
		return true;
    }

    IEnumerator LoadMissiles(Transform _target, Transform _camera, Collider _playerCol)
    {
        while (missileCount < maxMissiles)
        {
            yield return new WaitForSeconds(loadTime);

            float circleProg = (float)missileCount / (float)maxMissiles;
            float angle = circleProg * Mathf.PI * 2;

            float x = Mathf.Sin(angle) * circleWidth;
            float y = Mathf.Cos(angle) * circleHeight;
            Vector3 pos = _camera.TransformPoint(new Vector3(x, y, 0f));

            GameObject missile = GameObject.Instantiate(missilePrefab, pos + _camera.forward, _camera.rotation) as GameObject;
            missiles[missileCount] = missile.GetComponent<MissileController>();
            missiles[missileCount].Setup(_camera, _target, _playerCol);
			missile.layer = 8;
            missileCount++;
        }
    }

    public bool ReleaseMouse()
    {
        if (missiles == null) return true;

        StopCoroutine(loadingCoroutine);
        loadingCoroutine = null;

        StartCoroutine(LaunchMissiles());
        return false;
    }

    IEnumerator LaunchMissiles()
    {
		for (int i = 0; i < missileCount; i++){
			missiles[i].Fire();
			yield return new WaitForSeconds(launchTime);
		}
		missileCount = 0;
        missiles = null;
        knifeController.SetKnifeLock(false);
    }

    
}
