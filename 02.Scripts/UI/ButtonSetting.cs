using UnityEngine;
using UnityEngine.UI;

public class ButtonSetting : MonoBehaviour
{
    void Start()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }
}