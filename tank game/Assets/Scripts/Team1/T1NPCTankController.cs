using UnityEngine;
using System.Collections;
using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;

public class T1NPCTankController : T1.T1AdvancedFSM 
{
    public GameObject Bullet;
    public int health;
    public UnityEngine.AI.NavMeshAgent navAgent;
    public Transform[] points;
    public int lowHealth;
    public string teamTanksTag = "Team1";

    //Initialize the Finite state machine for the NPC tank
    protected override void Initialize()
    {
        health = 100;
        lowHealth = 50;

        elapsedTime = 0.0f;
        shootRate = 2.0f;

        //Get the target enemy(Player)
        List<GameObject> enemies = getEnemies();

        enemyTransform = GetClosestEnemy(enemies);

        //Get the turret of the tank
        turret = gameObject.transform.GetChild(0).transform;
        bulletSpawnPoint = turret.GetChild(0).transform;

        //Start Doing the Finite State Machine
        ConstructFSM();
        navAgent.updateRotation = false;
        navAgent.angularSpeed = 2;
    }

    public Transform GetClosestEnemy(List<GameObject> enemies)
    {
        if(enemies.Capacity == 0)
        {
            return transform;
        }
        Transform closestEnemy = enemies[0].transform;
        for (int i = 1; i < enemies.Capacity; i++)
        {
            if (Vector3.Distance(enemies[i].transform.position, transform.position) < Vector3.Distance(closestEnemy.position, transform.position))
                closestEnemy = enemies[i].transform;
           
        }

        return closestEnemy;
    }

    public List<GameObject> getEnemies()
    {
        GameObject[] tanks = FindObjectsOfType<GameObject>();
        List<GameObject> enemies = new List<GameObject>();
        for (int i = 0; i < tanks.Length; i++)
        {
            if (!tanks[i].CompareTag(teamTanksTag) && tanks[i].tag != "Untagged")
                enemies.Add(tanks[i]);
        }
        return enemies;
    }

    //Update each frame
    protected override void FSMUpdate()
    {
        //Check for health
        elapsedTime += Time.deltaTime;
    }

    protected override void FSMFixedUpdate()
    {
        CurrentState.Reason(enemyTransform, transform);
        CurrentState.Act(enemyTransform, transform);
    }

    public void SetTransition(T1.Transition t) 
    { 
        PerformTransition(t); 
    }

    private void ConstructFSM()
    {
        //Get the list of points
        pointList = GameObject.FindGameObjectsWithTag("WandarPoint");

        Transform[] waypoints = new Transform[pointList.Length];
        int i = 0;
        foreach(GameObject obj in pointList)
        {
            waypoints[i] = obj.transform;
            i++;
        }

        T1PatrolState patrol = new T1PatrolState(waypoints);
        patrol.AddTransition(T1.Transition.SawPlayer, T1.FSMStateID.Chasing);
        patrol.AddTransition(T1.Transition.NoHealth, T1.FSMStateID.Dead);
        patrol.AddTransition(T1.Transition.Colliding, T1.FSMStateID.Evading);


        T1ChaseState chase = new T1ChaseState(waypoints);
        chase.AddTransition(T1.Transition.LostPlayer, T1.FSMStateID.Patrolling);
        chase.AddTransition(T1.Transition.ReachPlayer, T1.FSMStateID.Attacking);
        chase.AddTransition(T1.Transition.NoHealth, T1.FSMStateID.Dead);

        T1AttackState attack = new T1AttackState(waypoints);
        attack.AddTransition(T1.Transition.LostPlayer, T1.FSMStateID.Patrolling);
        attack.AddTransition(T1.Transition.SawPlayer, T1.FSMStateID.Chasing);
        attack.AddTransition(T1.Transition.NoHealth, T1.FSMStateID.Dead);
        attack.AddTransition(T1.Transition.LowHealth, T1.FSMStateID.Flee);

        T1DeadState dead = new T1DeadState();
        dead.AddTransition(T1.Transition.NoHealth, T1.FSMStateID.Dead);

        T1FleeState flee = new T1FleeState(waypoints);
        flee.AddTransition(T1.Transition.NoHealth, T1.FSMStateID.Dead);
        flee.AddTransition(T1.Transition.SawPlayer, T1.FSMStateID.Chasing);

        T1EvadingState evade = new T1EvadingState(waypoints);
        evade.AddTransition(T1.Transition.NoHealth, T1.FSMStateID.Dead);
        evade.AddTransition(T1.Transition.LostPlayer, T1.FSMStateID.Patrolling);

        AddFSMState(patrol);
        AddFSMState(chase);
        AddFSMState(attack);
        AddFSMState(dead);
        AddFSMState(flee);
        AddFSMState(evade);
    }

    void OnCollisionEnter(Collision collision)
    {
        //Reduce health
        if (collision.gameObject.tag == "Bullet")
        {
            health -= 25;
            if (health <= 0)
            {
                Debug.Log("Switch to Dead State");
                SetTransition(T1.Transition.NoHealth);
                Destroy(gameObject);
            } else if (health <= lowHealth)
            {
                Debug.Log("Switch to Flee state");
                navAgent.enabled = true;
                SetTransition(T1.Transition.LowHealth);
            }
        }
    }
    
    public void ShootBullet()
    {
        if (elapsedTime >= shootRate)
        {
            Instantiate(Bullet, transform.Find("Turret").Find("SpawnPoint").position, transform.Find("Turret").rotation);
            elapsedTime = 0.0f;
        }
    }
}
