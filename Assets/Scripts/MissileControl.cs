using System.Collections;
using UnityEngine;

public class MissileControl : MonoBehaviour
{
    private float accel = 1.5f;
    private float maxSpeed = 50.0f;

    public void Launch(GameObject enemy)
    {
        // var lookAt = Vector3.up;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.back);
        StartCoroutine(missileTrack(enemy));
    }

    public void Miss()
    {
        transform.rotation = Quaternion.LookRotation(Vector3.up, Vector3.back);
        StartCoroutine(randomMovement());
    }

    private IEnumerator missileTrack(GameObject enemy)
    {
        var rotateSpeed = 0.0f;
        var speed = 0.0f;
        var randomDirTime = 0.5f;
        var targetDirection = new Vector3(Random.value, Random.value, Random.value);
        while (gameObject)
        {
            if (enemy)
            {
                rotateSpeed += 0.1f;
                // Determine which direction to rotate towards

                if (randomDirTime > 0.0f)
                {
                    randomDirTime -= Time.deltaTime;
                }
                else
                {
                    targetDirection = enemy.transform.position - transform.position;
                }

                // The step size is equal to speed times frame time.
                float singleStep = rotateSpeed * Time.deltaTime;

                // Rotate the forward vector towards the target direction by one step
                Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

                // Draw a ray pointing at our target in
                Debug.DrawRay(transform.position, newDirection, Color.red);

                // Calculate a rotation a step closer to the target and applies rotation to this object
                transform.rotation = Quaternion.LookRotation(newDirection);

                if (speed < maxSpeed) { speed += accel; }
                var toAdd = transform.forward * speed * Time.deltaTime;
                transform.position += toAdd;

                if (Vector3.Distance(transform.position, enemy.transform.position) < 2.5f)
                {
                    GetComponentInChildren<ParticleSystem>().Play();
                    yield return new WaitForSeconds(0.5f);
                    Destroy(this.gameObject);
                }
                yield return null;
            }
            else
            {
                GetComponentInChildren<ParticleSystem>().Play();
                yield return new WaitForSeconds(0.5f);
                Destroy(this.gameObject);
                yield return null;
            }
        }
    }

    private IEnumerator randomMovement()
    {
        var rotateSpeed = 0.0f;
        var speed = 0.0f;

        var randomDir = transform.forward;
        randomDir.x += Random.Range(-50.0f, 50.0f);
        randomDir.y += Random.Range(-50.0f, 50.0f);

        var timeLeft = 1.0f;
        while (timeLeft > 0.0f)
        {
            rotateSpeed += 0.1f;
            speed += accel;

            randomDir.x += Random.Range(-1.0f, 1.0f);
            randomDir.y += Random.Range(-1.0f, 1.0f);

            float singleStep = rotateSpeed * Time.deltaTime;

            Vector3 newDirection = Vector3.RotateTowards(transform.forward, randomDir, singleStep, 0.0f);

            // Draw a ray pointing at our target in
            Debug.DrawRay(transform.position, newDirection, Color.red);

            // Calculate a rotation a step closer to the target and applies rotation to this object
            transform.rotation = Quaternion.LookRotation(newDirection);

            var toAdd = transform.forward * speed * Time.deltaTime;
            transform.position += toAdd;

            timeLeft -= Time.deltaTime;
            yield return null;
        }
        Destroy(this.gameObject);
    }
}