﻿using UnityEngine;
using System.Collections;

public class PathMover : MonoBehaviour
{

    const float minPathUpdateTime = .2f;
    const float pathUpdateMoveThreshold = .5f;

    public Transform target;
    public Transform transformTarget;

    public float speed = 20;
    public float turnSpeed = 3;
    public float turnDst = 5;
    public float stoppingDst = 10;
    public float repathRate = .3f;
    public bool followingPath;
    public bool followingTarget;

    APath path;


    void Start()
    {
        StartCoroutine(UpdatePath());
        followingTarget = true;
    }

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = new APath(waypoints, transform.position, turnDst, stoppingDst);

            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }



    IEnumerator UpdatePath()
    {
       
            if (Time.timeSinceLevelLoad < repathRate)
            {
                yield return new WaitForSeconds(repathRate);
            }
            if (followingTarget == true)
            {

                PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
                float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
                Vector3 targetPosOld = target.position;
                while (true)
                {
                    yield return new WaitForSeconds(minPathUpdateTime);
                    if ((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
                    {
                        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
                        targetPosOld = target.position;
                    }
                }
        }
        

    }

    IEnumerator FollowPath()
    {


        followingPath = true;
        int pathIndex = 0;
       // transform.LookAt(path.lookPoints[0]);


        float speedPercent = 1;

        while (followingPath)
        {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);

            if (path.turnBoundaries.Length == 0)
            {
                followingPath = false;
                break;
            }
            if (pathIndex >= path.turnBoundaries.Length )
            {
                pathIndex = 0;

            }
            while ( path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
            {
                
                if (pathIndex == path.finishLineIndex)
                {
                    followingPath = false;
                    break;
                }
                else
                {
                    pathIndex++;
                }
            }
            

            if (followingPath)
            {

                if (pathIndex >= path.slowDownIndex && stoppingDst > 0)
                {
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDst);
                    if (speedPercent < 0.01f)
                    {
                        followingPath = false;
                    }
                }

                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
            }

            yield return null;

        }

    }

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            path.DrawWithGizmos();
        }
    }
}

