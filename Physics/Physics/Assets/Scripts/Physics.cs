using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Physics : MonoBehaviour {
    List<Particule> particules = new List<Particule>();
    public int nbParticules;
    public int gridSize;
    public int cellSize; // == neighbour distance
    public float gravity;
    public GameObject prefab;
    public float boundaryForce;
    public float boundaryOffset;
    public float speedMax;
    public float densityColor;
    public float rho;
    public float k;
    public float knear;

    
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
          
        }
        initGrid();
        fillGrid();
        UpdateDensity();

    }

	void FixedUpdate () {
     
        ApplyGravity();
        UpdateDensity();
        Relaxation();
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
         
            if (particule.velocity.sqrMagnitude > speedMax * speedMax) {
               particule.velocity = particule.velocity.normalized * speedMax;
            }
          
        }
    }

    void fillGrid() {
        clearGrid();
        foreach (Particule particule in particules) {
            particule.gridX = (int)particule.position.x / cellSize;
            particule.gridY = (int)particule.position.y / cellSize;
            
            gridParticule[particule.gridX, particule.gridY].Add(particule);
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

    void UpdateDensity() {
   
        List<int[]> indexes = new List<int[]>();
        foreach (Particule particule in particules) {
            particule.squareDensity = 0;
            particule.neighbours.Clear();
            indexes = IndexToExplore(particule);
            foreach(int[] index in indexes) {
                calculDensity(index[0], index[1], particule);
            }
            particule.neighbours.Remove(particule);
            particule.gameObject.GetComponent<SpriteRenderer>().color = getColor(particule.squareDensity);

        }
    }

    Color getColor(float density) {
        return new Color(1- density / densityColor, density/ densityColor, 0.5f,0.5f);
    }

    bool isNeighbour(Particule a , Particule b) {
        return Vector3.Distance(a.position, b.position) < cellSize;
    }

    void calculDensity(int i, int j, Particule particuleRef) {
        float dist;
        foreach(Particule particule in gridParticule[i, j]) {
            if(isNeighbour(particuleRef, particule)) {
                particuleRef.neighbours.Add(particule);
                dist = (1 - Vector3.Distance(particuleRef.position, particule.position) / cellSize);
                particuleRef.squareDensity += dist * dist;
                particuleRef.cubeDensity += dist * dist * dist;
         
            }
        }
    }

    List<int[]> IndexToExplore(Particule particuleRef) {
        int x = particuleRef.gridX;
        int y = particuleRef.gridY;
       
        int[] coord1 = new int[2];
        List<int[]> index = new List<int[]>();
        
        coord1[0] = x;
        coord1[1] = y;
        index.Add(coord1);
        if ((x+1) < gridSize && (y+1) < gridSize) {
          
            int[] coord = new int[2];
            coord[0] = x+1;
            coord[1] = y+1;
            index.Add(coord);
        }
        if((x-1) >= 0 && (y-1) >= 0) {
      
            int[] coord = new int[2];
            coord[0] = x - 1;
            coord[1] = y - 1;
            index.Add(coord);
        }
        if((x+1) < gridSize) {
          
            int[] coord = new int[2];
            coord[0] = x + 1;
            coord[1] = y ;
            index.Add(coord);
        }
        if ((y+1) < gridSize) {
       
            int[] coord = new int[2];
            coord[0] = x;
            coord[1] = y + 1;
            index.Add(coord);
        }
        if((y-1) >= 0) {
     
            int[] coord = new int[2];
            coord[0] = x;
            coord[1] = y - 1;
            index.Add(coord);
        }
        if((x-1) >= 0) {

            int[] coord = new int[2];
            coord[0] = x - 1;
            coord[1] = y ;
            index.Add(coord);
        }
        if((x+1) < gridSize && (y-1) >= 0) {
       
            int[] coord = new int[2];
            coord[0] = x + 1;
            coord[1] = y - 1;
            index.Add(coord);
        }
        if((x-1) >= 0 && (y+1) < gridSize) {
         
            int[] coord = new int[2];
            coord[0] = x - 1;
            coord[1] = y + 1;
            index.Add(coord);
        }

        return index;
    }

    void Relaxation() {
        float pi, pinear, dist;
        foreach (Particule particule in particules) {
            pi = k * (particule.squareDensity - rho);
            pinear = knear * particule.cubeDensity;

            foreach(Particule neighbour in particule.neighbours) {
                dist = 1 - (Vector3.Distance(particule.position, neighbour.position) / cellSize);
                neighbour.velocity += (pi * dist + pinear * dist * dist) * -1.0f * (particule.position - neighbour.position).normalized;  
            }  
        }
    }


}
