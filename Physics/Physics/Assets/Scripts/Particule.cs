using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particule : MonoBehaviour {

    public Vector3 velocity;
    public Vector3 position;
    


    public void UpdatePosition() {
        if(transform.position.magnitude > 100) {
            Debug.Log("Je suis dans l'espace");
            return;
        }
        transform.position += velocity * Time.fixedDeltaTime;
        position = transform.position;

    }

    
}
