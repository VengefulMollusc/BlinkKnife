using UnityEngine;
using System.Collections;
using System;

public class CubeLauncher : MonoBehaviour, WeaponController {

    private CubeController cube;
    private PlayerKnifeController knifeController;

    [SerializeField]
    private GameObject cubePrefab;

    void Start()
    {
        if (cubePrefab == null)
        {
            throw new MissingReferenceException("Missing cubePrefab Object");
        }
    } 

    public bool ClickMouse(Transform _target, Transform _camera, Collider[] _playerCol)
    {
        // deactivate cube if exists
        if (cube != null)
        {
            cube.Toggle();
            return false;
        }

        if (_target != null)
        {
            GameObject cubeObject = GameObject.Instantiate(cubePrefab, _target.transform.position, Quaternion.Euler(0f, 0f, 0f)) as GameObject;
            cube = cubeObject.GetComponent<CubeController>();
            knifeController.ReturnKnife();
            cube.Setup(_camera, _playerCol, _target);
        } else
        {
            GameObject cubeObject = GameObject.Instantiate(cubePrefab, _camera.position + _camera.forward, Quaternion.Euler(0f, 0f, 0f)) as GameObject;
            cube = cubeObject.GetComponent<CubeController>();
            cube.Setup(_camera, _playerCol, _target);
        }
        return false;
    }

    public bool ReleaseMouse()
    {
        return false;
    }

    public void Setup(PlayerKnifeController _knifeController)
    {
        knifeController = _knifeController;
    }
}
