using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Animations;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class UnitPatrol : MonoBehaviour
{
    //Sheep settings
    [Header("Sheep Settings")]
    public float speed = 2.5f;
    public float listeningRadius = 2f;
    public float turnSpeed = 4.5f;
    public float updateRate = 0.1f; //listening rate (in seconds)
    private float nextUpdate = 0f;

    public Transform exclmMark, questionMark, visionCone;

    public bool exit = false;

    //Path and waypoint settings
    [Header("Path & Waypoint Settings")]

    Vector3[] path;
    int pathIndex;
    public List<Transform> waypoints = new List<Transform>(); // Patrol points
    public Transform exitWaypoint;
    int waypointIndex;
    int lastWaypointIndex;
    int nWaypointsVisited = 0;

    //Target settings
    [Header("Target Settings")]

    public Transform target;
    public string lastKnownObjective; //Last known objective of the target
    public SphereCollider scListener;

    //Call the algorithm
    [Header("Algorithm A* Call")]

    public AGrid grid;

    private void Awake()
    {
        grid = GameObject.Find("A*").GetComponent<AGrid>();
        exitWaypoint = GameObject.Find("Exit").transform;
        target = GameObject.Find("JohnLemon").transform;
       
    }

    private void Start()
    {
        questionMark = this.gameObject.transform.GetChild(3);
        exclmMark = this.gameObject.transform.GetChild(2);
        visionCone = this.gameObject.transform.GetChild(4);

        scListener = GetComponent<SphereCollider>();
        if (scListener == null)
        {
            scListener = gameObject.AddComponent<SphereCollider>();
        }
        scListener.isTrigger = true;
        scListener.radius = listeningRadius;
        GameObject waypointer = GameObject.Find("Waypoints");
        foreach (Transform wp in waypointer.transform)
        {
            waypoints.Add(wp);
        }
        lastKnownObjective = "waypoint";
        MoveTo(lastKnownObjective);
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, exitWaypoint.position) <= 1f)
        {
            IsOnExit(exit);
        }
        if (Vector3.Distance(target.position, this.transform.position) <= 1f && GetComponentInChildren<ObserverPatrol>().frenesiMode)
        {
            Debug.Log("perdiste");
            GameEnding.result = "lose";
            GameEnding.End = true;
        }

    }

    private void IsOnExit(bool onExit)
    {
        if (onExit)
        {
            Spawner.contador--;
            Spawner.sheeps.Remove(this.gameObject);
            Destroy(this.gameObject);
        }
    }

    public void MoveTo(string objetivo)
    {
        StopAllCoroutines();
        if (objetivo == target.name)
        {
            PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
        }
        else if (objetivo == "waypoint")
        {
            int newWaypointIndex;
            do
            {
                newWaypointIndex = UnityEngine.Random.Range(0, waypoints.Count);
            } while (newWaypointIndex == lastWaypointIndex && waypoints.Count > 1);

            waypointIndex = newWaypointIndex;
            lastWaypointIndex = waypointIndex;

            if (waypointIndex < waypoints.Count)
            {
                nWaypointsVisited++;
                PathRequestManager.RequestPath(transform.position, waypoints[waypointIndex].position, OnPathFound);
                if (nWaypointsVisited >= 3) { lastKnownObjective = "salida"; }
            }
        }
        else if (objetivo == "salida" || nWaypointsVisited >= 3)
        {
            exit = true;
            PathRequestManager.RequestPath(transform.position, exitWaypoint.position, OnPathFound);
        }
    }

    void OnTriggerEnter(Collider other) //entra en el radio de audicion o interacción
    {
        if (other.CompareTag("Target"))
        {
            StopAllCoroutines();
            questionMark.gameObject.SetActive(true);
        }
    }

    void OnTriggerStay(Collider other) //lo buscará todo el rato (cada updateRate en segundos) que esté dentro del circulo de escucha
    {
        if (other.CompareTag("Target") && Time.time >= nextUpdate)
        {
            nextUpdate = Time.time + updateRate;

            StopAllCoroutines();

            Vector3 directionTarget = (target.position - transform.position);
            Quaternion sheepRotation = Quaternion.LookRotation(directionTarget);

            transform.rotation = Quaternion.Slerp(transform.rotation, sheepRotation, Time.deltaTime * turnSpeed);

        }
    }

    void OnTriggerExit(Collider other) //sale del radio de audicion, pero va al ultimo sitio conocido
    {
        if (other.CompareTag("Target"))
        {
            questionMark.gameObject.SetActive(false);
            MoveTo(lastKnownObjective);
        }
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            pathIndex = 0; // Resetear el índice del camino
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");

        }
    }

    IEnumerator FollowPath()
    {
        if (path.Length > 0)
        {
            Vector3 currentWaypoint = path[0];

            while (currentWaypoint != null)
            {
                if (transform.position == currentWaypoint)
                {
                    pathIndex++;
                    if (pathIndex >= path.Length)
                    {
                        if(nWaypointsVisited >= 3) 
                        {
                            lastKnownObjective = "salida";
                            MoveTo("salida"); 
                            yield break;
                        }
                        else
                        {
                            MoveTo("waypoint");
                            yield break;
                        }
                    }
                    currentWaypoint = path[pathIndex];
                }

                transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
                Vector3 directionTarget = (currentWaypoint - transform.position);
                Quaternion sheepRotation = Quaternion.LookRotation(directionTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, sheepRotation, Time.deltaTime * turnSpeed);

                yield return null;
            }
        }
    }

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = pathIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], Vector3.one);

                if (i == pathIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
}