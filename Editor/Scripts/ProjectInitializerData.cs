using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectInitializerData : ScriptableObject
{
	[SerializeField]
	private FolderStructure m_folderStructure;
	public FolderStructure GetFolderStructure => m_folderStructure;


	[System.Serializable]
	public sealed class FolderStructure
	{
		[SerializeField]
		private string m_rootFolder;
		public string RootFolder => m_rootFolder;

		[SerializeField]
		private string[] m_createdFolders = new string[0];
		public string[] CreatedFolders
		{
			get { return m_createdFolders; }
			set { m_createdFolders = value; }
		}

		[SerializeField]
		private MovedAsset[] m_movedFiles = new MovedAsset[0];
		public MovedAsset[] MovedFiles
		{
			get { return m_movedFiles; }
			set { m_movedFiles = value; }
		}

		[SerializeField]
		private string[] m_deletedFolders = new string[0];
		public string[] DeletedFolders
		{
			get { return m_deletedFolders; }
			set { m_deletedFolders = value; }
		}

		public bool AddCreatedFolder(string folderName)
		{
			return AddFolder(ref m_createdFolders, folderName);
		}

		public bool AddDeletedFolder(string folderName)
		{
			return AddFolder(ref m_deletedFolders, folderName);
		}

		public bool AddMovedFolder(string currentFolderName, string targetFolderName)
		{
			var tempFolders = m_movedFiles.ToList();

			for (int i = 0; i < tempFolders.Count; i++)
			{
				if (tempFolders[i].CurrentPath == currentFolderName)
				{
					return false;
				}
			}

			tempFolders.Add(new MovedAsset() { CurrentPath = currentFolderName, TargetPath = targetFolderName });
			m_movedFiles = tempFolders.ToArray();
			return true;
		}

		public bool RemoveCreatedFolder(string folderName)
		{
			return RemoveFolder(ref m_createdFolders, folderName);
		}

		public bool RemovDeletedFolder(string folderName)
		{
			return RemoveFolder(ref m_deletedFolders, folderName);
		}

		public bool RemoveMovedFolder(string currentFolderName)
		{
			var tempFolders = m_movedFiles.ToList();

			bool removed = false;
			for (int i = tempFolders.Count - 1; i >= 0; i--)
			{
				if (tempFolders[i].CurrentPath == currentFolderName)
				{
					tempFolders.RemoveAt(i);
					removed = true;
				}
			}

			m_movedFiles = tempFolders.ToArray();
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
			private string m_currentPath;
			public string CurrentPath
			{
				get => m_currentPath;
				set => m_currentPath = value;
			}

			[SerializeField]
			private string m_targetPath;
			public string TargetPath
			{
				get => m_targetPath;
				set => m_targetPath = value;
			}
		}
	}
}
