using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Animations;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class UnitFedeer : MonoBehaviour
{
    //Sheep settings
    [Header("Sheep Settings")]
    public float speed = 2;
    public float listeningRadius = 3f;
    public float turnSpeed = 3f;
    public float updateRate = 0.1f; //listening rate (in seconds)
    private float nextUpdate = 0f;
    public float feederWaitTime = 60f; //wait time at the feeder, this must be changed so it starts waiting, when the food is in the feeder basket
    public List<Node> feederNodes;
    Node feederNode;

    public Transform exclmMark, questionMark, visionCone;

    
    public Animator animator;
    float timerWacth = 0f;

    public bool exit = false;

    //Path and waypoint settings
    [Header("Path & Waypoint Settings")]

    Vector3[] path;
    int pathIndex;
    public Transform exitWaypoint;

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
        animator = GetComponentInChildren<Animator>();
       
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
        

        feederNodes = AGrid.feeders;
        feederNode = feederNodes[UnityEngine.Random.Range(0, feederNodes.Count)];
        while(!feederNode.available) { feederNode = feederNodes[UnityEngine.Random.Range(0, feederNodes.Count)]; }
        feederNode.available = false;
        lastKnownObjective = "comedor";
        MoveTo(lastKnownObjective);
    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(feederNode.fd.transform.position, this.transform.position) <= 1f) //La oveja esta al lado de su comedero asignado
        {
            if(timerWacth == 0) timerWacth = Time.time + 10f;
            feederNode.fd.GetComponent<Feeder>().sheep = true;
            scListener.radius = 0f;
            animator.SetBool("Come", true);
            if (Time.time >= timerWacth)
            {
                if (!animator.GetBool("Vigila"))
                {
                    animator.SetBool("Vigila", true);
                    questionMark.gameObject.SetActive(true);
                    timerWacth = 0;
                }
                else
                {
                    animator.SetBool("Vigila", false);
                    questionMark.gameObject.SetActive(false);
                    timerWacth = 0;
                }
            }

            if (animator.GetBool("Come") && !animator.GetBool("Vigila")) 
            {
                visionCone.gameObject.SetActive(false);
            }else if (!visionCone.gameObject.activeSelf)
            {
                visionCone.gameObject.SetActive(true) ;
            }
            
            StartCoroutine(WaitAtFeeder());
        }
        else
        {
            animator.SetBool("Come", false);
            animator.SetBool("Vigila", false);
            timerWacth = 0;
            if(lastKnownObjective != "comedor")
            {
                feederNode.fd.GetComponent<Feeder>().sheep = false;
                feederNode.fd.GetComponent<Feeder>().FoodVisibility(false);
            }
        }

        if(Vector3.Distance(target.position, this.transform.position) <= 0.9f && GetComponentInChildren<ObserverFeeder>().frenesiMode) //El player esta tocando a la oveja y la oveja esta en modo Frenesi
        {
            Debug.Log("Perdiste");
            GameEnding.result = "lose";
            GameEnding.End = true;
           
        }

        if(Vector3.Distance(transform.position, exitWaypoint.position) <= 1f && exit) //La oveja ha llegado a la salida y esta lista para irse
        {
            IsOnExit(exit);
        }
    }


    public void MoveTo(string objetivo) //Le dice a la oveja donde ir, la OvejaFeeder tiene tres Fases de recorrido
    {
        StopAllCoroutines();
        if(objetivo == target.name) //Buscar al Player
        {
            PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
        }
        else if(objetivo == "comedor") // Ir al comedor
        {
            PathRequestManager.RequestPath(transform.position, feederNode.worldPosition, OnPathFound);
        }
        else if(objetivo == "salida")//Ir a la salida
        {
            exit = true;
            PathRequestManager.RequestPath(transform.position, exitWaypoint.position, OnPathFound);
        }
    }


    private void IsOnExit(bool onExit) //Si la oveja ya comio y esta en la salida desaparece.
    {
        if(onExit) 
        {
            feederNode.available = true;
            Spawner.contador--;
            Spawner.sheeps.Remove(this.gameObject);
            Destroy(this.gameObject); 
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

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)//Si el PathRequest encuentra un camino valido manda a la oveja a seguir ese camino
    {
        if (pathSuccessful)
        {
            path = newPath;
            pathIndex = 0; // Resetear el índice del camino
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }
    
    IEnumerator FollowPath()//Siguiendo el camino encontrado por el PathRequest
    {
        if(path.Length > 0) 
        { 
            Vector3 currentWaypoint = path[0];

            while (currentWaypoint != null)
            {
                if (transform.position == currentWaypoint)
                {
                    pathIndex++;
                    if (pathIndex >= path.Length)
                    {
                    
                        yield break;  // Salir de la corutina una vez que se dirige hacia la salida
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

    IEnumerator WaitAtFeeder()//Cuando esta en el comedor y no ha comido todavia, "come" unos segundos y luego sale del restaurante 
    {
        
        yield return new WaitForSeconds(feederWaitTime);
        lastKnownObjective = "salida";
        scListener.radius = listeningRadius;
        MoveTo(lastKnownObjective);
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
