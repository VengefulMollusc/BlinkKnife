using UnityEngine;
using System.Collections;

public interface WeaponController
{

	void ClickMouse(Transform _target, Transform _camera, Collider[] _playerCols);
    
    bool ReleaseMouse();

    void Setup(PlayerKnifeController _knifeController);
}
