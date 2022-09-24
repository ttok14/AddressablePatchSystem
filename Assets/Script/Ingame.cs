using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class Ingame : MonoBehaviour
{
    public Image img;
    public Text txtResult;

    public string TargetSpriteKey;

    private void Start()
    {
        Addressables.LoadAssetAsync<Sprite>(TargetSpriteKey).Completed += (result) =>
        {
            if (result.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                txtResult.text = "����!";
                img.sprite = result.Result;
            }
            else
            {
                txtResult.text = "����!";
                img.sprite = null;
            }
        };
    }
}
