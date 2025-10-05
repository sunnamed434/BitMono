using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] private string secretPassword = "MySecretPassword123";
    [SerializeField] private int secretNumber = 42;
    
    private void Start()
    {
        Debug.Log("TestScript started - this should be obfuscated!");
        ShowSecretInfo();
    }
    
    private void ShowSecretInfo()
    {
        Debug.Log($"Secret Password: {secretPassword}");
        Debug.Log($"Secret Number: {secretNumber}");
    }
    
    public void PublicMethod()
    {
        Debug.Log("This is a public method that should be obfuscated");
    }
    
    private void PrivateMethod()
    {
        Debug.Log("This is a private method that should be obfuscated");
    }
}