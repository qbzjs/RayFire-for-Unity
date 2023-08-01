using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HeroFishing.Battle {

    public class SimulationSceneManager : MonoBehaviour {
        public static SimulationSceneManager Instance;

        [SerializeField]
        public Hero MyHero;


        // Start is called before the first frame update
        void Awake() {
            Instance = this;
        }

    }

}