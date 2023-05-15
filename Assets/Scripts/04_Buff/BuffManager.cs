using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using SpreadSheet;
using UnityEditor;
using UnityEngine;

public sealed class BuffManager : Singleton<BuffManager>
{
	// < 코드, 명칭, 버프 >
	private Dictionary<E_BuffType, BuffDictionary> m_BuffDictionary;

	private void Awake()
	{
		Initialize();

		// 엑셀에서 읽어온 데이터로 딕셔너리에 버프 생성
		//LoadBuffAll();
	}

	private void Initialize()
	{
		m_BuffDictionary = new Dictionary<E_BuffType, BuffDictionary>();

		for (E_BuffType i = 0; i != E_BuffType.Max; ++i)
		{
			m_BuffDictionary[i] = new BuffDictionary();
		}
	}

#if UNITY_EDITOR
	[ContextMenu("LoadBuffAll")]
	public void LoadBuffAll()
	{
		Initialize();

		DataSet dataSet = new DataSet();
		SpreadSheetManager.Instance.LoadJsonData(dataSet);

		StringBuilder sb = new StringBuilder();

		sb.AppendLine("\t\tswitch (title)");
		sb.AppendLine("\t\t{");

		for (E_BuffType buffType = E_BuffType.Buff; buffType != E_BuffType.Max - 1; ++buffType)
		{
			sb.Append("#region ");
			sb.AppendLine(BuffEnumUtil.EnumToKorString<E_BuffType>(buffType));
			LoadBuff(sb, dataSet, buffType);
			sb.AppendLine("#endregion");
		}

		sb.AppendLine("\t\t}");

		CreateBuffFunc(sb);
	}
	public void LoadBuff(StringBuilder sb, DataSet dataSet, E_BuffType buffType)
	{
		string sheetName = string.Concat(BuffEnumUtil.EnumToKorString(buffType), " 목록");

		DataRow[] rows = dataSet.Tables[sheetName].Select();

		foreach (var row in rows)
		{
			#region 명칭
			// 명칭 불러오기
			string title = row[0] as string;
			#endregion
			#region 코드
			// 코드 불러오기
			string codeStr = row[1] as string;

			// 자료형 파싱
			if (int.TryParse(codeStr, out int code) == false)
			{
				Debug.LogError("버프 코드 불러오기 오류! | 코드: " + codeStr);
				return;
			}
			#endregion
			#region 효과 종류
			// 효과 종류 불러오기
			string effectTypeStr = row[2] as string;

			// 한글 -> 영어 전환
			switch (effectTypeStr)
			{
				case "스탯형":
					effectTypeStr = "Stat";
					break;
				case "무기형":
					effectTypeStr = "Weapon";
					break;
				case "전투형":
					effectTypeStr = "Combat";
					break;
				default:
					Debug.LogError("버프 효과 종류 불러오기 오류! | 버프 효과 종류: " + effectTypeStr);
					return;
			}

			// 자료형 파싱
			if (System.Enum.TryParse(effectTypeStr, out E_BuffEffectType effectType) == false)
			{
				Debug.LogError("버프 효과 종류 전환 오류! | 버프 효과 종류: " + effectTypeStr);
				return;
			}
			#endregion
			#region 등급
			// 등급 불러오기
			string gradeStr = row[3] as string;

			// 자료형 파싱
			if (System.Enum.TryParse(gradeStr, out E_BuffGrade grade) == false)
			{
				Debug.LogError("버프 등급 전환 오류! | 버프 등급: " + gradeStr);
				return;
			}
			#endregion
			#region 최대 스택
			// 최대 스택 불러오기
			string maxStackStr = row[4] as string;

			// 자료형 파싱
			if (int.TryParse(maxStackStr, out int maxStack) == false)
			{
				Debug.LogError("버프 최대 스택 전환 오류! | 버프 최대 스택: " + maxStackStr);
				return;
			}
			#endregion
			#region 적용 무기 타입
			// 적용되는 무기 타입 불러오기
			string weaponStr = row[5] as string;

			// 한글 -> 영어 전환
			switch (weaponStr)
			{
				case "공통":
					weaponStr = "All";
					break;
				case "근거리 무기":
					weaponStr = "Melee";
					break;
				case "원거리 무기":
					weaponStr = "Ranged";
					break;
				default:
					Debug.LogError("버프 적용 무기 타입 전환 오류! | 버프 적용 무기: " + weaponStr);
					return;
			}

			// 자료형 파싱
			if (System.Enum.TryParse(weaponStr, out E_BuffWeapon weapon) == false)
			{
				Debug.LogError("버프 적용 무기 타입 전환 오류! | 버프 적용 무기: " + weaponStr);
				return;
			}
			#endregion
			#region 설명
			string description = row[7] as string;
			#endregion

			CreateBuffData(title, code, buffType, effectType, grade, maxStack, weapon, description);

			AppendBuffFunc(sb, title);
		}

		AssetDatabase.SaveAssets();
	}
	private void CreateBuffData(string title, int code, E_BuffType buffType, E_BuffEffectType effectType, E_BuffGrade grade, int maxStack, E_BuffWeapon weapon, string description)
	{
		BuffData buffData = CreateBuffScriptableObject(title, code, buffType, effectType, grade, maxStack, weapon, description);

		CreateBuffScript(title, buffType);

		m_BuffDictionary[buffType].Add((code, title), buffData);
	}
	public BuffData CreateBuffScriptableObject(string title, int code, E_BuffType buffType, E_BuffEffectType effectType, E_BuffGrade grade, int maxStack, E_BuffWeapon weapon, string description)
	{
		BuffData buffData = new BuffData(title, code, buffType, effectType, grade, maxStack, weapon, description);

		string path = Path.Combine(Application.dataPath, "DataBase", "Scriptable Object", buffType.ToString());

		if (Directory.Exists(path) == false)
			Directory.CreateDirectory(path);

		string file = Path.Combine("Assets", "DataBase", "Scriptable Object", buffType.ToString(), title + ".asset");

		AssetDatabase.CreateAsset(buffData, file);

		return buffData;
	}
	public void CreateBuffScript(string title, E_BuffType buffType)
	{
		string path = Path.Combine(Application.dataPath, "DataBase", "Script", buffType.ToString());

		if (Directory.Exists(path) == false)
			Directory.CreateDirectory(path);

		string file = Path.Combine(path, title + ".cs");
		string template = Path.Combine(Application.dataPath, "DataBase", "Template", "BuffScriptTemplate.txt");
		string className = title.Replace(' ', '_');

		StringBuilder sb = new StringBuilder();

		sb.Append(File.ReadAllText(template));
		sb.Replace("$Buff", className);

		try
		{
			byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());

			using (FileStream fs = new FileStream(file, FileMode.CreateNew, FileAccess.Write))
			{
				fs.Write(new ReadOnlySpan<byte>(bytes));
			}
		}
		catch (Exception)
		{

		}
		//File.WriteAllText(file, sb.ToString());
	}
	private void CreateBuffFunc(StringBuilder sb, [System.Runtime.CompilerServices.CallerFilePath] string path = "")
	{
		string code = File.ReadAllText(path);

		var str = code.Split("\t\t// $BuffFunc");

		string allCase = sb.ToString();
		sb.Clear();

		sb.Append(str[0]);
		sb.AppendLine("\t\t// $BuffFunc");
		sb.Append(allCase);
		sb.Append("\t\t// $BuffFunc");
		sb.Append(str[2]);

		File.WriteAllText(path, sb.ToString());
	}
	private void AppendBuffFunc(StringBuilder sb, string title)
	{
		string className = title.Replace(' ', '_');

		sb.Append("\t\t\tcase \"");
		sb.Append(title);
		sb.AppendLine("\":");
		sb.Append("\t\t\t\treturn new ");
		sb.Append(className);
		sb.AppendLine("(buffData);");
	}
#endif

	public BuffData GetBuffData(int code)
	{
		BuffData buff = null;

		for (E_BuffType i = 0; i < E_BuffType.Max; ++i)
		{
			if (TryGetBuffData(i, code, out buff) == true)
				break;
		}

		return buff;
	}
	public bool TryGetBuffData(E_BuffType buffType, int code, out BuffData buff)
	{
		BuffDictionary buffDictionary = m_BuffDictionary[buffType];

		if (buffDictionary == null)
		{
			buff = null;

			Debug.LogError("버프 목록 생성 안됨");
			return false;
		}

		if (buffDictionary.TryGetValue(code, out buff))
		{
			buff = null;

			Debug.LogError("버프 가져오기 실패");
			return false;
		}

		return true;
	}
	public BuffData GetBuffData(string title)
	{
		BuffData buff = null;

		for (E_BuffType i = 0; i < E_BuffType.Max; ++i)
		{
			if (TryGetBuffData(i, title, out buff) == true)
				break;
		}

		return buff;
	}
	public bool TryGetBuffData(E_BuffType buffType, string title, out BuffData buff)
	{
		BuffDictionary buffDictionary = m_BuffDictionary[buffType];

		if (buffDictionary == null)
		{
			buff = null;

			Debug.LogError("버프 목록 생성 안됨");
			return false;
		}

		if (buffDictionary.TryGetValue(title, out buff) == false)
		{
			buff = null;

			Debug.LogError("버프 가져오기 실패");
			return false;
		}

		return true;
	}

	public AbstractBuff CreateBuff(int code)
	{
		BuffData buffData = GetBuffData(code);

		return CreateBuff(buffData.title);
	}
	public AbstractBuff CreateBuff(string title)
	{
		BuffData buffData = GetBuffData(title);

		if (buffData == null)
			return null;

		// $BuffFunc
		switch (title)
		{
#region 버프
			case "체력 증가":
				return new 체력_증가(buffData);
			case "재생":
				return new 재생(buffData);
			case "빠른 재생":
				return new 빠른_재생(buffData);
			case "회복량 증가":
				return new 회복량_증가(buffData);
			case "방어력 증가":
				return new 방어력_증가(buffData);
			case "회피율 증가":
				return new 회피율_증가(buffData);
			case "공격력 증가":
				return new 공격력_증가(buffData);
			case "공격 속도 증가":
				return new 공격_속도_증가(buffData);
			case "이동 속도 증가":
				return new 이동_속도_증가(buffData);
			case "공격 범위 증가":
				return new 공격_범위_증가(buffData);
			case "사거리 증가":
				return new 사거리_증가(buffData);
			case "투사체 속도 증가":
				return new 투사체_속도_증가(buffData);
			case "공격 횟수 증가":
				return new 공격_횟수_증가(buffData);
			case "치명타 확률 증가":
				return new 치명타_확률_증가(buffData);
			case "치명타 대미지 증가":
				return new 치명타_대미지_증가(buffData);
			case "점프 추가":
				return new 점프_추가(buffData);
			case "전방향 대쉬":
				return new 전방향_대쉬(buffData);
			case "대쉬 횟수 증가":
				return new 대쉬_횟수_증가(buffData);
			case "대쉬 충전 속도 증가":
				return new 대쉬_충전_속도_증가(buffData);
			case "대쉬 무적":
				return new 대쉬_무적(buffData);
			case "무적시간 증가":
				return new 무적시간_증가(buffData);
#endregion
#region 디버프
			case "체력 감소":
				return new 체력_감소(buffData);
			case "느린 재생":
				return new 느린_재생(buffData);
			case "회복량 감소":
				return new 회복량_감소(buffData);
			case "방어력 감소":
				return new 방어력_감소(buffData);
			case "회피율 감소":
				return new 회피율_감소(buffData);
			case "공격력 감소":
				return new 공격력_감소(buffData);
			case "공격 속도 감소":
				return new 공격_속도_감소(buffData);
			case "이동 속도 감소":
				return new 이동_속도_감소(buffData);
			case "공격 범위 감소":
				return new 공격_범위_감소(buffData);
			case "사거리 감소":
				return new 사거리_감소(buffData);
			case "투사체 속도 감소":
				return new 투사체_속도_감소(buffData);
			case "패링 확률 감소":
				return new 패링_확률_감소(buffData);
#endregion
		}
		// $BuffFunc

		return null;
	}
	public AbstractBuff CreateBuff(BuffData buffData)
	{
		return CreateBuff(buffData.title);
	}

	public class BuffDictionary
	{
		public Dictionary<string, int> m_NameDictionary = new Dictionary<string, int>();
		public Dictionary<int, BuffData> m_BuffDictionary = new Dictionary<int, BuffData>();

		public BuffData this[int code]
		{
			get
			{
				if (m_BuffDictionary.TryGetValue(code, out BuffData buff) == false)
					return null;

				return buff;
			}
			set
			{
				m_BuffDictionary[code] = value;
			}
		}
		public BuffData this[string name]
		{
			get
			{
				if (m_NameDictionary.TryGetValue(name, out int code) == false)
					return null;

				return this[code];
			}
			set
			{
				if (m_NameDictionary.TryGetValue(name, out int code) == false)
					return;

				this[code] = value;
			}
		}

		public void Add((int code, string name) key, BuffData buff)
		{
			m_NameDictionary.Add(key.name, key.code);
			m_BuffDictionary.Add(key.code, buff);
		}
		public void Add(int code, string name, BuffData buff)
		{
			m_NameDictionary.Add(name, code);
			m_BuffDictionary.Add(code, buff);
		}
		public bool TryAdd((int code, string name) key, BuffData buff)
		{
			if (m_NameDictionary.TryAdd(key.name, key.code) == false)
				return false;
			if (m_BuffDictionary.TryAdd(key.code, buff) == false)
				return false;

			return true;
		}
		public bool TryAdd(int code, string name, BuffData buff)
		{
			if (m_NameDictionary.TryAdd(name, code) == false)
				return false;
			if (m_BuffDictionary.TryAdd(code, buff) == false)
				return false;

			return true;
		}
		public bool TryGetValue(int code, out BuffData buff)
		{
			return m_BuffDictionary.TryGetValue(code, out buff);
		}
		public bool TryGetValue(string name, out BuffData buff)
		{
			if (m_NameDictionary.TryGetValue(name, out int code) == false)
			{
				buff = default(BuffData);
				return false;
			}

			return TryGetValue(code, out buff);
		}
	}
}
