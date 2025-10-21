using System.IO;
using UnityEngine;

public class APIKeyManager : MonoBehaviour
{
    private string authFilePath;

    private void Start()
    {
        string targetFolder = GetOpenAIFolderPath();
        authFilePath = Path.Combine(targetFolder, "auth.json");

        // auth.json 생성 또는 덮어쓰기 (조건문 제거)
        Debug.Log(File.Exists(authFilePath) ? "auth.json 파일이 이미 존재합니다. 덮어씌웁니다." : "auth.json 파일이 존재하지 않습니다. 새로 생성합니다.");
        CreateEncryptedAuthFile();
    }

    /// <summary>
    /// .openai 폴더 경로를 가져오고, 존재하지 않으면 생성
    /// </summary>
    private string GetOpenAIFolderPath()
    {
        string userFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        string targetFolder = Path.Combine(userFolder, ".openai");

        // 폴더가 존재하지 않으면 자동 생성 (이미 존재하면 아무 작업 안 함)
        Directory.CreateDirectory(targetFolder);
        Debug.Log($"폴더 확인 및 생성 완료: {targetFolder}");

        return targetFolder;
    }

    /// <summary>
    /// auth.json을 암호화하여 저장 (존재하지 않으면 새로 생성, 있으면 덮어쓰기)
    /// </summary>
    private void CreateEncryptedAuthFile()
    {
        string sourceAuthFilePath = Path.Combine(Application.dataPath, "auth.json");

        if (!File.Exists(sourceAuthFilePath))
        {
            Debug.LogError($"Assets 폴더 내 auth.json 파일을 찾을 수 없습니다: {sourceAuthFilePath}");
            return;
        }

        try
        {
            string jsonContent = File.ReadAllText(sourceAuthFilePath);
            if (string.IsNullOrEmpty(jsonContent))
            {
                Debug.LogError("auth.json 파일의 내용이 비어 있습니다.");
                return;
            }

            AuthData tmpAuthData = JsonUtility.FromJson<AuthData>(jsonContent);
            if (tmpAuthData == null)
            {
                Debug.LogError("auth.json 파일을 올바르게 파싱하지 못했습니다.");
                return;
            }

            string encryptedJsonContent = EncryptionHelper.Encrypt(tmpAuthData.GetEncryptedJson());
            File.WriteAllText(authFilePath, encryptedJsonContent);

            Debug.Log($"암호화된 auth.json 파일 생성 완료: {authFilePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"auth.json 파일 처리 중 오류 발생: {ex.Message}");
        }
    }
}

[System.Serializable]
public class AuthData
{
    [SerializeField]
    private string api_key;

    [SerializeField]
    private string organization;

    public string ApiKey
    {
        set { api_key = value; }
        get { return api_key; }
    }

    public string Organization
    {
        set { organization = value; }
        get { return organization; }
    }

    // JSON 파일 저장 시 사용될 암호화된 데이터 반환 (키 이름을 수동 설정)
    public string GetEncryptedJson()
    {
        // 수동으로 JSON 문자열을 생성
        string json = $"{{\"api_key\":\"{api_key}\",\"organization\":\"{organization}\"}}";
        Debug.Log($"암호화된 JSON 내용: {json}");
        return json;
    }

    // 복호화된 JSON을 객체로 변환하는 메서드 추가
    public static AuthData FromDecryptedJson(string json)
    {
        // Unity의 JsonUtility를 사용하여 역직렬화 (자동 매핑)
        return JsonUtility.FromJson<AuthData>(json);
    }
}