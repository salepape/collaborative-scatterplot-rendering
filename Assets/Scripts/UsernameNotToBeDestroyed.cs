using UnityEngine;

public class UsernameNotToBeDestroyed : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
