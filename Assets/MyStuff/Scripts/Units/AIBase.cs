using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class AIBase : MonoBehaviour {

    public enum UnitState { AttackMoving, Moving, Guarding, Investigating, AttackingUnit };

    public struct Command
    {
        public UnitState stateToExecute;
        public Vector3 positionToExecute; //använd sedan thisTransform.position för start ofc
        public Transform target;
        public bool friendlyFire;

        public Command(UnitState uS, Vector3 pos, Transform t, bool ff)
        {
            stateToExecute = uS;
            positionToExecute = pos;
            target = t;
            friendlyFire = ff;
        }
    }

    [HideInInspector]
    public List<Command> nextCommando = new List<Command>(); //kedje kommandon??

    public abstract void AttackMove(Vector3 pos);

    public abstract void Move(Vector3 pos);

    public abstract void Guard();

    public abstract void Guard(Vector3 pos);

    public abstract void Investigate(Vector3 pos);

    public abstract void AttackUnit(Transform t, bool friendlyFire);


    public abstract void ExecuteNextCommand();

    public abstract void AddCommandToList(Vector3 pos, UnitState nextState, Transform tar, bool friendlyfire);

    public virtual void ClearCommands()
    {
        nextCommando.Clear();
    }
}
