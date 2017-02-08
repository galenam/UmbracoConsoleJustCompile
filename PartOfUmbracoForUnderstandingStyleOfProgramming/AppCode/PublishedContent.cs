using System;
using System.Collections.Generic;
using System.Linq;
using UmbracoCMS.AppCode.FakeClassLibrary;
using UmbracoCMS.AppCode.Interfaces;

namespace UmbracoCMS.AppCode
{
	/// <summary>
	/// Опубликованная страница
	/// </summary>
	public class PublishedContent : IUnifiedContent<IPublishedContent>
	{
		private static readonly Lazy<UmbracoHelper> umbracoHelper =
			new Lazy<UmbracoHelper>(() => new UmbracoHelper(UmbracoContext.Current));

		public bool IsPropertyExist(string propertyName, IPublishedContent contentPage)
		{
			return contentPage != null && contentPage.HasProperty(propertyName);
		}

		public IEnumerable<IPublishedContent> GetChildren(IPublishedContent contentPage)
		{
			return contentPage == null ? null : contentPage.Children();
		}

		public string GetName(IPublishedContent contentPage)
		{
			return contentPage == null ? string.Empty : contentPage.Name;
		}

		public IPublishedContent GetParent(IPublishedContent contentPage)
		{
			return contentPage == null ? null : contentPage.Parent;
		}

		public int GetContentId(IPublishedContent contentPage)
		{
			return contentPage == null ? 0 : contentPage.Id;
		}

		public int GetLevel(IPublishedContent contentPage)
		{
			return contentPage == null ? 0 : contentPage.Level;
		}

		public int GetTemplateId(IPublishedContent contentPage)
		{
			return contentPage == null ? 0 : contentPage.TemplateId;
		}

		public IEnumerable<IPublishedContent> GetParents(IPublishedContent contentPage)
		{
			return contentPage == null ? null : contentPage.Ancestors();
		}

		// для GetPage . Тут порядок неправильный
		public IEnumerable<IPublishedContent> GetParentsReversed(IPublishedContent contentPage)
		{
			return GetParents(contentPage).Reverse();
		}

		public IPublishedContent GetById(int id)
		{
			return id <= 0 ? null : umbracoHelper.Value.TypedContent(id);
		}

		public IPublishedContent GetByNameFirstLevelPage(string context)
		{
			return string.IsNullOrEmpty(context)? null: GetById(ExportManager.CatalogNodeId).Children()
					.FirstOrDefault(pc => pc.Name.Equals(context, StringComparison.CurrentCultureIgnoreCase));
		}

		/// <summary>
		/// Библиотеку uQuery не следует использовать с Umbraco 7
		/// </summary>
		/// <param name="context">имя страницы</param>
		/// /// <param name="source">группа страниц</param>
		/// <returns>страница</returns>
		public IPublishedContent GetByName(string context, CmsPageSource source = CmsPageSource.Default)
		{
			List<IPublishedContent> nodes = null;
			var id = PageFlowManager.GetRootNodeIdBySource(source.ToString().ToLower());
			if (id == 0) return null;
			try
			{
				nodes = umbracoHelper.Value.TypedSearch(context).ToList();
			}
			catch (Exception)
			{
				return null;
			}
			return nodes.Any()? nodes.FirstOrDefault(content => content.Name.ToLower() == context && content.Ancestors().FirstOrDefault(ancestor => ancestor.Id == id)!=null): null;
		}

		public D GetPropertyValue<D>(IPublishedContent contentPage, string propertyName, bool isRecursive)
		{
			if (contentPage == null || !contentPage.HasProperty(propertyName)) return default(D);
			var prop = contentPage.GetProperty(propertyName, isRecursive).DataValue;
            // i'm very tied and want to sleep ((
			//return prop.TryConvertTo<D>().Result;
		    return default(D);
		}

		/// <summary>
		/// Возвращает плоский список вложенных узлов
		/// </summary>
		/// <param name="contentPage">Родительская страница для анализа</param>
		/// <returns>плоский список вложенных узлов</returns>
		public IEnumerable<IPublishedContent> GetChildrenFlatten(IPublishedContent contentPage)
		{
			return contentPage == null ? null : contentPage.Descendants();
		}
	}
}