using System.Collections.Generic;
using UnityEngine;
public class SymbolLoader : MonoBehaviour
{
    private Dictionary<string, Sprite> _imageDictionary;

    void Awake()
    {
        _imageDictionary = new Dictionary<string, Sprite>();

        // 모든 이미지 로드
        Sprite[] loadedImages = Resources.LoadAll<Sprite>("Symbols");

        // 이미지 로드 성공 여부 확인 및 딕셔너리에 추가
        if (loadedImages.Length > 0)
        {
            // Debug.Log($"Loaded {loadedImages.Length} images from Resources/Symbols.");

            foreach (var image in loadedImages)
            {
                if (!_imageDictionary.ContainsKey(image.name))
                {
                    _imageDictionary.Add(image.name, image);
                    // Debug.Log($"Added image '{image.name}' to dictionary.");
                }
                else
                {
                    Debug.LogWarning($"Duplicate image name '{image.name}' detected. Skipping.");
                }
            }
        }
        else
        {
            Debug.LogWarning("No images found in Resources/Symbols.");
        }
    }
    /// <summary>
    /// 이미지 이름으로 이미지를 가져온다.
    /// </summary>
    public Sprite GetImageByName(string name)
    {
        if (_imageDictionary.TryGetValue(name, out Sprite image))
        {
            return image;
        }
        else
        {
            Debug.LogWarning($"Image with name '{name}' not found.");
            return null;
        }
    }
}