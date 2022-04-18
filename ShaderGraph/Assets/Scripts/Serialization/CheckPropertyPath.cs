using UnityEditor;
using UnityEngine;

public class CheckPropertyPath : MonoBehaviour
{
    // Test후 주석 처리
    // [InitializeOnLoadMethod]
    static void PropertyChecker()
    {
        var so = new SerializedObject(Texture2D.whiteTexture);

        var pop = so.GetIterator();

        while (pop.NextVisible(true))
        {
            Debug.Log("PropertyChecker : " +  pop.propertyPath);
        }
    }
}