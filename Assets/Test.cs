using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine(FiveSeconds());
	}
	

    IEnumerator FiveSeconds()
    {
        yield return new WaitForSeconds(30);
        UploadImages.Instance.UnitTests();

    }

}
