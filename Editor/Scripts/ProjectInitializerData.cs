using System.Collections.Generic;
using UnityEngine;

namespace LeonDrace.ProjectInitializer
{
	public partial class ProjectInitializerData : ScriptableObject
	{
		public static readonly string AssetName = "Project Initializer Data";

		[SerializeField, HideInInspector]
		private bool m_Show = false;
		public bool Show
		{
			get => m_Show;
			set => m_Show = value;
		}

		[SerializeField]
		private Preset[] m_Presets;
		[SerializeField]
		public Preset[] Presets
		{
			get => m_Presets;
			set => m_Presets = value;
		}

		public void AddNewPreset()
		{
			var tempList = new List<Preset>(m_Presets)
			{
				new Preset() { Name = "New Preset" }
			};
			m_Presets = tempList.ToArray();
		}

		public void RemovePresetAt(int index)
		{
			var tempList = new List<Preset>(m_Presets);
			tempList.RemoveAt(index);
			m_Presets = tempList.ToArray();
		}


		[System.Serializable]
		public sealed class Preset
		{
			[SerializeField]
			private string m_Name;
			public string Name
			{
				get => m_Name;
				set => m_Name = value;
			}
			[SerializeField]
			private FolderStructure m_FolderStructure;
			public FolderStructure FolderStructure => m_FolderStructure;

			[SerializeField]
			private Package[] m_LocalPackages;
			public Package[] LocalPackages => m_LocalPackages;
		}

		[System.Serializable]
		public class Package
		{
			[SerializeField]
			private bool m_Active;
			public bool Active => m_Active;
			[SerializeField]
			private bool m_IsValid;
			public bool IsValid => m_IsValid;
			[SerializeField]
			private bool m_HasCustomPath;
			public bool HasCustomPath => m_HasCustomPath;
			[SerializeField]
			private string m_Path;
			public string Path => m_Path;
			[SerializeField]
			private string m_Tag;
			public string Tag => m_Tag;
		}
	}
}
