using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class TankController : MonoBehaviour {



    public string Name;
    private GameObject root;
    private GameObject turret;
    private GameObject barrel;
    private GameObject firingPoint;
    private float forwardSpeed = 0.05f;
    private float rotateSpeed = 0.01f;
    private float barrelRotateSpeed = 0.04f;
    private DateTime lastFireTime = DateTime.Now;
    private float fireInterval = 2;
    private float projectileForce = 2000;
    private int ammoCount = 10;
    private int projectileLifetime = 4;
    private int currentHealth = 10;
    private Dictionary<GameObject, DateTime> projectiles;

    private bool toggleForward;
    private bool toggleReverse;
    private bool toggleLeft;
    private bool toggleRight;
    private bool toggleTurretRight;
    private bool toggleTurretLeft;

	// Use this for initialization
	public virtual void Start ()
    {
        root = this.gameObject;
        turret = root.transform.Find("top").gameObject;
        barrel = turret.transform.Find("barrel").gameObject;
        firingPoint = barrel.transform.Find("firingpoint").gameObject;
        projectiles = new Dictionary<GameObject,DateTime>();
      
	}
	
	// Update is called once per frame
	public virtual void Update ()
    {
        ManageProjectiles();

        if (toggleForward)
            Forward();
        if (toggleReverse)
            Reverse();
        if (toggleLeft)
            TurnLeft();
        if (toggleRight)
            TurnRight();
        if (toggleTurretLeft)
            TurretLeft();
        if (toggleTurretRight)
            TurretRight();


	}

    private void ManageProjectiles()
    {

        ClearOldProjectiles();
    }

    private void ClearOldProjectiles()
    {
        if (projectiles == null)
            return;
        if (projectiles.Count == 0)
            return;

        GameObject destroyThis = null;
        foreach(GameObject p in projectiles.Keys)
        {
            TimeSpan timeSinceSpawn = DateTime.Now - projectiles[p];
            if (timeSinceSpawn.TotalSeconds > projectileLifetime)
            {
                destroyThis = p;
                break;
            }
        }

        if (destroyThis != null)
        {
            projectiles.Remove(destroyThis);
            GameObject.Destroy(destroyThis);
            
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Console.WriteLine("Tank collision detected: " + collision.gameObject.name);

        var go = collision.collider.gameObject;
        if (go.tag == "projectile")
        {
            //it's one of ours colliding on the way out of the barrel. Ignore.
            if (projectiles != null && projectiles.ContainsKey(go))
                return;

            CalculateDamage(go);
            GameObject.Destroy(go);
        }

     
    }

    private void CalculateDamage(GameObject go)
    {
        //for now, all hits subtract one health
        currentHealth--;
    }


    public void ToggleForward()
    {
        toggleForward = true;
        toggleReverse = false;
    }
    public void ToggleReverse()
    {
        toggleForward = false;
        toggleReverse = true;
    }
    public void ToggleLeft()
    {
        toggleLeft = true;
        toggleRight = false;
    }
    public void ToggleRight()
    {
        toggleLeft = false;
        toggleRight = true;
    }
    public void ToggleTurretRight()
    {
        toggleTurretLeft = false;
        toggleTurretRight = true;
    }
    public void ToggleTurretLeft()
    {
        toggleTurretLeft = true;
        toggleTurretRight = false;
    }

    public void Stop()
    {
        toggleForward = false;
        toggleReverse = false;
        toggleLeft = false;
        toggleRight = false;
    }
    public void StopTurret()
    {
        toggleTurretLeft = false;
        toggleTurretRight = false;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    public int GetCurrentAmmo()
    {
        return ammoCount;
    }

    public void Forward()
    {
        if (root == null)
        {
            Console.WriteLine("Root ref null");
            return;
        }
        else
            Console.WriteLine("Root ref fixed");
        root.transform.position += root.transform.forward * forwardSpeed;
    }
    public void Reverse()
    {
        root.transform.position -= root.transform.forward * forwardSpeed;
    }
    public void TurnRight()
    {
        root.transform.RotateAround(Vector3.up, rotateSpeed);
    }
    public void TurnLeft()
    {
        root.transform.RotateAround(Vector3.up, -rotateSpeed);
    }
    public void TurretLeft()
    {
        turret.transform.RotateAround(Vector3.up, -barrelRotateSpeed);
    }
    public void TurretRight()
    {
        turret.transform.RotateAround(Vector3.up, barrelRotateSpeed);
    }

    public void Fire()
    {
        TimeSpan timeSinceLastFired = DateTime.Now - lastFireTime;
        if (timeSinceLastFired.TotalSeconds < fireInterval)
            return;

        if (ammoCount < 1)
            return;
        
        lastFireTime = DateTime.Now;
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.tag = "projectile";
        projectile.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        projectile.transform.position = firingPoint.transform.position;
        projectile.AddComponent<Rigidbody>();
        projectile.GetComponent<Rigidbody>().AddForce(barrel.transform.up * projectileForce);
        ammoCount--;
        projectiles.Add(projectile,lastFireTime);
        
    }
    
}




