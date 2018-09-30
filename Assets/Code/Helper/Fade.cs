using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    private float FadeRate = 0.5f;
    private Image image;
    private float targetAlpha;
    // Use this for initialization
    void Start()
    {
        this.image = this.GetComponent<Image>();
        if (this.image == null)
        {
            Debug.LogError("Error: No image on " + this.name);
        }
        this.targetAlpha = this.image.color.a;

        Color curColor = this.image.color;
        curColor.a = 0;
        this.image.color = curColor;
        
    }

    // Update is called once per frame
    void Update()
    {
        Color curColor = this.image.color;
        float alphaDiff = Mathf.Abs(curColor.a - this.targetAlpha);
        if (alphaDiff > 0.0001f)
        {
            curColor.a = Mathf.Lerp(curColor.a, targetAlpha, this.FadeRate * Time.deltaTime);
            this.image.color = curColor;
        }
    }

    public void FadeOut(float rate)
    {
        FadeRate = rate;
        this.targetAlpha = 0.0f;
    }

    public void FadeIn(float rate)
    {
        FadeRate = rate;
        this.targetAlpha = 1.0f;
    }
}