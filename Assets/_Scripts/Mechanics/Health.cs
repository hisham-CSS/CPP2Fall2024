using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;

    private int _health = 1;

    public int health
    {
        get => _health;
        set
        {
            _health = value;

            if (_health > maxHealth)
                _health = maxHealth;

            Debug.Log($"Health value is {_health}");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
