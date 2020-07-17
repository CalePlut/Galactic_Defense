using UnityEngine;

public class skyboxManager : MonoBehaviour
{
    public Material[] skyboxes;
    private Camera camera;

    // Start is called before the first frame update
    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    public void warp()
    {
        camera.clearFlags = CameraClearFlags.Color;
    }

    public void exitWarp()
    {
        camera.clearFlags = CameraClearFlags.Skybox;
        var randomSkybox = Random.Range(0, skyboxes.Length);
        RenderSettings.skybox = skyboxes[randomSkybox];
    }
}