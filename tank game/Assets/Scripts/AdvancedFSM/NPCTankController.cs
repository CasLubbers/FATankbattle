using UnityEngine;
using System.Collections;
using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;

public class NPCTankController : AdvancedFSM 
{
    public GameObject Bullet;
    public int health;
    public UnityEngine.AI.NavMeshAgent navAgent;
    public Transform[] points;
    

    //Initialize the Finite state machine for the NPC tank
    protected override void Initialize()
    {
        health = 100;

        elapsedTime = 0.0f;
        shootRate = 2.0f;

        //Get the target enemy(Player)
        List<GameObject> enemies = getEnemies();
        Debug.Log(enemies);
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
            Debug.Log("ERROR: No Enemies found");
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
        GameObject[] tanks = GameObject.FindGameObjectsWithTag("Tank");
        List<GameObject> enemies = new List<GameObject>();
        for (int i = 0; i < tanks.Length; i++)
        {
            if (tanks[i].transform.Find("Team1") == null)
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

    public void SetTransition(Transition t) 
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

        PatrolState patrol = new PatrolState(waypoints);
        patrol.AddTransition(Transition.SawPlayer, FSMStateID.Chasing);
        patrol.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        patrol.AddTransition(Transition.Colliding, FSMStateID.Evading);

        ChaseState chase = new ChaseState(waypoints);
        chase.AddTransition(Transition.LostPlayer, FSMStateID.Patrolling);
        chase.AddTransition(Transition.ReachPlayer, FSMStateID.Attacking);
        chase.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        AttackState attack = new AttackState(waypoints);
        attack.AddTransition(Transition.LostPlayer, FSMStateID.Patrolling);
        attack.AddTransition(Transition.SawPlayer, FSMStateID.Chasing);
        attack.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        attack.AddTransition(Transition.LowHealth, FSMStateID.Flee);

        DeadState dead = new DeadState();
        dead.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        FleeState flee = new FleeState(waypoints);
        flee.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        flee.AddTransition(Transition.LostPlayer, FSMStateID.Patrolling);

        EvadingState evade = new EvadingState(waypoints);
        evade.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        evade.AddTransition(Transition.LostPlayer, FSMStateID.Patrolling);

        AddFSMState(patrol);
        AddFSMState(chase);
        AddFSMState(attack);
        AddFSMState(dead);
        AddFSMState(flee);
        AddFSMState(evade);
    }

    /// <summary>
    /// Check the collision with the bullet
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter(Collision collision)
    {
        //Reduce health
        if (collision.gameObject.tag == "Bullet")
        {
            health -= 25;
            Debug.Log("Health:" + health);
            if (health <= 0)
            {
                Debug.Log("Switch to Dead State");
                SetTransition(Transition.NoHealth);
                Explode();
            }
        }
    }

    protected void Explode()
    {
        float rndX = Random.Range(10.0f, 30.0f);
        float rndZ = Random.Range(10.0f, 30.0f);
        for (int i = 0; i < 3; i++)
        {
            GetComponent<Rigidbody>().AddExplosionForce(10000.0f, transform.position - new Vector3(rndX, 10.0f, rndZ), 40.0f, 10.0f);
            GetComponent<Rigidbody>().velocity = transform.TransformDirection(new Vector3(rndX, 20.0f, rndZ));
        }

        Destroy(gameObject, 1.5f);
    }

    /// <summary>
    /// Shoot the bullet from the turret
    /// </summary>
    public void ShootBullet()
    {
        if (elapsedTime >= shootRate)
        {
            Instantiate(Bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            elapsedTime = 0.0f;
        }
    }
}
