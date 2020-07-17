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

        private int damage;
        private BasicShip target;

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

        public void CannonSetup(int _damage, BasicShip _target)
        {
            damage = _damage;
            target = _target;
            StartCoroutine(cannonDelay());
        }

        private IEnumerator cannonDelay()
        {
            yield return new WaitForSeconds(0.5f);
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

                        if (!(hit.gameObject.name == "Shield"))
                        {
                            target.receiveDamage(damage);
                        }

                        ////Here we compare to enemy base and deal damage ON HIT, not on spawn.
                        //if (!(hit.gameObject.name == "Shield")) //Don't deal damage if we hit the shield.
                        //{
                        //    if (hit.gameObject.GetComponent<BasicShip>())
                        //    {
                        //        var enemy = hit.gameObject.GetComponent<BasicShip>();
                        //        enemy.receiveDamage(damage);
                        //    }
                        //    if (hit.gameObject.GetComponentInParent<BasicShip>())
                        //    {
                        //        var enemy = hit.gameObject.GetComponentInParent<BasicShip>();
                        //        enemy.receiveDamage(damage);
                        //    }
                        //}

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