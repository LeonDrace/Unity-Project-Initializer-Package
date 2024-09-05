using System.Linq;
using UnityEngine;

namespace LeonDrace.ProjectInitializer
{
	public class ProjectInitializerData : ScriptableObject
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
		private FolderStructure m_FolderStructure;
		public FolderStructure GetFolderStructure => m_FolderStructure;


		[System.Serializable]
		public sealed class FolderStructure
		{
			[SerializeField]
			private string m_RootFolder;
			public string RootFolder => m_RootFolder;

			[SerializeField]
			private string[] m_CreatedFolders = new string[0];
			public string[] CreatedFolders
			{
				get { return m_CreatedFolders; }
				set { m_CreatedFolders = value; }
			}

			[SerializeField]
			private MovedAsset[] m_MovedFiles = new MovedAsset[0];
			public MovedAsset[] MovedFiles
			{
				get { return m_MovedFiles; }
				set { m_MovedFiles = value; }
			}

			[SerializeField]
			private string[] m_DeletedFolders = new string[0];
			public string[] DeletedFolders
			{
				get { return m_DeletedFolders; }
				set { m_DeletedFolders = value; }
			}

			public bool AddCreatedFolder(string folderName)
			{
				return AddFolder(ref m_CreatedFolders, folderName);
			}

			public bool AddDeletedFolder(string folderName)
			{
				return AddFolder(ref m_DeletedFolders, folderName);
			}

			public bool AddMovedFolder(string currentFolderName, string targetFolderName)
			{
				var tempFolders = m_MovedFiles.ToList();

				for (int i = 0; i < tempFolders.Count; i++)
				{
					if (tempFolders[i].CurrentPath == currentFolderName)
					{
						return false;
					}
				}

				tempFolders.Add(new MovedAsset() { CurrentPath = currentFolderName, TargetPath = targetFolderName });
				m_MovedFiles = tempFolders.ToArray();
				return true;
			}

			public bool RemoveCreatedFolder(string folderName)
			{
				return RemoveFolder(ref m_CreatedFolders, folderName);
			}

			public bool RemovDeletedFolder(string folderName)
			{
				return RemoveFolder(ref m_DeletedFolders, folderName);
			}

			public bool RemoveMovedFolder(string currentFolderName)
			{
				var tempFolders = m_MovedFiles.ToList();

				bool removed = false;
				for (int i = tempFolders.Count - 1; i >= 0; i--)
				{
					if (tempFolders[i].CurrentPath == currentFolderName)
					{
						tempFolders.RemoveAt(i);
						removed = true;
					}
				}

				m_MovedFiles = tempFolders.ToArray();
				return removed;
			}

			private bool AddFolder(ref string[] folders, string folderName)
			{
				var tempFolders = folders.ToList();

				if (tempFolders.Contains(folderName))
				{
					return false;
				}

				tempFolders.Add(folderName);
				folders = tempFolders.ToArray();
				return true;
			}

			private bool RemoveFolder(ref string[] folders, string folderName)
			{
				var tempFolders = folders.ToList();

				if (tempFolders.Contains(folderName))
				{
					return false;
				}

				tempFolders.Remove(folderName);
				folders = tempFolders.ToArray();
				return true;
			}

			[System.Serializable]
			public sealed class MovedAsset
			{
				[SerializeField]
				private string m_CurrentPath;
				public string CurrentPath
				{
					get => m_CurrentPath;
					set => m_CurrentPath = value;
				}

				[SerializeField]
				private string m_TargetPath;
				public string TargetPath
				{
					get => m_TargetPath;
					set => m_TargetPath = value;
				}
			}
		}
	}
}
