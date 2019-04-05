using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;

public class AttackState : FSMState
{
    private NavMeshAgent agent;
    public string teamTanksTag = "Team1";
    public float attackDistance = 200f;
    public float lostDistance = 300f;

    public static Transform target;

    public AttackState(Transform[] wp) 
    {
        waypoints = wp;
        stateID = FSMStateID.Attacking;
        curRotSpeed = 2.0f;
        curSpeed = 100.0f;
    }

    public override void Reason(Transform enemy, Transform player)
    {
        if (target == null)
            target = enemy;
        agent = player.GetComponent<NavMeshAgent>();
        //Check the distance with the player tank
        float dist = Vector3.Distance(player.position, target.position);
        if (dist >= attackDistance && dist < lostDistance)
        {
            Debug.Log("Switch to Chase State");
            player.GetComponent<NPCTankController>().SetTransition(Transition.SawPlayer);
            return;
        }
        //Transition to patrol is the tank become too far
        else if (dist >= lostDistance)
        {
            Debug.Log("Switch to Patrol State");
            player.GetComponent<NPCTankController>().SetTransition(Transition.LostPlayer);
            return;
        }
        
    }

    public override void Act(Transform enemy, Transform player)
    {

        agent = player.GetComponent<NavMeshAgent>();
        //Stop the tank from driving
            //player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        agent.velocity = Vector3.zero;
        //agent.destination = player.transform.position;

        //Always Turn the turret towards the enemy
        Transform turret = player.GetComponent<NPCTankController>().turret;
        Quaternion turretRotation = Quaternion.LookRotation(target.position - turret.position);
        turret.rotation = Quaternion.Slerp(turret.rotation, turretRotation, Time.deltaTime * curRotSpeed);

        Vector3 pos = turret.Find("SpawnPoint").position;

        Ray forwardRay = new Ray(pos, turret.Find("SpawnPoint").forward);
        RaycastHit hit;
        bool tankInFront = Physics.Raycast(forwardRay, out hit, attackDistance);

        // friendly fire
        if (tankInFront && hit.transform.gameObject.tag != teamTanksTag && hit.transform.gameObject.tag != "Untagged")
        {
            //Shoot bullet towards the enemy
            player.GetComponent<NPCTankController>().ShootBullet();
        }
    }
   
}
