using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor.Build;
using UnityEngine;

public class ProjectileTurret : MonoBehaviour
{
    [SerializeField] float projectileSpeed = 1;
    [SerializeField] Vector3 gravity = new Vector3(0, -9.8f, 0);
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;
    [SerializeField] bool useLowAngle;
    [SerializeField] float maxXAngleRotation = 40;

    List<Vector3> points = new List<Vector3>();


    // Update is called once per frame
    void Update()
    {
        TrackMouse();
        TurnBase();
        RotateGun();
        DrawTrajectoryLine(crosshair.transform.position, useLowAngle);

        if (Input.GetButtonDown("Fire1"))
            Fire();
    }

    private void DrawTrajectoryLine(Vector3 targetPosition, bool usingLowAngle)
    {
        points.Clear();

        Vector3 startingPosition = barrelEnd.position;
        Vector3 startingVelocity = barrelEnd.forward * projectileSpeed;
        int i = 0;

        points.Add(startingPosition);
        for (float time = 0; time < maxNumberOfPoints; time += TimeStep)
        {
            i++;

            Vector3 point = Vector3.zero;
            point.x = startingPosition.x + startingVelocity.x * time + (gravity.x / 2f * time * time);
            //point.y = barrelEnd.transform.position.y; // for testing with keeping line straight
            point.y = startingPosition.y + startingVelocity.y * time + (gravity.y / 2f * time * time);
            point.z = startingPosition.z + startingVelocity.z * time + (gravity.z / 2f * time * time);


            Vector3 lastPosition = points[i - 1];
            Vector3 directionFromPerviousPoint = (point - lastPosition).normalized;

            if(Physics.Raycast(lastPosition, directionFromPerviousPoint, out RaycastHit hit, (point - lastPosition).magnitude))
            {
                points.Add(hit.point);
                break;
            }
            else
            {
                points.Add(point);
            }

        }


        line.positionCount = points.Count;

        line.SetPositions(points.ToArray());


    }


    [SerializeField] private int maxNumberOfPoints = 5;
    [SerializeField] private float TimeStep = 0.5f;

    [ContextMenu("Fire")]
    void Fire()
    {
        GameObject projectile = Instantiate(projectilePrefab, barrelEnd.position, gun.transform.rotation);
        projectile.GetComponent<Rigidbody>().velocity = projectileSpeed * barrelEnd.transform.forward;
        Physics.gravity = gravity;
    }

    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if(Physics.Raycast(cameraRay, out hit, 1000, targetLayer))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
            //Debug.Log("hit ground");
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
        turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);

        //Quaternion trajectoryAngleOffset = lookRotation;
        
        //turretBase.transform.rotation = trajectoryAngleOffset;
        //float? baseAngle = CalculateTrajectoryZ(crosshair.transform.position, useLowAngle);
        //if (baseAngle != null)
        //    trajectoryAngleOffset *= Quaternion.Euler(0,(float)baseAngle,0);
        //turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, trajectoryAngleOffset, Time.deltaTime * baseTurnSpeed);

        //if(baseAngle != null)
        //{
        //    turretBase.transform.localEulerAngles = new Vector3(0,(float)baseAngle, 0f);
        //}

        //else
        //{
        //    Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        //    Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
        //    turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
        //}

    }

    void RotateGun()
    {
        float? angle = CalculateTrajectory(crosshair.transform.position, useLowAngle);

        //float? angle = Trajectory(barrelEnd.position, crosshair.transform.position, GravityPlane.Z, useLowAngle);


        if (angle != null)
        {
            float appliedXAngleRotation = (float)angle;
            //appliedXAngleRotation = Mathf.Clamp(appliedXAngleRotation, 0, maxXAngleRotation);

            gun.transform.localEulerAngles = new Vector3(appliedXAngleRotation, 0, 0);

        }

    }


    enum GravityPlane
    {
        X, Y, Z,
    }



    float? Trajectory(Vector3 orgin, Vector3 target, GravityPlane gravityPlane, bool useLow)
    {
        Vector3 targetDirection = target - orgin;

        float targetSinglePoint = 0f;

        float grav = 0f;

        switch (gravityPlane)
        {
            case GravityPlane.X: targetSinglePoint = targetDirection.x; targetDirection.x = 0; grav = -gravity.x; break;
            case GravityPlane.Y: targetSinglePoint = targetDirection.y; targetDirection.y = 0; grav = -gravity.y; break;
            case GravityPlane.Z: targetSinglePoint = targetDirection.z; targetDirection.z = 0; grav = -gravity.z; break;
        }

        float x = targetDirection.magnitude;

        float v = projectileSpeed;
        float v2 = Mathf.Pow(v, 2);
        float v4 = Mathf.Pow(v, 4);

        //Currently this value assumes gravity is downward
        //float g = -gravity.y;


        float x2 = Mathf.Pow(x, 2);

        float underRoot = v4 - grav * ((grav * x2) + (2 * targetSinglePoint * v2));

        if (underRoot >= 0)
        {
            float root = Mathf.Sqrt(underRoot);
            float highAngle = v2 + root;
            float lowAngle = v2 - root;

            float calculatedAngle;

            if (useLow)
                calculatedAngle = (Mathf.Atan2(lowAngle, grav * x) * Mathf.Rad2Deg);
            else
                calculatedAngle = (Mathf.Atan2(highAngle, grav * x) * Mathf.Rad2Deg);

            //Downwards gravity
            if (grav * -1 >= 0)
            {
                calculatedAngle = 180 - calculatedAngle;
            }
            //Upwards gravity
            else
            {
                calculatedAngle = 360 - calculatedAngle;
            }


            return calculatedAngle;

        }
        else
            return null;

    }


    float? CalculateTrajectory(Vector3 target, bool useLow)
    {
        Vector3 targetDir = target - barrelEnd.position;

        float y = targetDir.y; 

        targetDir.y = 0;

        float x = targetDir.magnitude;

        float v = projectileSpeed;
        float v2 = Mathf.Pow(v, 2);
        float v4 = Mathf.Pow(v, 4);

        //Currently this value assumes gravity is downward
        float g = -gravity.y;


        float x2 = Mathf.Pow(x, 2);

        float underRoot = v4 - g * ((g * x2) + (2 * y * v2));

        if (underRoot >= 0)
        {
            float root = Mathf.Sqrt(underRoot);
            float highAngle = v2 + root;
            float lowAngle = v2 - root;

            float calculatedAngle;

            if (useLow)
                calculatedAngle = (Mathf.Atan2(lowAngle, g * x) * Mathf.Rad2Deg);
            else
                calculatedAngle = (Mathf.Atan2(highAngle, g * x) * Mathf.Rad2Deg);


            //Downwards gravity
            if(gravity.y >= 0)
            {
                calculatedAngle = 180 - calculatedAngle;
            }
            //Upwards gravity
            else
            {
                calculatedAngle = 360 - calculatedAngle;
            }


            return calculatedAngle;

        }
        else
            return null;
    }


    private float? CalculateTrajectoryZ(Vector3 target, bool useLow)
    {
        Vector3 targetDir = target - barrelEnd.position;

        float z = targetDir.z;

        targetDir.z = 0;

        float x = targetDir.magnitude;

        float v = projectileSpeed;
        float v2 = Mathf.Pow(v, 2);
        float v4 = Mathf.Pow(v, 4);

        //Currently this value assumes gravity is downward
        float g = -gravity.z;


        float x2 = Mathf.Pow(x, 2);

        float underRoot = v4 - g * ((g * x2) + (2 * z * v2));

        if (underRoot >= 0)
        {
            float root = Mathf.Sqrt(underRoot);
            float highAngle = v2 + root;
            float lowAngle = v2 - root;

            float calculatedAngle;

            if (useLow)
                calculatedAngle = (Mathf.Atan2(lowAngle, g * x) * Mathf.Rad2Deg);
            else
                calculatedAngle = (Mathf.Atan2(highAngle, g * x) * Mathf.Rad2Deg);


            //Downwards gravity
            if (gravity.z >= 0)
            {
                calculatedAngle = 180 - calculatedAngle;
            }
            //Upwards gravity
            else
            {
                calculatedAngle = 360 - calculatedAngle;
            }

            //Debug.Log(calculatedAngle);

            return calculatedAngle;

        }
        else
            return null;

    }


}
