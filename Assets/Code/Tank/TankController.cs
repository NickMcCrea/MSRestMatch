using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using TMPro;

public class TankController : MonoBehaviour
{

    public enum TankState
    {
        normal,
        destroyed
    }
    public bool infiniteHealth = false;
    public bool infiniteAmmo = false;
    public Color mainTankColor;
    public TankState currentState = TankState.normal;
    public volatile int Health = 10;
    public volatile int Ammo = 10;
    public volatile float X;
    public volatile float Y;
    public volatile float ForwardX;
    public volatile float ForwardY;
    public volatile float Heading;
    public volatile float TurretHeading;
    public volatile float TurretForwardX;
    public volatile float TurretForwardY;
    public string Token;
    public string Name;
    public int Kills;
    public UnityEngine.GameObject root;
    public UnityEngine.GameObject turret;
    public UnityEngine.GameObject barrel;
    public UnityEngine.GameObject firingPoint;
    private float barrelRotateSpeed = 0.5f;
    private float reorientateSpeed = 10f;
    private DateTime lastFireTime = DateTime.Now;
    private float fireInterval = 2;

    private float projectileForce = 2000;
    private float tankMovementForce = 15f;
    private float torqueForce = 35f;

    private int projectileLifetime = 4;
    private DateTime destroyTime;
    private readonly int startingHealth = 10;
    private readonly int startingAmmo = 10;

    private Dictionary<UnityEngine.GameObject, DateTime> projectiles;

    private bool toggleForward;
    private bool toggleReverse;
    private bool toggleLeft;
    private bool toggleRight;
    private bool toggleTurretRight;
    private bool toggleTurretLeft;
    private ParticleSystem smokeParticleSystem;
    public GameSimRules Ruleset { get; internal set; }
    public GameSimulation Sim { get; internal set; }
    public int Deaths { get; internal set; }
    GameObject uiLabel;

    AudioSource fireSound;
    AudioSource shellHit;
    AudioSource tankExplosion;

    // Use this for initialization
    public virtual void Start()
    {
        root = this.gameObject;
        turret = root.transform.Find("top").gameObject;
        barrel = turret.transform.Find("barrel").gameObject;
        firingPoint = barrel.transform.Find("firingpoint").gameObject;

        uiLabel = Instantiate(Resources.Load("Prefabs/TextLabel")) as UnityEngine.GameObject;

        uiLabel.GetComponent<TextMeshPro>().text = Name;

        projectiles = new Dictionary<UnityEngine.GameObject, DateTime>();
        Health = startingHealth;
        Ammo = startingAmmo;
        smokeParticleSystem = root.transform.Find("main").gameObject.GetComponent<ParticleSystem>();
        var em = smokeParticleSystem.emission;

        var sources = GetComponents<AudioSource>();

        fireSound = sources[0];
        shellHit = sources[1];
        tankExplosion = sources[2];


        em.enabled = false;
    }

    // Update is called once per frame
    public virtual void Update()
    {


        ManageProjectiles();

        if (currentState == TankState.normal)
        {
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

            Quaternion q = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * reorientateSpeed);

        }
        else if (currentState == TankState.destroyed)
        {
            TimeSpan sinceDestruction = DateTime.Now - destroyTime;
            if (sinceDestruction.TotalSeconds > Ruleset.RespawnTime)
            {
                Sim.RespawnTank(this);
            }
        }


        if (Camera.current != null)
        {
            var pos = new Vector3(transform.position.x, 10, transform.position.z);

            Quaternion labelRotation = Quaternion.LookRotation(pos - Camera.current.transform.position, Camera.current.transform.up);
            uiLabel.transform.SetPositionAndRotation(pos, labelRotation);
        }

        X = transform.position.x;
        Y = transform.position.z;
        ForwardX = transform.forward.x;
        ForwardY = transform.forward.z;

        //A = atan2(V.y, V.x)
        Heading = (float)Math.Atan2(transform.forward.z, transform.forward.x);
        Heading = Heading * Mathf.Rad2Deg;
        Heading = (Heading + 360) % 360;

        TurretHeading = (float)Math.Atan2(turret.transform.up.z, turret.transform.up.x);
        TurretHeading = TurretHeading * Mathf.Rad2Deg;
        TurretHeading = (TurretHeading + 360) % 360;


        TurretForwardX = turret.transform.up.x;
        TurretForwardY = turret.transform.up.z;


        if (transform.position.y < -10)
        {
            DestroyTank();
        }


        if (infiniteAmmo)
            ReplenishAmmo();
        if (infiniteHealth)
            ReplenishHealth();


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

        UnityEngine.GameObject destroyThis = null;
        foreach (UnityEngine.GameObject p in projectiles.Keys)
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
            UnityEngine.GameObject.Destroy(destroyThis);

        }
    }

    internal void ReActivate()
    {
        currentState = TankState.normal;
        ReplenishHealthAndAmmo();
        Quaternion q = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
        transform.rotation = q;
        Debug.Log(Name + " Respawned ");

        var em = smokeParticleSystem.emission;
        em.enabled = false;
    }

    public void ReplenishHealthAndAmmo()
    {
        ReplenishAmmo();
        ReplenishHealth();
    }

    public void ReplenishHealth()
    {
        Health = startingHealth;
    }

    public void ReplenishAmmo()
    {
        Ammo = startingAmmo;
    }

    void OnCollisionEnter(Collision collision)
    {
        Console.WriteLine("Tank collision detected: " + collision.gameObject.name);

        var go = collision.collider.gameObject;
        if (go.tag.Contains("projectile"))
        {
            //it's one of ours colliding on the way out of the barrel. Ignore.
            if (projectiles != null && projectiles.ContainsKey(go))
                return;

            CalculateDamage(go);
            UnityEngine.GameObject.Destroy(go);


            var bulletExplosivo = Instantiate(Resources.Load("Prefabs/ShellExplosion")) as UnityEngine.GameObject;
            bulletExplosivo.transform.position = go.transform.position;

            shellHit.Play();
        }


    }

    private void CalculateDamage(UnityEngine.GameObject go)
    {
        //for now, all hits subtract one health
        Health--;

        if (Health <= 0)
        {
            Sim.RecordFrag(this, go.GetComponent<ProjectileState>().OwningTank);
            DestroyTank();
            Debug.Log(Name + " Destroyed ");
        }
    }

    private void DestroyTank()
    {
        if (currentState == TankState.destroyed)
            return;

        currentState = TankState.destroyed;
        Stop();
        StopTurret();


        var explosivo1 = Instantiate(Resources.Load("Prefabs/Explosion")) as UnityEngine.GameObject;
        explosivo1.transform.position = transform.position;

        var explosivo2 = Instantiate(Resources.Load("Prefabs/TankExplosion")) as UnityEngine.GameObject;
        explosivo2.transform.position = transform.position;

        var em = smokeParticleSystem.emission;
        em.enabled = true;

        tankExplosion.Play();

        destroyTime = DateTime.Now;
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



    public void Forward()
    {
        if (root == null)
        {
            Console.WriteLine("Root ref null");
            return;
        }
        else
            Console.WriteLine("Root ref fixed");

        if (transform.position.y > -1.75f)
            return;

        //root.GetComponent<Rigidbody>().AddForce(root.transform.forward * tankMovementForce);

        root.GetComponent<Rigidbody>().MovePosition(root.transform.position + root.transform.forward * Time.deltaTime * 5);
    }
    public void Reverse()
    {
        if (transform.position.y > -1.75f)
            return;
        //root.GetComponent<Rigidbody>().AddForce(-root.transform.forward * tankMovementForce);
        root.GetComponent<Rigidbody>().MovePosition(root.transform.position - root.transform.forward * Time.deltaTime * 5);


    }

    public void TurnRight()
    {
        if (transform.position.y > -1.75f)
            return;
        root.transform.Rotate(Vector3.up, 100 * Time.deltaTime);
        //root.GetComponent<Rigidbody>().AddTorque(root.transform.up * torqueForce);


    }

    public void TurnLeft()
    {
        if (transform.position.y > -1.75f)
            return;

        root.transform.Rotate(Vector3.up, -100 * Time.deltaTime);
        //root.GetComponent<Rigidbody>().AddTorque(root.transform.up * -torqueForce);


    }

    public void TurretLeft()
    {
        if (GameFlags.BasicTank)
            turret.transform.RotateAround(transform.up, -barrelRotateSpeed * Time.deltaTime);
        else
            turret.transform.RotateAround(transform.up, barrelRotateSpeed * Time.deltaTime);
    }
    public void TurretRight()
    {
        if (GameFlags.BasicTank)
            turret.transform.RotateAround(transform.up, barrelRotateSpeed * Time.deltaTime);
        else
            turret.transform.RotateAround(transform.up, -barrelRotateSpeed * Time.deltaTime);
    }

    public void Fire()
    {
        TimeSpan timeSinceLastFired = DateTime.Now - lastFireTime;
        if (timeSinceLastFired.TotalSeconds < fireInterval)
            return;

        if (Ammo < 1)
            return;

        lastFireTime = DateTime.Now;
        UnityEngine.GameObject projectile = Instantiate(Resources.Load("Prefabs/Projectile")) as UnityEngine.GameObject;
        projectile.GetComponent<ProjectileState>().OwningTank = this;
        projectile.transform.position = firingPoint.transform.position;
        projectile.GetComponent<TrailRenderer>().enabled = true;
        projectile.GetComponent<TrailRenderer>().Clear();




        if (GameFlags.BasicTank)
        {
            projectile.GetComponent<Rigidbody>().AddForce(barrel.transform.up * projectileForce);
            projectile.GetComponent<TrailRenderer>().startColor = mainTankColor;
            projectile.GetComponent<TrailRenderer>().endColor = mainTankColor;
        }
        else
        {
            projectile.GetComponent<Rigidbody>().AddForce(-barrel.transform.up * projectileForce);
            projectile.GetComponent<TrailRenderer>().startColor = Color.blue;
            projectile.GetComponent<TrailRenderer>().endColor = Color.blue;
        }


        Ammo--;
        projectiles.Add(projectile, lastFireTime);

        fireSound.Play();

    }

    private void OnDestroy()
    {
        GameObject.Destroy(uiLabel);
    }


}




