using UnityEngine;

public class skyboxManager : MonoBehaviour
{
    public Material[] skyboxes;
    private Camera cam;

    // Start is called before the first frame update
    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    public void warp()
    {
        cam.clearFlags = CameraClearFlags.Color;
    }

    public void exitWarp()
    {
        cam.clearFlags = CameraClearFlags.Skybox;
        var randomSkybox = Random.Range(0, skyboxes.Length);
        RenderSettings.skybox = skyboxes[randomSkybox];
    }
}