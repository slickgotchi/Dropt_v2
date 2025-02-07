using UnityEngine;

public class DisintegrationController : MonoBehaviour
{
    public Material disintegrationMaterial;
    public float disintegrationTime = 2f;

    private float disintegrateProgress = 0f;
    private bool isDisintegrating = false;

    void Update()
    {
        if (isDisintegrating)
        {
            disintegrateProgress += Time.deltaTime / disintegrationTime;
            disintegrationMaterial.SetFloat("_DisintegrateProgress", disintegrateProgress);

            if (disintegrateProgress >= 1f)
            {
                isDisintegrating = false;
                // Destroy the object or deactivate it
                Destroy(gameObject);
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            StartDisintegration();
        }
    }

    public void StartDisintegration()
    {
        isDisintegrating = true;
    }
}
