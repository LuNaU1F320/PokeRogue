using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBar : MonoBehaviour
{
    [SerializeField] GameObject health;
    [HideInInspector] public static float HpBarSpeed = 1.0f;

    public void SetHp(float hpNormalized)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1.0f);
    }
    public IEnumerator SetHpSmooth(float newhp)
    {
        float curHp = health.transform.localScale.x;
        float changeAmt = curHp - newhp;

        while (curHp - newhp > Mathf.Epsilon)
        {
            curHp -= changeAmt * Time.deltaTime * HpBarSpeed;
            health.transform.localScale = new Vector3(curHp, 1.0f);
            yield return null;
        }
        health.transform.localScale = new Vector3(newhp, 1.0f);
    }
}
