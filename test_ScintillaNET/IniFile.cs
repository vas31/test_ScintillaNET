using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System;



// Файл добавить в проект и поправить имя "namespace" как в проекте (Form1.cs)

// Все параметры методов имеют строковый формат!


namespace test_ScintillaNET
{
	/// <summary>
	/// Класс IniFile предоставляет несколько методов для работы с INI-файлами.
	/// </summary>
	class IniFile
	{
		public static int capacity = 512;

		const string INI_EXT = ".ini";							// расширение INI-файла
		string _path;
		string _exeName = Assembly.GetExecutingAssembly().GetName().Name;

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		static extern long WritePrivateProfileString(string section, string key, string value, string filePath);
		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder retVal, int size, string filePath);
		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		static extern int GetPrivateProfileString(string section, string key, string defaultValue, [In, Out] char[] value, int size, string filePath);



		//-------------------------------------------------------------------------------
		// Путь к файлу конфигурации (может использоваться за пределами методов).
		//
		// IniFile ini = new IniFile(Environment.ExpandEnvironmentVariables("%ProgramFiles%\\AppFolder\\config.ini"));
		//-------------------------------------------------------------------------------
		public IniFile(string iniPath = null)
		{
			_path = new FileInfo(iniPath ?? _exeName + INI_EXT).FullName.ToString();
		}


		//-------------------------------------------------------------------------------
		// Чтение значения указного ключа в указанной секции.
		//
		// ;ini.Read("sectionName", "keyName");
		// ;Возвращает: "" = если параметр не найден
		// ini.Read("sectionName", "keyName", "Value");
		// Возвращает: если параметр не найден = Value (из параметров метода)
		//-------------------------------------------------------------------------------
		//public string Read(string section, string key)
		//{
		//	var retVal = new StringBuilder(255);
		//	GetPrivateProfileString(section ?? _exeName, key, "", retVal, 255, _path);
		//	return retVal.ToString();
		//}
		/// <summary>
		/// чтение параметра ("секция", "ключ", "значение по-умолчанию")
		/// </summary>
		/// <param name="section">имя секции</param>
		/// <param name="key">имя ключа</param>
		/// <param name="value">значение по-умолчанию, когда ключ не найден</param>
		/// <returns>Прочитанное значение. Если ключ не найден, то возвращается значение value из аргументов данного метода</returns>
		public string Read(string section, string key, string value)
		{
			var retVal = new StringBuilder(255);
			GetPrivateProfileString(section ?? _exeName, key, "", retVal, 255, _path);
			//return retVal.ToString();
			if (retVal.ToString() != "") {
				return retVal.ToString();
			}
			else {
				return value;
			}
		}


		//-------------------------------------------------------------------------------
		// Запись значения указного ключа и указанной секции в файл конфигурации.
		//
		// ini.Write("sectionName", "keyName", "valueName");
		//-------------------------------------------------------------------------------
		public void Write(string section, string key, string value)
		{
			WritePrivateProfileString(section, key, value, _path);
		}


		//-------------------------------------------------------------------------------
		// Удаление указанного ключа в файле конфигурации.
		//
		// ini.DeleteKey("sectionName", "keyName");
		//-------------------------------------------------------------------------------
		public void DeleteKey(string section, string key)
		{
			Write(key, null, section ?? _exeName);
		}


		//-------------------------------------------------------------------------------
		// Удаление указанной секции в файле конфигурации.
		//
		// ini.DeleteSection("sectionName");
		//-------------------------------------------------------------------------------
		public void DeleteSection(string section = null)
		{
			Write(null, null, section ?? _exeName);
		}


		//-------------------------------------------------------------------------------
		// Проверка наличия указанного ключа со значением в указанной секции файла
		// конфигурации.
		//
		// if (! ini.KeyExists("sectionName", "keyName")) => ключа со значением нет
		//-------------------------------------------------------------------------------
		public bool KeyExists(string section, string key, string value = null)
		{
			//return Read(key, section).Length > 0;
			return Read(section, key, value = "").Length > 0;
		}


		//-------------------------------------------------------------------------------
		// Чтение в массив всех ключей (только имена, без параметров) в указанной
		// секции файла конфигурации.
		//
		// string[] values = ini.ReadKeys("GUID");
		//-------------------------------------------------------------------------------
		public string[] ReadKeys(string section)
		{
			// первая строка не распознается, если ini-файл сохранен в UTF-8 с BOM
			while (true)
			{
				char[] chars = new char[capacity];
				int size = GetPrivateProfileString(section, null, "", chars, capacity, _path);

				if (size == 0)
				{
					return null;
				}

				if (size < capacity - 2)
				{
					string result = new String(chars, 0, size);
					string[] keys = result.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
					return keys;
				}
				capacity = capacity * 2;
			}
		}

	} //class IniFile
}
