using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class AttackState : FSMState
{
    private NavMeshAgent agent;
    public AttackState(Transform[] wp) 
    {
        
        waypoints = wp;
        stateID = FSMStateID.Attacking;
        curRotSpeed = 2.0f;
        curSpeed = 100.0f;
    }

    public override void Reason(Transform enemy, Transform player)
    {
        agent = player.GetComponent<NavMeshAgent>();
        //Check the distance with the player tank
        float dist = Vector3.Distance(player.position, enemy.position);
        if (dist >= 200.0f && dist < 300.0f)
        {
            agent.enabled = true;
            Debug.Log("Switch to Chase State");
            player.GetComponent<NPCTankController>().SetTransition(Transition.SawPlayer);
            return;
        }
        //Transition to patrol is the tank become too far
        else if (dist >= 300.0f)
        {
            agent.enabled = true;
            Debug.Log("Switch to Patrol State");
            player.GetComponent<NPCTankController>().SetTransition(Transition.LostPlayer);
            return;
        }
    }

    public override void Act(Transform enemy, Transform player)
    {
        agent = player.GetComponent<NavMeshAgent>();
        //Stop the tank from driving
        if (agent.enabled)
            agent.enabled = false;

        //Always Turn the turret towards the enemy
        Transform turret = player.GetComponent<NPCTankController>().turret;
        Quaternion turretRotation = Quaternion.LookRotation(enemy.position - turret.position);
        turret.rotation = Quaternion.Slerp(turret.rotation, turretRotation, Time.deltaTime * curRotSpeed);


        Vector3 pos = turret.Find("SpawnPoint").position;

        Ray forwardRay = new Ray(pos, turret.Find("SpawnPoint").forward);
        RaycastHit hit;

        Debug.DrawRay(pos, turret.Find("SpawnPoint").forward * 200, Color.red);


        if (!Physics.Raycast(forwardRay, out hit, 200) || (Physics.Raycast(forwardRay, out hit, 200) && hit.transform.gameObject.tag != "Team1" && hit.transform.gameObject.tag != "Untagged"))
        {
            //Shoot bullet towards the enemy
            player.GetComponent<NPCTankController>().ShootBullet();
        }
    }
}
