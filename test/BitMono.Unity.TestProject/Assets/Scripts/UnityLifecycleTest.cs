using UnityEngine;
using System.Collections;

/// <summary>
/// Unity lifecycle test script for BitMono obfuscation
/// Tests Unity-specific methods that should be protected
/// </summary>
public class UnityLifecycleTest : MonoBehaviour
{
    [Header("Unity Lifecycle Test")]
    [SerializeField]
    private string testString = "Unity lifecycle test";
    
    [SerializeField]
    private int testInt = 100;
    
    private string privateString = "This should be obfuscated";
    private int privateInt = 200;
    
    // Unity Lifecycle Methods - These should be protected by BitMono's critical analysis
    void Awake()
    {
        Debug.Log("Awake called");
        InitializePrivateData();
    }
    
    void OnEnable()
    {
        Debug.Log("OnEnable called");
    }
    
    void Start()
    {
        Debug.Log("Start called");
        StartCoroutine(TestCoroutine());
    }
    
    void Update()
    {
        // This should be protected
        if (Input.GetKeyDown(KeyCode.Return))
        {
            TestInput();
        }
    }
    
    void LateUpdate()
    {
        // This should be protected
    }
    
    void FixedUpdate()
    {
        // This should be protected
    }
    
    void OnDisable()
    {
        Debug.Log("OnDisable called");
    }
    
    void OnDestroy()
    {
        Debug.Log("OnDestroy called");
    }
    
    // Unity Event Methods - These should be protected
    void OnMouseDown()
    {
        Debug.Log("OnMouseDown called");
    }
    
    void OnMouseUp()
    {
        Debug.Log("OnMouseUp called");
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter called");
    }
    
    void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerExit called");
    }
    
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("OnCollisionEnter called");
    }
    
    // Private methods that should be obfuscated
    private void InitializePrivateData()
    {
        privateString = "Initialized private data";
        privateInt = 300;
        Debug.Log($"Private data initialized: {privateString}, {privateInt}");
    }
    
    private void TestInput()
    {
        Debug.Log("Input test triggered");
        ProcessPrivateInput();
    }
    
    private void ProcessPrivateInput()
    {
        Debug.Log("Processing private input");
        Debug.Log($"Private string: {privateString}");
        Debug.Log($"Private int: {privateInt}");
    }
    
    private IEnumerator TestCoroutine()
    {
        Debug.Log("Coroutine started");
        yield return new WaitForSeconds(1f);
        Debug.Log("Coroutine after 1 second");
        yield return new WaitForSeconds(1f);
        Debug.Log("Coroutine after 2 seconds");
    }
    
    // Public methods
    public void PublicUnityMethod()
    {
        Debug.Log("Public Unity method called");
        Debug.Log($"Test string: {testString}");
        Debug.Log($"Test int: {testInt}");
    }
    
    public void TestSerializedFields()
    {
        Debug.Log($"Serialized string: {testString}");
        Debug.Log($"Serialized int: {testInt}");
    }
}


