using UnityEngine;
using System.Collections;

public class T1DeadState : T1FSMState
{
    public T1DeadState() 
    {
        stateID = T1.FSMStateID.Dead;
    }

    public override void Reason(Transform player, Transform npc)
    {

    }

    public override void Act(Transform player, Transform npc)
    {
        //Do Nothing for the dead state
    }
}
