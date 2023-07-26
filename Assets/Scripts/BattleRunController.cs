using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleRunController : MonoBehaviour
{
    [SerializeField] private ActorController actor;
    [SerializeField] private Transform camFollowTarget;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 100))
            {
                actor.MoveTo(hit.point);
            }
        }
    }

    private void LateUpdate()
    {
        camFollowTarget.transform.position = actor.transform.position;
    }
}
