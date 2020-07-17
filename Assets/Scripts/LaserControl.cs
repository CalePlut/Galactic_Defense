using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserControl : MonoBehaviour
{
    public GameObject beamPrefab, beamStartPrefab, beamEndPrefab;

    private List<GameObject> beamStarts;
    private List<GameObject> beamEnds;
    private List<GameObject> beams;
    // private List<LineRenderer> lines;

    public List<GameObject> emitters;

    [Header("Adjustable Variables")]
    public float beamEndOffset = 1f; //How far from the raycast hit point the end effect is positioned

    public float textureScrollSpeed = 8f; //How fast the texture scrolls along the beam
    public float textureLengthScale = 3; //Length of the beam texture

    //  public List<GameObject> laserObjects;
    // public List<ParticleSystem> cannons;

    // Start is called before the first frame update
    private void Start()
    {
        beamStarts = new List<GameObject>();
        beamEnds = new List<GameObject>();
        beams = new List<GameObject>();
    }

    public void fireLasers(bool _upgrade, Vector3 target)
    {
        var toFire = 1;
        if (_upgrade) { toFire = 2; }
        StartCoroutine(laserStagger(target, toFire));
    }

    #region new Laser

    private void addLaser(Vector3 startLoc, Vector3 endLoc)
    {
        //Instantiates laser parts and aligns them
        var start = Instantiate(beamStartPrefab, Vector3.zero, Quaternion.identity);
        var end = Instantiate(beamEndPrefab, Vector3.zero, Quaternion.identity);
        var beam = Instantiate(beamPrefab, Vector3.zero, Quaternion.identity);
        var line = beam.GetComponent<LineRenderer>();

        alignLaser(startLoc, endLoc, start, end, line);

        beamStarts.Add(start);
        beamEnds.Add(end);
        beams.Add(beam);
    }

    private void removeLaser()
    {
        var start = beamStarts[0];
        var end = beamEnds[0];
        var beam = beams[0];

        beamStarts.RemoveAt(0);
        beamEnds.RemoveAt(0);
        beams.RemoveAt(0);

        Destroy(start);
        Destroy(end);
        Destroy(beam);
    }

    private void alignLaser(Vector3 start, Vector3 target, GameObject beamStart, GameObject beamEnd, LineRenderer line)
    {
        RaycastHit hit;
        if (Physics.Linecast(start, target, out hit))
        {
            beamEnd.transform.position = hit.point;
        };

        line.useWorldSpace = false;

        line.positionCount = 2;
        line.SetPosition(0, start);
        beamStart.transform.position = start;
        //beamEnd.transform.position = target;
        line.SetPosition(1, target);

        beamStart.transform.LookAt(beamEnd.transform.position);
        beamEnd.transform.LookAt(beamStart.transform.position);

        float distance = Vector3.Distance(start, target);
        line.sharedMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
        line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);
    }

    private IEnumerator laserStagger(Vector3 target, int toFire)
    {
        if (toFire > emitters.Count) { toFire = emitters.Count; }
        for (int i = 0; i < toFire; i++)
        {
            addLaser(emitters[i].transform.position, target);
            yield return new WaitForSeconds(0.098f);
        }
        yield return new WaitForSeconds(1.5f);
        for (int i = 0; i < toFire; i++)
        {
            removeLaser();
        }
    }

    #endregion new Laser

    //public void fireSingleLaser(Vector3 target)
    //{
    //    var laser = laserObjects[Random.Range(0, laserObjects.Count)];
    //    StartCoroutine(singleFire(target, 1.0f, laser.GetComponent<LineRenderer>()));
    //}

    //IEnumerator laserFire(Vector3 position, float duration, int toFire)
    //{
    //    if (toFire > laserObjects.Count-1) { toFire = laserObjects.Count - 1; }
    //    //Begin by firing each one up
    //    for(int i=0; i<toFire; i++)
    //    {
    //        var laser = laserObjects[i];
    //        laser.SetActive(true);
    //        var laserBeam = laser.GetComponent<LineRenderer>();
    //        laserBeam.SetPosition(1, jitter(position)); //Targets within 0.5f of the target
    //        StartCoroutine(laserWarm(laserBeam));
    //        yield return new WaitForSeconds(Random.Range(0.1f, 0.25f));
    //    }
    //    yield return new WaitForSeconds(duration);
    //    foreach(GameObject laser in laserObjects)
    //    {
    //        laser.SetActive(false);
    //    }
    //}

    //IEnumerator singleFire(Vector3 position, float duration, LineRenderer lase)
    //{
    //    lase.gameObject.SetActive(true);
    //    var targetLoc = jitter(position);
    //    StartCoroutine(laserWarm(lase));
    //    var timeLeft = duration;
    //    while (timeLeft > 0)
    //    {
    //        lase.SetPosition(1, targetLoc);
    //        timeLeft -= Time.deltaTime;
    //        yield return null;
    //    }

    //    lase.gameObject.SetActive(false);
    //}

    //IEnumerator laserWarm(LineRenderer lase)
    //{
    //    var col = lase.material.color;
    //    col.a = 0.0f;
    //    while (col.a < 3.5f)
    //    {
    //        col.a += 4.0f * Time.deltaTime;
    //        lase.material.color = col;
    //        yield return null;
    //    }
    //}

    //Vector3 jitter(Vector3 original)
    //{
    //    var randX = original.x + Random.Range(-0.5f, 0.5f);
    //    var randY = original.y + Random.Range(-0.0f, 1.0f);
    //    return new Vector3(randX, randY, original.z);
    //}
}