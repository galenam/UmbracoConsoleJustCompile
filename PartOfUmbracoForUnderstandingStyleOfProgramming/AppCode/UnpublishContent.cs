using System;
using System.Collections.Generic;
using System.Linq;
using UmbracoCMS.AppCode.FakeClassLibrary;
using UmbracoCMS.AppCode.Interfaces;
using UmbracoCMS.AppCode.Manager;

namespace UmbracoCMS.AppCode
{
	/// <summary>
	/// Неопубликованная страница
	/// </summary>
	public class UnpublishContent:IUnifiedContent<IContent>
	{
        // i'm very tied. no initialization, it's hard to create a lot of fake objects
		private readonly IContentService contentService = null;

		public bool IsPropertyExist(string propertyName, IContent contentPage)
		{
			return contentPage != null && contentPage.HasProperty(propertyName);
		}

		public IEnumerable<IContent> GetChildren(IContent contentPage)
		{
			return contentPage == null ? null : contentPage.Children();
		}

		public string GetName(IContent contentPage)
		{
			return contentPage == null ? string.Empty : contentPage.Name;
		}

		public IContent GetParent(IContent contentPage)
		{
			return contentPage == null ? null : contentPage.Parent();
		}

		public int GetContentId(IContent contentPage)
		{
			return contentPage == null ? 0 : contentPage.Id;
		}

		public int GetLevel(IContent contentPage)
		{
			return contentPage == null ? 0 : contentPage.Level;
		}

		public int GetTemplateId(IContent contentPage)
		{
			return contentPage == null ? 0 : contentPage.Template.Id;
		}

		public IEnumerable<IContent> GetParents(IContent contentPage)
		{
			return contentPage == null ? null : contentPage.Ancestors();
		}

		// для GetPage . Тут порядок правильный
		public IEnumerable<IContent> GetParentsReversed(IContent contentPage)
		{
			return GetParents(contentPage);
		}
		

		public IContent GetById(int id)
		{
			return id <= 0 ? null : contentService.GetById(id);
		}

		public IContent GetByNameFirstLevelPage(string context)
		{
			return string.IsNullOrEmpty(context)
				? null
				: contentService.GetById(ExportManager.CatalogNodeId)
					.Children()
					.FirstOrDefault(c => c.Name.Equals(context, StringComparison.CurrentCultureIgnoreCase));
		}

		public IContent GetByName(string context, CmsPageSource source = CmsPageSource.Default)
		{
			if (string.IsNullOrEmpty(context)) return null;
			var id = PageFlowManager.GetRootNodeIdBySource(source.ToString().ToLower());
			// трудное решение. если при нуле искать везде - упадет производительность и, возможно, если переносить в умбраку тонны настроек мобильников, маркетинга и др.
			// имена разделов начнут совпадать. поэтому, для начала пусть при null валят переосмысливать источник.
			if (id <= 0) return null;
			var root = contentService.GetById(id);
			return root == null ? null : root.Descendants().FirstOrDefault(x => x.Name.Equals(context, StringComparison.CurrentCultureIgnoreCase));
		}

		public D GetPropertyValue<D>(IContent contentPage, string propertyName, bool isRecursive)
		{
			if (contentPage == null || !contentPage.HasProperty(propertyName)) return default(D);
			var value = contentPage.GetValue<D>(propertyName);
			if (!isRecursive || !EqualityComparer<D>.Default.Equals(value, default(D))) return value;

			var ancestor = contentPage.Ancestors().LastOrDefault(c => c.HasProperty(propertyName) && !string.IsNullOrEmpty(c.GetValue<string>(propertyName)));
			return ancestor == null ? default(D) : ancestor.GetValue<D>(propertyName);
		}

		/// <summary>
		/// Возвращает плоский список вложенных узлов
		/// </summary>
		/// <param name="contentPage">Родительская страница для анализа</param>
		/// <returns>плоский список вложенных узлов</returns>
		public IEnumerable<IContent> GetChildrenFlatten(IContent contentPage)
		{
			return contentPage == null ? null : contentPage.Descendants();
		}
	}
}