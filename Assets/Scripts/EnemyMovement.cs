using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float movementSpeed = 3f;
    public Transform[] patrolPoints;
    private bool isBacktracking = false;
    private int currentPointIndex = 0;

    // Update is called once per frame
    void Update()
    {
        float step = movementSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position,patrolPoints[currentPointIndex].position, step);

        if (transform.position == patrolPoints[currentPointIndex].position)
        {
            if (isBacktracking)
            {
                currentPointIndex--;
            }
            else
            {
                currentPointIndex++;
            }
        }

        if (currentPointIndex >= patrolPoints.Length - 1)
        {
            isBacktracking = true;
        }
        else if (currentPointIndex <= 0)
        {
            isBacktracking = false;
        }

    }
}
