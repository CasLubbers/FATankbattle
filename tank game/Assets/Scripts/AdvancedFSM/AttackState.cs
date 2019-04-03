using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class AttackState : FSMState
{
    public AttackState(Transform[] wp) 
    { 
        waypoints = wp;
        stateID = FSMStateID.Attacking;
        curRotSpeed = 2.0f;
        curSpeed = 100.0f;
    }

    public override void Reason(Transform enemy, Transform player)
    {
        //Check the distance with the player tank
        float dist = Vector3.Distance(player.position, enemy.position);
        if (dist >= 200.0f && dist < 300.0f)
        {
            player.GetComponent<NavMeshAgent>().isStopped = false;
            Debug.Log("Switch to Chase State");
            player.GetComponent<NPCTankController>().SetTransition(Transition.SawPlayer);
            return;
        }
        //Transition to patrol is the tank become too far
        else if (dist >= 300.0f)
        {
            player.GetComponent<NavMeshAgent>().isStopped = false;
            Debug.Log("Switch to Patrol State");
            player.GetComponent<NPCTankController>().SetTransition(Transition.LostPlayer);
            return;
        }
        //Transition to flee when the health is below 50
        //if (player.GetComponent<NPCTankController>().health <= 50)
        //{
        //    Debug.Log("Switch to Flee state");
        //    player.GetComponent<NPCTankController>().SetTransition(Transition.LowHealth);
        //    return;
        //}
    }

    public override void Act(Transform enemy, Transform player)
    {
        //Set the target position as the player position
        //destPos = enemy.position;

        //Always Turn the turret towards the player
        //Transform turret = player.GetComponent<NPCTankController>().turret;
        //Quaternion turretRotation = Quaternion.LookRotation(destPos - turret.position);
        //turret.rotation = Quaternion.Slerp(turret.rotation, turretRotation, Time.deltaTime * curRotSpeed);
        float dist = Vector3.Distance(player.position, enemy.position);
        if(dist <= 150)
        {
            player.GetComponent<NavMeshAgent>().isStopped = true;
            //Shoot bullet towards the player
            player.GetComponent<NPCTankController>().ShootBullet();
        }
        
    }
}
