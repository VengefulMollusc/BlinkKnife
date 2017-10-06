using UnityEngine;
using System.Collections;

public interface WeaponController
{

	bool ClickMouse(GameObject _target, Transform _camera, Collider[] _playerCols);
    
    bool ReleaseMouse();

    void Setup(PlayerKnifeController _knifeController);
}
