using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CostHpNum : MonoBehaviour
{
    Text textHp;

    private void Start()
    {
        textHp = transform.Find("HpText").GetComponent<Text>();
        textHp.gameObject.SetActive(false);

        Canvas canvas = GetComponent<Canvas>();
        Camera uiCam = GameObject.Find("UICam").GetComponent<Camera>();
        canvas.worldCamera = uiCam;
    }

    public void ShowHpNum(Vector3 pos, string s)
    {
        textHp.gameObject.SetActive(true);
        textHp.text = s;
        Color c = textHp.material.color;
        c.a = 1;
        textHp.material.color = c;

        transform.position = pos;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOBlendableMoveBy(new Vector3(0, 1.5f, 0), 0.6f).SetEase(Ease.OutCubic));
        seq.Append(textHp.material.DOFade(0.1f, 0.2f));
        seq.AppendCallback(() => { textHp.gameObject.SetActive(false); } );
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            ShowHpNum(new Vector3(3, 0, 3), "-9999");
        }
    }

}
