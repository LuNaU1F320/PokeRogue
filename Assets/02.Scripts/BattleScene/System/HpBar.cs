using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class HpBar : MonoBehaviour
{
    [SerializeField] GameObject health;
    [HideInInspector] public static float HpBarSpeed = GlobalValue.HpBarSpeed;

    public void SetHp(float hpNormalized)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1.0f);
    }
    // public IEnumerator SetHpSmooth(float newhp)
    // {
    //     float curHp = health.transform.localScale.x;
    //     float changeAmt = curHp - newhp;

    //     while (curHp - newhp > Mathf.Epsilon)
    //     {
    //         curHp -= changeAmt * Time.deltaTime * HpBarSpeed;
    //         health.transform.localScale = new Vector3(curHp, 1.0f);
    //         yield return null;
    //     }
    //     health.transform.localScale = new Vector3(newhp, 1.0f);
    // }
    public async UniTask SetHpSmooth(float newHp)
    {
        float curHp = health.transform.localScale.x;
        float changeAmt = curHp - newHp;

        while (curHp - newHp > Mathf.Epsilon)
        {
            curHp -= changeAmt * Time.deltaTime * HpBarSpeed;
            health.transform.localScale = new Vector3(curHp, 1.0f);
            await UniTask.Yield(); // 다음 프레임까지 대기
        }

        health.transform.localScale = new Vector3(newHp, 1.0f);
    }
}
