using OpenAI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class TestAI : MonoBehaviour
{
    public Player testPlayer;
    public InputField inputField; // 유니티 인스펙터에서 연결

    public Transform[] targetTransforms;

    // 전역 이벤트 선언
    public static event Action<string> OnAIResponse;

    private List<ChatMessage> chatMessages;
    private OpenAIApi openAIApi;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        openAIApi = new OpenAIApi();
        SetRole();
        AddSpot(targetTransforms[0]);
        AddSpot(targetTransforms[1]);
        AddSpot(targetTransforms[2]);
        AddSpot(targetTransforms[3]);

        inputField.onEndEdit.RemoveAllListeners();
        inputField.onEndEdit.AddListener(OnInputEnd);
    }

    public void SetRole()
    {
        chatMessages = new List<ChatMessage>();

        string role =
        @"당신은 Unity 2D 로그라이크 생존 게임의 '행동 해석 AI'입니다.
플레이어의 명령을 분석하여 실행 가능한 행동을 JSON 형식으로 출력하세요.
JSON 이외의 문장은 절대 포함하지 마세요.

### 출력 형식 명세
아래 JSON 스키마를 **엄격히** 따르세요.

{
  ""action"": string,                // 수행할 행동 (예: ""move"", ""rest"", ""eat"")
  ""target"": string | null,         // 행동 대상 이름 (없으면 null)
  ""position"": {                    // 좌표 객체
      ""x"": float,                  // X 좌표 (소수점 한 자리까지)
      ""y"": float                   // Y 좌표 (소수점 한 자리까지)
  },
  ""success"": boolean,              // 명령 유효 여부
  ""message"": string                // Unity 로그에 표시될 간단한 결과 문장
}

### 추가 규칙
1. **절대** JSON 외의 문장, 주석, 설명을 출력하지 마세요.
2. **action 필드 규칙**
   - 허용된 행동: ""move"", ""rest"", ""eat""
3. **target 필드**
   - ""eat"" 행동 시 반드시 음식 이름(예: ""빵"", ""피자"", ""물"", ""오염된 물"", ""썩은 햇반"")을 지정하세요.
   - ""move"" 또는 ""rest"" 행동 시에는 항상 null로 설정하세요.
4. **position 필드**
   - ""move"" 시 절대 좌표를 입력합니다.
   - ""rest"", ""eat"" 행동 시 항상 { ""x"": 0.0, ""y"": 0.0 }을 출력합니다.
5. **success 필드**
   - 명령이 유효하면 true, 잘못된 명령이면 false를 출력합니다.
6. **message 필드**
   - Unity Debug.Log()로 바로 출력할 짧은 한국어 설명입니다.
   - 예시: ""빵을 먹었다."", ""A 지점으로 이동했다."", ""휴식을 취했다.""
7. 좌표 값은 항상 소수점 한 자리까지만 표현합니다 (예: 3.0, -2.5).
8. JSON의 모든 키 이름은 반드시 쌍따옴표("" "")로 감싸야 합니다.

### 출력 예시 1 (이동)
{
  ""action"": ""move"",
  ""target"": null,
  ""position"": { ""x"": 4.0, ""y"": 3.0 },
  ""success"": true,
  ""message"": ""B 지점으로 이동했다.""
}

### 출력 예시 2 (휴식)
{
  ""action"": ""rest"",
  ""target"": null,
  ""position"": { ""x"": 0.0, ""y"": 0.0 },
  ""success"": true,
  ""message"": ""휴식을 취했다.""
}

### 출력 예시 3 (음식 섭취)
{
  ""action"": ""eat"",
  ""target"": ""피자"",
  ""position"": { ""x"": 0.0, ""y"": 0.0 },
  ""success"": true,
  ""message"": ""피자를 먹었다.""
}";

        ChatMessage systemMessage = new ChatMessage
        {
            Role = "system",
            Content = role
        };
        chatMessages.Add(systemMessage);
    }

    public void AddSpot(Transform t)
    {
        Debug.Log($"AddSpot: {t.name} at {t.position}");
        string spotNameInfo = $"지역 이름: {t.name}";
        string spotPosInfo = t.name + " 좌표: ( X: " + t.position.x + ", Y: " + t.position.y + " )";

        chatMessages.Add(new ChatMessage
        {
            Role = "system",
            Content = spotNameInfo + "\n" + spotPosInfo
        });

    }

    public void DeleteSpot(string spotName)
    {
        chatMessages.Add(new ChatMessage
        {
            Role = "system",
            Content = "지역 삭제: " + spotName + " (이 지역에 대한 모든 정보가 제거되었습니다.)"
        });
    }

    private void OnInputEnd(string text)
    {
        // 엔터키로 입력 종료된 경우 실행
        if (!string.IsNullOrWhiteSpace(text))
        {
            GetResponse(text.Trim());
            inputField.text = ""; // 입력창 초기화
            inputField.ActivateInputField(); // 커서 유지
        }
    }

    public async void GetResponse(string question)
    {
        if (question.Length < 1)
        {
            return;
        }

        string role = "user";


        ChatMessage newMessage = new ChatMessage
        {
            // Content = courtManager.GetAskFieldText(),
            Content = question,
            Role = role
        };

        chatMessages.Add(newMessage);

        CreateChatCompletionRequest request = new CreateChatCompletionRequest
        {
            Messages = chatMessages,
            Model = "gpt-4o-mini"
        };

        var response = await openAIApi.CreateChatCompletion(request);

        if (response.Choices != null && response.Choices.Count > 0)
        {
            var chatResponse = response.Choices[0].Message;
            chatMessages.Add(chatResponse);

            // 여기서 직접 호출 대신 이벤트로 전달
            OnAIResponse?.Invoke(chatResponse.Content);

            Debug.Log(chatResponse.Content);
        }
    }
}
