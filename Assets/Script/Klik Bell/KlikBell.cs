using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KlikBell : MonoBehaviour
{
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnMouseDown()
    {
        audioSource.Play();
    }
}
