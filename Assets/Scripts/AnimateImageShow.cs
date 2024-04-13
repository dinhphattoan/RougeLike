using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimateImageShow : MonoBehaviour
{

    public Image m_Image;
    public RectTransform rectTransform;
    public Sprite[] m_SpriteArray;
    public float m_Speed = .02f;

    public SpriteRenderer spritePlayerRenderer;
    bool IsDone;
    private void Start() {
    }
    public void Func_PlayUIAnim()
    {
        IsDone = false;
        StartCoroutine(Func_PlayAnimUI());
    }

    public void Func_StopUIAnim()
    {
        IsDone = true;
        StopCoroutine(Func_PlayAnimUI());
    }
    IEnumerator Func_PlayAnimUI()
    {
        yield return new WaitForSeconds(m_Speed);
        m_Image.sprite = spritePlayerRenderer.sprite;
        rectTransform.sizeDelta = m_Image.sprite.rect.size*3;
        if (IsDone == false)
            StartCoroutine(Func_PlayAnimUI());
    }
}