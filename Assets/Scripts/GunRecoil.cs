using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    Vector3 originalPosition;

    bool recoiling = false;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            recoiling = true;
        }
        if (recoiling)
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                originalPosition + new Vector3(0, 0, -0.5f),
                Time.deltaTime * 20
            );

            if (Vector3.Distance(transform.localPosition,
                originalPosition + new Vector3(0, 0, -0.5f)) < 0.05f)
            {
                recoiling = false;
            }
        }
        else
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                originalPosition,
                Time.deltaTime * 10
            );
        }
    }
}