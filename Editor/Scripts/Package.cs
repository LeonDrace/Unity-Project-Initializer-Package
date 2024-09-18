using UnityEngine;

namespace LeonDrace.ProjectInitializer
{
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
		private string m_Tag;
		public string Tag => m_Tag;
	}

	[System.Serializable]
	public sealed class LocalPackage : Package
	{
		[SerializeField]
		private string m_Path;
		public string Path => m_Path;
		[SerializeField]
		private bool m_HasCustomPath;
		public bool HasCustomPath => m_HasCustomPath;
	}

	[System.Serializable]
	public sealed class UrlPackage : Package
	{
		[SerializeField]
		private string m_Url;
		public string Url => m_Url;
	}
}
