using UnityEngine;

public class SetUPShaderProperty : MonoBehaviour
{
    private int _setMainColor = 0;
    // Start is called before the first frame update
    private void Start()
    {
        _setMainColor = Shader.PropertyToID("_SetMainColor");
        
        Debug.Log("setMainColor Id : " + _setMainColor);
        Shader.SetGlobalColor(_setMainColor, new Color(1,0,0));
    }
}
