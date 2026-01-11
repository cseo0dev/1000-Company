using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 작성자 : 정하윤
public class VitalDisplayUIController : MonoBehaviour
{
    [SerializeField] private List<VitalCellUI> memberUIs;

    private void Start()
    {
        // 테스트 코드
        List<TeamMemberData> teamData = new List<TeamMemberData>
        {
            new TeamMemberData { nickName = "홍길동", grade = EGrade.인턴, CurrentMental = 80 },
            new TeamMemberData { nickName = "김영희", grade = EGrade.대리, CurrentMental = 20 },
            new TeamMemberData { nickName = "김철수", grade = EGrade.과장, CurrentMental = 20 }
        };

        SetTeamMembers(teamData);
    }

    // 전체 팀원 데이터로 UI 초기화
    public void SetTeamMembers(List<TeamMemberData> members)
    {
        for (int i = 0; i < memberUIs.Count; i++)
        {
            if (i < members.Count)
            {
                memberUIs[i].gameObject.SetActive(true);
                memberUIs[i].SetData(members[i]);
            }
            // 데이터가 없는 경우 숨김
            else
            {
                memberUIs[i].gameObject.SetActive(false); 
            }
        }
    }

    // 특정 팀원의 HP만 갱신
    public void UpdateMemberMental(int memberIndex, int mental)
    {
        if (memberIndex < 0 || memberIndex >= memberUIs.Count)
            return;

        memberUIs[memberIndex].UpdateMental(mental);
    }
}
