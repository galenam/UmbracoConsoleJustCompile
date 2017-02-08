using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using UmbracoCMS.AppCode.FakeClassLibrary;

namespace UmbracoCMS.AppCode
{
	public static class PageFlowManager
	{
		#region Fields
		private static Lazy<PageArchetypeConverter<IPublishedContent>> _publishedPageArchetypeConverter = new Lazy<PageArchetypeConverter<IPublishedContent>>(() => new PageArchetypeConverter<IPublishedContent>(new PublishedContent()));
		private static Lazy<PageArchetypeConverter<IContent>> _unpublishedPageArchetypeConverter = new Lazy<PageArchetypeConverter<IContent>>(() => new PageArchetypeConverter<IContent>(new UnpublishContent()));
		#endregion

		#region properties
		public static string ExtendedMenuFieldAlias
		{
			get { return ConfigurationManager.AppSettings["OzonExtendedMenuFieldAlias"]; }
		}

		private static PageArchetypeConverter<IPublishedContent> PublishedPageArchetypeConverter
		{
			get { return _publishedPageArchetypeConverter.Value; }
		}

		private static PageArchetypeConverter<IContent> UnpublishedPageArchetypeConverter
		{
			get { return _unpublishedPageArchetypeConverter.Value; }
		}

	    public static int IdAddition { get; } = 6;
		#endregion

		/// <summary>
		/// Менюшечка приезжает в форматике умбракочки, надо ее в озоновскую конвертировать.
		/// </summary>
		/// <param name="context">Имя контекста, должно быть уникальным, если не уникальное - берет контекст с минимальным id и таким именем</param>
		/// <param name="ispreview">режим предпросмотра</param>
		/// <returns>Расширенное меню в формате озона</returns>
		public static ExtendedMenuResponse GetExtendedMenuByContext(string context, bool ispreview)
		{
			ErrorInfo error = null;
			var fieldName = ExtendedMenuFieldAlias;
			if (string.IsNullOrEmpty(context)) {
				error = new ErrorInfo { ErrorCode = "Umbraco: не задан контекст" };
				return new ExtendedMenuResponse {Error = error, ExtendedMenu = null, Status = StatusEnum.Error};
			}
			
			var uMenu =ispreview? UnpublishedPageArchetypeConverter.GetExtendedMenu(context, fieldName, ref error):
				PublishedPageArchetypeConverter.GetExtendedMenu(context, fieldName, ref error);

			return uMenu != null
				? new ExtendedMenuResponse {Error = null, Status = StatusEnum.Success, ExtendedMenu = uMenu}
				: error != null
					? new ExtendedMenuResponse {Error = error, Status = StatusEnum.Error, ExtendedMenu = null}
					: new ExtendedMenuResponse
					  {
						  Error = new ErrorInfo {ErrorCode = "Umbraco: не удалось десериализовать json"},
						  Status = StatusEnum.Error,
						  ExtendedMenu = null
					  };
		}

		///// <summary>
		///// Получить ссылку на картинку по внутреннему id картинки
		///// </summary>
		///// <param name="imageId">внутренний Id картинки</param>
		///// <returns>url картинки</returns>
		//private static string GetImageUrl(int imageId)
		//{
		//	var imageUrl = string.Empty;
		//	if (imageId > 0) {
		//		// можно через uquery получить media
		//		var media = uQuery.GetMedia(imageId);
		//		imageUrl = media == null ? string.Empty : media.GetImageUrl();
		//	}
		//	return imageUrl;
		//}

		/// <summary>
		/// Возвращает страницу
		/// </summary>
		/// <param name="param">Имя контекста. Должно быть уникальным. Крутитесь, как хотите.</param>
		/// <param name="ispreview">Признак: true- опубликованные разделы; false - иное</param>
		/// <param name="source">Источник: под каким рутом искать страницы</param>
		/// <returns></returns>
		public static CmsPageResponse GetPageInfo(string param, bool ispreview, CmsPageSource source)
		{
			ErrorInfo error=null;
			var pageInfoFromOzon = ispreview ? UnpublishedPageArchetypeConverter.GetPage(param, ref error, source) :
				PublishedPageArchetypeConverter.GetPage(param, ref error, source);

			var isExist = IsPageRealExists(ispreview, pageInfoFromOzon == null, param, source);

			var result = pageInfoFromOzon != null
				? new CmsPageResponse {Error = null, Status = StatusEnum.Success, Page = pageInfoFromOzon, IsPageExists = true}
				: error == null
					? new CmsPageResponse
					  {
						  Error = new ErrorInfo {ErrorDescription = "UmbracoCMS: в процессе выполнения произошла непредвиденная ошибка"},
						  Status = StatusEnum.Error,
						  Page = null,
						  IsPageExists = isExist
					  }
					: new CmsPageResponse {Error = error, Status = StatusEnum.Error, Page = null, IsPageExists = isExist};
			if (result.Error != null)
			{
				LogHelper.Info(typeof(PageFlowManager), (string.Format("GetPageInfo contextName={2}, ispreview={3}, source={4}, ErrorDescription={0}, ErrorCode={1}", 
					result.Error.ErrorDescription, result.Error.ErrorCode, param, ispreview, source)));
			}

			return result;
		}

		/// <summary>
		/// если в боевом режиме страница не начитана из кэша, то умбрака лезет в бд и там шебуршится, пытаясь выяснить: у нее опять побило кэш или страницы, и вправду, нет.
		/// </summary>
		/// <param name="ispreview">режим "предпросмотр</param>
		/// <param name="objectIsEmpty">если объект из кэша построился, проверять ничего не надо</param>
		/// <param name="param">имя контекста</param>
		/// <param name="source">Источник: под каким рутом искать страницы </param>
		/// <returns></returns>
		private static bool IsPageRealExists(bool ispreview, bool objectIsEmpty, string param, CmsPageSource source=CmsPageSource.Default)
		{
			if (ispreview) return !objectIsEmpty;
			if (!objectIsEmpty) return true;

			var content = UnpublishedPageArchetypeConverter.UnifiedContent.GetByName(param, source);
			return content != null && content.HasPublishedVersion();
		}

		/// <summary>
		/// получить все меню сайта
		/// </summary>
		/// <param name="isPreview">режим предпросмотра</param>
		/// <returns>меню сайта</returns>
		public static MenuResponse GetMenu(bool isPreview)
		{
			var menu = new MenuResponse { Menu = new MenuInfo() };
			ErrorInfo error = null;

			var menuItems = isPreview
				? UnpublishedPageArchetypeConverter.GetMenuItems(ref error): PublishedPageArchetypeConverter.GetMenuItems(ref error);

			if (error != null)
			{
				menu.InitError(error.ErrorCode, "Menu error");

				LogHelper.Info(typeof(PageFlowManager), (string.Format("GetMenu ispreview={2}, ErrorDescription={0}, ErrorCode={1}",
						menu.Error.ErrorDescription, menu.Error.ErrorCode, isPreview)));
			} else {
				menu.Status = StatusEnum.Success;
			}

			menu.Menu.MarketingSections = new List<MenuMarketingSectionInfo>();
			menu.Menu.Sections = new List<MenuSectionInfo>();
			foreach (var mItem in menuItems) {
				if (mItem is MenuMarketingSectionInfo) {
					menu.Menu.MarketingSections.Add((MenuMarketingSectionInfo)mItem);
				}
				if (mItem is MenuSectionInfo) {
					menu.Menu.Sections.Add((MenuSectionInfo)mItem);
				}
			}

			return menu;
		}

		public static ContextInfo GetContextInfo(string contextName, bool ispreview, bool showAll = false)
		{
			var cInfo = ProccessWebSection(ispreview
				? UnpublishedPageArchetypeConverter.GetWebSection(contextName, showAll)
				: PublishedPageArchetypeConverter.GetWebSection(contextName, showAll), contextName, ispreview);
			if (cInfo != null) cInfo.IsPageExists = IsPageRealExists(ispreview, cInfo.WebSection == null, contextName);

			return cInfo;
		}

		public static ContextInfo GetParent(string context, bool ispreview)
		{
			return ProccessWebSection(ispreview ? UnpublishedPageArchetypeConverter.GetParentWebSection(context)
				: PublishedPageArchetypeConverter.GetParentWebSection(context), context, ispreview, "GetParent");
		}

		private static ContextInfo ProccessWebSection(Tuple<WebSection, List<ErrorInfo>> tuple, string contextName, bool isPreview = false,
			string methodName = "GetContextInfo")
		{
			var contextInfo = new ContextInfo();
			if (tuple.Item2 != null && tuple.Item2.Any()) {
				var sb = new StringBuilder();
				foreach (var err in tuple.Item2) {
					sb.Append(err.ErrorCode);
				}
				contextInfo.InitError(sb.ToString(), string.Format("WebSection context={0} is null", contextName));
				LogHelper.Info(typeof(PageFlowManager), (string.Format("{4} contextName={2}, ispreview={3}, ErrorDescription={0}, ErrorCode={1}",
						contextInfo.Error.ErrorDescription, contextInfo.Error.ErrorCode, contextName, isPreview, methodName)));
			} else {
				if (tuple.Item1 == null) {
					contextInfo.InitError("Произошла непредвиденная ошибка" , string.Format("WebSection context={0} is null", contextName));
					LogHelper.Info(typeof(PageFlowManager), (string.Format("GetContextInfo contextName={2}, ispreview={3}, ErrorDescription={0}, ErrorCode={1}",
						contextInfo.Error.ErrorDescription, contextInfo.Error.ErrorCode, contextName, isPreview)));
				} else {
					contextInfo.Status = StatusEnum.Success;
				}
			}
			contextInfo.WebSection = tuple.Item1;
			return contextInfo;
		}

		/// <summary>
		/// Для того, чтобы не убить поиск при добавлении новых сущностей в умбраку,
		/// надо делать поиск с учетом главного раздела 
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static int GetRootNodeIdBySource(string source)
		{
			var ids = (RootIdsConfigurationSection)ConfigurationManager.GetSection("RootIds");
			var root = ids != null && ids.Ids != null ? ids.Ids.FirstOrDefault(r => r.Name == source.ToLower()) : null;
			return root == null ? 0 : root.Value;
		}
	}
}
