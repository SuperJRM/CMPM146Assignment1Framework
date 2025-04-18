using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class SteeringBehavior : MonoBehaviour
{
    public Vector3 target;
    public KinematicBehavior kinematic;
    public List<Vector3> path;
    // you can use this label to show debug information,
    // like the distance to the (next) target
    public TextMeshProUGUI label;



    [SerializeField] float arrivalRadius = 1f;
    [SerializeField] float stopRadius = 1f;
    [SerializeField] float minDesiredSpeed = 10f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        kinematic = GetComponent<KinematicBehavior>();
        target = transform.position;
        path = null;
        EventBus.OnSetMap += SetMap;
    }

    // Update is called once per frame
    void Update()
    {
        // Assignment 1: If a single target was set, move to that target
        //                If a path was set, follow that path ("tightly")

        // you can use kinematic.SetDesiredSpeed(...) and kinematic.SetDesiredRotationalVelocity(...)
        //    to "request" acceleration/decceleration to a target speed/rotational velocity

        //  -- Learned from section --
        float dist = (target - transform.position).magnitude;
        Vector3 dir = target - transform.position;
        float angle = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
        label.text = dist.ToString() + " " + angle.ToString();
        //  --

        if (path == null || path.Count == 0)
        {
            if (dist < stopRadius)
            {
                kinematic.SetDesiredSpeed(0);
            }
            else if (dist < arrivalRadius)
            {
                kinematic.SetDesiredSpeed(kinematic.GetMaxSpeed()/2);
            }
            else
            {
                if ((1 - (Mathf.Abs(angle)/180)) * kinematic.GetMaxSpeed() < minDesiredSpeed)
                {
                    kinematic.SetDesiredSpeed(minDesiredSpeed);
                }
                else
                {
                    kinematic.SetDesiredSpeed((1 - (Mathf.Abs(angle)/180)) * kinematic.GetMaxSpeed());
                }

                kinematic.SetDesiredRotationalVelocity(Mathf.Abs(angle)/180 * (kinematic.GetMaxRotationalVelocity() * Mathf.Sign(angle)));
            }
        }
        else
        {
            if (dist < stopRadius && path != null)
            {
                if (path.Count > 0)
                {
                    SetTarget(path[0]);
                    path.Remove(path[0]);
                }
                else
                {
                    kinematic.SetDesiredSpeed(0);
                }
            }
            if (dist < arrivalRadius && path != null)
            {
                if (path.Count > 0)
                {
                    //find angle of path[0]
                    float newAngle = Vector3.SignedAngle(transform.forward, path[0] - transform.position, Vector3.up);
                    kinematic.SetDesiredSpeed((1 - (Mathf.Abs(angle) / 180)) * kinematic.GetMaxSpeed());
                }
                else
                {
                    kinematic.SetDesiredSpeed(kinematic.GetMaxSpeed() / 2);
                }
            }
            else
            {
                if ((1 - (Mathf.Abs(angle)/180)) * kinematic.GetMaxSpeed() < minDesiredSpeed)
                {
                    kinematic.SetDesiredSpeed(minDesiredSpeed);
                }
                else
                {
                    kinematic.SetDesiredSpeed((1 - (Mathf.Abs(angle)/180)) * kinematic.GetMaxSpeed());
                }

                kinematic.SetDesiredRotationalVelocity(Mathf.Abs(angle)/180 * (kinematic.GetMaxRotationalVelocity() * Mathf.Sign(angle)));
            }
        }
    }

    public void SetTarget(Vector3 target)
    {
        this.target = target;
        EventBus.ShowTarget(target);
    }

    public void SetPath(List<Vector3> path)
    {
        this.path = path;
    }

    public void SetMap(List<Wall> outline)
    {
        this.path = null;
        this.target = transform.position;
    }
}
