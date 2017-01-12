using UnityEngine;
using System.Collections;

public class InfoObjectScript : MonoBehaviour {

    public bool isAi;

	void Start () {
        DontDestroyOnLoad(gameObject);
	}
	
}
