using System.Collections.Generic;
using UmbracoCMS.AppCode.FakeClassLibrary;

namespace UmbracoCMS.AppCode.Interfaces
{
	public interface IUnifiedContent<T>
	{
		bool IsPropertyExist(string propertyName, T contentPage);
		IEnumerable<T> GetChildren(T contentPage);
		IEnumerable<T> GetChildrenFlatten(T contentPage);
		string GetName(T contentPage);
		T GetParent(T contentPage);
		int GetContentId(T contentPage);
		int GetLevel(T contentPage);
		int GetTemplateId(T contentPage);
		IEnumerable<T> GetParents(T contentPage);
		IEnumerable<T> GetParentsReversed(T contentPage);
		T GetById(int id);
		T GetByNameFirstLevelPage(string context);
		T GetByName(string context, CmsPageSource source = CmsPageSource.Default);
		D GetPropertyValue<D>(T contentPage, string propertyName, bool isRecursive);
	}
}
