using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Role : MonoBehaviour {
    [SerializeField]
    Animator MyAni;

    public void SetAniTrigger(string name) {
        MyAni.SetTrigger(name);
    }

}
