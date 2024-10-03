using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    public GameObject comida;
    public bool hayComida = false;
    // Start is called before the first frame update
    void Start()
    {
        comida.SetActive(false);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Food")
        {
            if (Input.GetKey(KeyCode.Space))
            {
                hayComida = true;
                foodVisibility();
            }
        }
    }

    public void foodVisibility()
    {
        comida.SetActive(hayComida);
    }
}
