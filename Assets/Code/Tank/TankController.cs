using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class TankController : MonoBehaviour {

    private GameObject root;
    private GameObject turret;
    private GameObject barrel;
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

	// Use this for initialization
	public virtual void Start ()
    {
        root = this.gameObject;
        turret = root.transform.Find("top").gameObject;
        barrel = turret.transform.Find("barrel").gameObject;
        projectiles = new Dictionary<GameObject,DateTime>();
	}
	
	// Update is called once per frame
	public virtual void Update ()
    {
        ManageProjectiles();
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
            if (projectiles.ContainsKey(go))
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

    #region MSTank API

    

    protected int GetCurrentHealth()
    {
        return currentHealth;
    }

    protected int GetCurrentAmmo()
    {
        return ammoCount;
    }

    protected void Forward()
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

    protected void Reverse()
    {
        root.transform.position -= root.transform.forward * forwardSpeed;
    }

    protected void TurnRight()
    {
        root.transform.RotateAround(Vector3.up, rotateSpeed);
    }

    protected void TurnLeft()
    {
        root.transform.RotateAround(Vector3.up, -rotateSpeed);
    }

    protected void TurretLeft()
    {
        turret.transform.RotateAround(Vector3.up, -barrelRotateSpeed);
    }

    protected void TurretRight()
    {
        turret.transform.RotateAround(Vector3.up, barrelRotateSpeed);
    }

    protected void Fire()
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
        projectile.transform.position = barrel.transform.position + barrel.transform.up;
        projectile.AddComponent<Rigidbody>();
        projectile.GetComponent<Rigidbody>().AddForce(barrel.transform.up * projectileForce);
        ammoCount--;
        projectiles.Add(projectile,lastFireTime);
        
    }
    #endregion
}




