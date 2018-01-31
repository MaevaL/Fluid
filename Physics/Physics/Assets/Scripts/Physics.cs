using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Physics : MonoBehaviour {
    List<Particule> particules = new List<Particule>();
    public int nbParticules;
    public int gridSize;
    public int cellSize;
    public float gravity;
    public GameObject prefab;
    public float boundaryForce;
    public float boundaryOffset;
    public float speedMax;
    private GameObject go;
    private Vector3 position = new Vector3(0,0,0);
    private Camera cam;
    private float gridWidth;
    private List<Particule>[,] gridParticule; 



	void Start () {
        gridWidth = gridSize * cellSize;
        gridParticule = new List<Particule>[gridSize , gridSize];
        cam = Camera.main;
        cam.transform.position = new Vector3(gridWidth / 2, gridWidth / 2, -10);
		for(int i = 0; i < nbParticules; i++) {
            position.x = Random.Range(0, 1f) * (gridWidth - 2*boundaryOffset) + boundaryOffset;
            position.y = Random.Range(0, 1f) * (gridWidth - 2*boundaryOffset) + boundaryOffset;
            position.z = 0;
            go = Instantiate(prefab, position, Quaternion.identity);
            
            particules.Add(go.GetComponent<Particule>());
            particules[i].position = position;
            particules[i].velocity = Vector3.left + Vector3.up;
        }
        initGrid();
        fillGrid();
	}

	void FixedUpdate () {
     
        ApplyGravity();
        BoundaryRepulsion();
        clampSpeed();
        UpdateAllParticulesPosition();
        fillGrid();
        

    }

    void UpdateAllParticulesPosition() {
        foreach (Particule particule in particules) {
            particule.UpdatePosition();
        }

    }

    void ApplyGravity() {
        foreach(Particule particule in particules) {
            particule.velocity += Vector3.down * gravity * Time.fixedDeltaTime;
        }
    }

    void BoundaryRepulsion() {
        foreach(Particule particule in particules) {
            // Bottom
            if(particule.position.y < boundaryOffset) {
                float local = boundaryOffset - particule.position.y;
                particule.velocity += local * boundaryForce * Vector3.up;
            }
            //Up
            if (particule.position.y > gridWidth - boundaryOffset) {
                float local = particule.position.y - (gridSize - boundaryOffset);
                particule.velocity += local * boundaryForce * Vector3.down;
            }
            // Right
            if (particule.position.x > gridWidth - boundaryOffset) {
                float local = particule.position.x - (gridSize - boundaryOffset);
                particule.velocity += local * boundaryForce * Vector3.left;
            }

            // Left
            if (particule.position.x < boundaryOffset) {
                float local = boundaryOffset - particule.position.x;
                particule.velocity += local * boundaryForce * Vector3.right;
            }

        }
    }

    void clampSpeed() {
        foreach(Particule particule in particules) {
            if(particule.velocity.sqrMagnitude > speedMax * speedMax) {
                particule.velocity = particule.velocity.normalized * speedMax;
            }
        }
    }

    void fillGrid() {
        int x = 0, y = 0;
        clearGrid();
        foreach (Particule particule in particules) {
            x = (int)particule.position.x / cellSize;
            y = (int)particule.position.y / cellSize;

            gridParticule[x, y].Add(particule);
        }  
         
    }

    void clearGrid() {
        for(int i = 0; i < gridSize; i++) {
            for(int j = 0; j < gridSize; j++) {
               
                gridParticule[i, j].Clear();
            }
        }
    }

    void initGrid() {
        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {

                gridParticule[i, j] = new List<Particule>();
            }
        }
    }
}
