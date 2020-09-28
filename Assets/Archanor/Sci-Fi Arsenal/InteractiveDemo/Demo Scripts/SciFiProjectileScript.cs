using System.Collections;
using UnityEngine;

namespace SciFiArsenal
{
    public class SciFiProjectileScript : MonoBehaviour
    {
        public GameObject impactParticle;
        public GameObject projectileParticle;
        public GameObject muzzleParticle;
        public GameObject[] trailParticles;

        [HideInInspector]
        public Vector3 impactNormal; //Used to rotate impactparticle.

        private bool hasCollided = false;

        #region Cale added

        private float damage;
        private BasicShip target;
        private Event myEvent;
        private AffectManager affect;

        #endregion Cale added

        private void Start()
        {
            projectileParticle = Instantiate(projectileParticle, transform.position, transform.rotation) as GameObject;
            projectileParticle.transform.parent = transform;
            if (muzzleParticle)
            {
                muzzleParticle = Instantiate(muzzleParticle, transform.position, transform.rotation) as GameObject;
                muzzleParticle.transform.rotation = transform.rotation * Quaternion.Euler(180, 0, 0);
                Destroy(muzzleParticle, 1.5f); // Lifetime of muzzle effect.
            }
        }

        public void CannonSetup(float _damage, BasicShip _target, Event @event, AffectManager affectManager)
        {
            damage = _damage;
            target = _target;
            myEvent = @event;
            affect = affectManager;
            //GetComponent<Collider>().enabled = false;
            StartCoroutine(cannonDelay());
        }

        private IEnumerator cannonDelay()
        {
            //yield return new WaitForSeconds(0.5f);
            // GetComponent<Collider>().enabled = true;
            while (target != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, target.transform.position, 100.0f * Time.deltaTime);
                yield return null;
            }
        }

        private void OnCollisionEnter(Collision hit)
        {
            if (!hasCollided)
            {
                if (hit.gameObject.layer != 9) //ADDED TO STOCK: We don't collide with other weapons
                {
                    if ((gameObject.CompareTag("Player") && hit.gameObject.CompareTag("Enemy")) || (gameObject.CompareTag("Enemy") && hit.gameObject.CompareTag("Player"))) //ADDED TO STOCK: We only collide with things with different tags
                    {
                        hasCollided = true;
                        impactParticle = Instantiate(impactParticle, transform.position, Quaternion.FromToRotation(Vector3.up, impactNormal)) as GameObject;
                        affect.CullEvent(myEvent); //This is where the bullet culls its own event

                        if (!(hit.gameObject.name == "Shield"))
                        {
                            target.TakeDamage(damage);
                        }
                        else
                        {
                            var player = GameObject.Find("Player Ship");
                            player.GetComponent<PlayerShip>().ShieldHit(damage);
                        }

                        if (hit.gameObject.tag == "Destructible") // Projectile will destroy objects tagged as Destructible
                        {
                            Destroy(hit.gameObject);
                        }

                        foreach (GameObject trail in trailParticles)
                        {
                            GameObject curTrail = transform.Find(projectileParticle.name + "/" + trail.name).gameObject;
                            curTrail.transform.parent = null;
                            Destroy(curTrail, 3f);
                        }
                        Destroy(projectileParticle, 3f);
                        Destroy(impactParticle, 5f);
                        Destroy(gameObject);

                        ParticleSystem[] trails = GetComponentsInChildren<ParticleSystem>();
                        //Component at [0] is that of the parent i.e. this object (if there is any)
                        for (int i = 1; i < trails.Length; i++)
                        {
                            ParticleSystem trail = trails[i];

                            if (trail.gameObject.name.Contains("Trail"))
                            {
                                trail.transform.SetParent(null);
                                Destroy(trail.gameObject, 2f);
                            }
                        }
                    }
                }
            }
        }
    }
}