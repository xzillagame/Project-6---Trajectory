using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTurret : MonoBehaviour
{
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;

    
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float projectileSpeed = 1;

    [SerializeField] float laserMaxDistance = 1000f;

    List<Vector3> laserPoints = new List<Vector3>();

    [SerializeField] int maxLaserReflect = 3;

    [SerializeField, Range(0,1)] float timescale = 0.5f;

    // Update is called once per frame
    void Update()
    {

        Time.timeScale = timescale;

        TrackMouse();
        TurnBase();

        laserPoints.Clear();
        laserPoints.Add(barrelEnd.position);

        if(Physics.Raycast(barrelEnd.position, barrelEnd.forward, out RaycastHit hit, laserMaxDistance, targetLayer))
        {
            laserPoints.Add(hit.point);

            LaserReflectionCalculation(hit.point, VectorReflectionCalculation(barrelEnd.forward, hit.normal));
        }
        //If laser is not hitting anything, project out still
        else
        {
            laserPoints.Add(barrelEnd.position + (barrelEnd.forward * laserMaxDistance));
        }

        line.positionCount = laserPoints.Count;
        for(int i = 0; i < line.positionCount; i++)
        {
            line.SetPosition(i, laserPoints[i]);
        }


        if (Input.GetButtonDown("Fire1"))
            Fire();

    }

    [ContextMenu("Shoot")]
    void Fire()
    {
        GameObject projectile = Instantiate(projectilePrefab, barrelEnd.position, gun.transform.rotation);
        Rigidbody projectileRB = projectile.GetComponent<Rigidbody>();
        projectileRB.useGravity = false;
        Projectile p = projectile.GetComponent<Projectile>();
        p.InitalizeProjectile(barrelEnd.transform.forward, projectileSpeed);
        p.canReflect = true;


        //Debug.Log(Vector3.Reflect(new Vector3(0.25f, 1, 0.1f), new Vector3(0.5f, 0.5f, 1f)).normalized);
        //Debug.Log(VectorReflectionCalculation(new Vector3(0.25f, 1, 0.1f), new Vector3(0.5f, 0.5f, 1f)));

    }

    private void LaserReflectionCalculation(Vector3 orginPosition, Vector3 direction)
    {
        Vector3 iteratedReflectDirection = direction;
        Vector3 iteratedReflectpoint = orginPosition;

        for(int i = 0; i < maxLaserReflect; i++)
        {

            if (Physics.Raycast(iteratedReflectpoint, iteratedReflectDirection, out RaycastHit hit, laserMaxDistance, targetLayer))
            {
                laserPoints.Add(hit.point);

                iteratedReflectDirection = Vector3.Reflect(iteratedReflectDirection, hit.normal);//VectorReflectionCalculation(iteratedReflectDirection, hit.normal);
                iteratedReflectpoint = hit.point;

            }
            else
            {
                laserPoints.Add(iteratedReflectpoint + (iteratedReflectDirection * laserMaxDistance));
                return;
            }
        }


    }

    private Vector3 VectorReflectionCalculation(Vector3 originalDirectionToReflect, Vector3 collisionNormal)
    {
        originalDirectionToReflect.Normalize();
        collisionNormal.Normalize();
        Vector3 reflectionVector = originalDirectionToReflect - 
                        (2 * Vector3.Dot(originalDirectionToReflect, collisionNormal) * collisionNormal);

        return reflectionVector;
    }


    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if(Physics.Raycast(cameraRay, out hit, 1000, targetLayer ))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, directionToTarget.y, directionToTarget.z));
        turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }
}
