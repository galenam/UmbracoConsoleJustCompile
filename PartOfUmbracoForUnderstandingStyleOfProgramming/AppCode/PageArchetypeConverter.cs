using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UmbracoCMS.AppCode.FakeClassLibrary;
using UmbracoCMS.AppCode.Interfaces;

namespace UmbracoCMS.AppCode
{
	/// <summary>
	/// Преобразование страниц из archetype в формат PFS
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class PageArchetypeConverter<T> : BaseArchetypeConverter<T>
	{
		#region Properties
		private readonly int _allGoodsFacetId = 0;
		public int AllGoodsFacetId
		{
			get { return _allGoodsFacetId; }
		}

		#endregion

		#region HorizontalMenuCopyFromPFS
		private static readonly int HorizontalMenuItemsCount =6;

		private static readonly HashSet<string> HorizontalMenuBySecondLevel =
			GetHashSetFromConfig("HorizontalMenuBySecondLevel");

		private static readonly HashSet<string> HorizontalMenuByExtendedMenu =
			GetHashSetFromConfig("HorizontalMenuByExtendedMenu");
		#endregion

		#region FunctionsForPublic

		public PageArchetypeConverter(IUnifiedContent<T> content):base(content){}

		#endregion

		private static HashSet<string> GetHashSetFromConfig(string name)
		{
			var value = ConfigurationManager.AppSettings[name] ?? String.Empty;
			var values = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			return new HashSet<string>(values);
		}

		public ExtendedMenuInfo GetExtendedMenu(string context, string fieldName, ref ErrorInfo error)
		{
			T content = UnifiedContent.GetByNameFirstLevelPage(context);
			return GetObject(content, fieldName, ref error, ConvertFromArchetypeModelToExtendedMenuInfo);
		}
		

		/// <summary>
		/// Получение из archetype json объекта типа ExtendedMenuInfo
		/// </summary>
		/// <param name="propValue">Содержимое поля</param>
		/// <returns></returns>
		private ExtendedMenuInfo ConvertFromArchetypeModelToExtendedMenuInfo(string propValue)
		{
			if (string.IsNullOrEmpty(propValue)) return null;

			var extMenu =
				ArchetypeConverterManager.ConvertFromArchetypeModelToT<ExtendedMenuInfo>(propValue, ConvertFromArchetypeModelToExtendedMenuInfo);
			if (extMenu != null) {
				extMenu.Hash = extMenu.ToHash();
			}
			return extMenu;
		}

		/// <summary>
		/// Начитывание полей ExtendedMenuInfo объекта
		/// </summary>
		/// <param name="alias">имя свойства в archetype поле</param>
		/// <param name="extMenu">инициализируемый объект</param>
		/// <param name="propInnerValue">значение свойства в archetype поле</param>
		private void ConvertFromArchetypeModelToExtendedMenuInfo(string alias,
			ExtendedMenuInfo extMenu, string propInnerValue)
		{
			switch (alias) {
				case "blocksMenu":
					extMenu.Blocks = ArchetypeConverterManager.ConvertFromArchetypeModelToListT<ExtendedMenuBlockInfo>(propInnerValue,
						ConvertFromArchetypeModelToExtendedMenuBlockInfo);
					break;
				case "brands":
					extMenu.Brands = ArchetypeConverterManager.ConvertFromArchetypeModelToListT<ExtendedMenuBrandInfo>(propInnerValue,
						ConvertFromArchetypeModelToExtendedMenuBrandInfo);
					break;
				case "itemsBlockId":
					extMenu.ItemsBlockId = JsonConvert.DeserializeObject<int>(propInnerValue);
					break;
			}
		}

		/// <summary>
		/// Начитывание полей ExtendedMenuBlockInfo объекта
		/// </summary>
		/// <param name="alias">имя свойства в archetype поле</param>
		/// <param name="block">инициализируемый объект</param>
		/// <param name="value">значение свойства в archetype поле</param>
		private void ConvertFromArchetypeModelToExtendedMenuBlockInfo(string alias,
			ExtendedMenuBlockInfo block, string value)
		{
			block.Items = ArchetypeConverterManager.ConvertFromArchetypeModelToListT<ExtendedMenuSecondLevelItemInfo>(value,
				ConvertFromArchetypeModelToExtendedMenuSecondLevelItemInfo);
		}

		/// <summary>
		/// Начитывание полей ExtendedMenuItemInfoBase объекта
		/// </summary>
		/// <param name="alias">имя свойства в archetype поле</param>
		/// <param name="item">инициализируемый объект</param>
		/// <param name="value">значение свойства в archetype поле</param>
		private void ConvertFromArchetypeModelToExtendedMenuItemInfoBase(string alias,
			ExtendedMenuItemInfoBase item, string value)
		{
			switch (alias) {
				case "name":
					item.Name = value;
					break;
				case "href":
					item.Href = value;
					break;
			}
		}


		/// <summary>
		/// Начитывание полей ExtendedMenuSecondLevelItemInfo объекта
		/// </summary>
		/// <param name="alias">имя свойства в archetype поле</param>
		/// <param name="secondLevelItem">инициализируемый объект</param>
		/// <param name="value">значение свойства в archetype поле</param>
		private void ConvertFromArchetypeModelToExtendedMenuSecondLevelItemInfo(string alias,
			ExtendedMenuSecondLevelItemInfo secondLevelItem, string value)
		{
			ConvertFromArchetypeModelToExtendedMenuItemInfoBase(alias, secondLevelItem, value);
			switch (alias) {
				case "isNew":
					secondLevelItem.IsNew = ArchetypeConverterManager.ConvertToBool(value);
					break;
				case "items":
					secondLevelItem.Items = ArchetypeConverterManager.ConvertFromArchetypeModelToListT<ExtendedMenuThirdLevelItemInfo>(value,
						ConvertFromArchetypeModelToExtendedMenuThirdLevelItemInfo);
					break;
			}

		}

		/// <summary>
		/// Начитывание полей ExtendedMenuThirdLevelItemInfo объекта
		/// </summary>
		/// <param name="alias">имя свойства в archetype поле</param>
		/// <param name="thirdLevelItem">инициализируемый объект</param>
		/// <param name="value">значение свойства в archetype поле</param>
		private void ConvertFromArchetypeModelToExtendedMenuThirdLevelItemInfo(string alias,
			ExtendedMenuThirdLevelItemInfo thirdLevelItem, string value)
		{
			ConvertFromArchetypeModelToExtendedMenuItemInfoBase(alias, thirdLevelItem, value);
			if (alias == "inOneLine") {
				thirdLevelItem.InOneLine = ArchetypeConverterManager.ConvertToBool(value);
			}
		}

		/// <summary>
		/// Начитывание полей ExtendedMenuBrandInfo объекта
		/// </summary>
		/// <param name="alias">имя свойства в archetype поле</param>
		/// <param name="brand">инициализируемый объект</param>
		/// <param name="value">значение свойства в archetype поле</param>
		private void ConvertFromArchetypeModelToExtendedMenuBrandInfo(string alias, ExtendedMenuBrandInfo brand,
			string value)
		{
			switch (alias) {
				case "name":
					brand.Name = value;
					break;
				case "href":
					brand.Href = value;
					break;
				case "imageUrlPicker":
					brand.ImageUrl = brand.ImageUrlGray = value;
					break;
			}
		}





		public Tuple<WebSection, List<ErrorInfo>> GetWebSection(string context, bool showAll = false)
		{
			// если не задана страница, значит, надо отдать все, что есть, не жадничать
			T content = string.IsNullOrEmpty(context) ? UnifiedContent.GetById(ExportManager.CatalogNodeId) : UnifiedContent.GetByName(context);
			return GetWebSection(content, showAll: showAll);
		}

		private Tuple<WebSection, List<ErrorInfo>> GetWebSection(T content, WebSection wSectionParent = null, bool showAll = false, bool needChildren = true)
		{
			var errors = new List<ErrorInfo>();
			ErrorInfo error = null;
			var fieldName = ExportManager.WebSectionFieldAlias;
			var wSection = GetObject<WebSection>(content, fieldName, ref error, ConvertFromArchetypeModelToWebSection);
			if (error != null) {
				errors.Add(error);
			}
			if (wSection == null) {
				if (error == null) {
					errors.Add(new ErrorInfo { ErrorCode = "WebSection не заполнен на странице " + UnifiedContent.GetName(content) });
				}
				return new Tuple<WebSection, List<ErrorInfo>>(null, errors);
			}
			// чтобы SphinxBuilder не дулся, надо ему давать уникальный id. А чтобы было ясно: это старый или новый раздел (прям сразу, с первого взгляда) 
			// если раздел старый, то его id экспортирован из БО (ориентировочно < 3500) 
			// а если новый, то от широты души плюсую ему 1000000 (настройка) к родному умбрако-id
			wSection.ID = wSection.ID > 0 ? wSection.ID : PageFlowManager.IdAddition + UnifiedContent.GetContentId(content);


			var level = UnifiedContent.GetLevel(content);
			wSection.Level = level - 2 < 0 ? 0 : level - 2;
			var parent = default(T);
			if (level > 0)
			{
				if (wSectionParent == null)
				{
					parent = UnifiedContent.GetParent(content);
					wSectionParent = GetObject<WebSection>(parent, fieldName, ref error, ConvertFromArchetypeModelToWebSection);
				}
				if (wSectionParent != null)
				{
					wSection.HaveParent = UnifiedContent.GetLevel(parent) > 2;
					if (UnifiedContent.IsPropertyExist(ExportManager.NeedParentsFieldAlias, content) &&
					    GetPropertyValue<bool>(content, ExportManager.NeedParentsFieldAlias))
					{
						wSection.ParentID = wSectionParent.ID;
					} else if (UnifiedContent.GetContentId(content) != ExportManager.CatalogNodeId)
					{
						wSection.ParentID = 199;
					}
					if (wSection.FacetId <= 0 || wSection.FacetId == AllGoodsFacetId)
					{
						wSection.Parent = wSectionParent;
						if (wSectionParent.FacetId == 0 || wSectionParent.FacetId == AllGoodsFacetId)
						{
							var tuple = GetWebSection(parent, null, showAll, false);
							wSection.Parent.Parent = tuple == null ? null : tuple.Item1;
						}
					}
				}
			}
			if (wSection.FacetId == AllGoodsFacetId && wSection.Parent != null) {
				var wp = wSection.Parent;
				while (wp != null) {
					if (wp.FacetId != AllGoodsFacetId) {
						wSection.FacetId = wp.FacetId;
						break;
					}
					wp = wp.Parent;
				}
			}

			wSection.Name = UnifiedContent.GetName(content);
			// если функция для получения вложенных разделов не передается, то метод запускается только для начитывания свойств и ab-тесты тоже не нужны
			if (needChildren)
			{
				var children = UnifiedContent.GetChildren(content).ToList();
				if (children.Any())
				{
					var childTuple =
						children.Select(
							child =>
								GetWebSection(child, wSection, showAll))
							.ToList();
					// несла-несла адскую логику PFS, но так и не донесла. IsMenu интересен для root разделов 0 уровня, и для вложенных в том случае, если передается еще один
					// спец. признак, что оно ему надо. Для того, чтобы не усложнять логику сверх необходимости и не дублировать то, что и так проверяется в PFS, но при этом не
					// таскать огромные сообщения туда-сюда, нахожу неразумный компромисс: заблокированные передаю по требованию, с признаком IsMenu PFS пусть  сам разбирается
					wSection.Childs =
						new Collection<WebSection>(
							childTuple.Where(el => !el.Item1.Lock || showAll).Select(tuple => tuple.Item1).ToList());
					errors.AddRange(childTuple.SelectMany(tuple => tuple.Item2).Where(err => err != null));
				}
				// танцы с ab-тестами
				// заполнение информации по тесту на самом разделе по которому идут тесты
				string abTestName;
				var abGroupsContent = GetAbTestInfofromFromInnerFormat(content, out abTestName);
				wSection.Properties = wSection.Properties ?? new WebSectionProperties();
				if (abGroupsContent != null && abGroupsContent.Count > 0)
				{
					wSection.Properties.Add(new WebSectionProperty {Name = "ab_test", Value = abTestName});
					wSection.Properties.Add(new WebSectionProperty {Name = "ab_groups"});
					wSection.AbTests = new Collection<WebSection>(abGroupsContent.Select(ab => UnifiedContent.GetById(ab.NodeId)).
						Select(x => !EqualityComparer<T>.Default.Equals(x, default(T)) ? GetSimpleWebSection(x) : null)
						.Where(x => x != null)
						.ToList());
				}
				else if (level > 0)
				{
					// проверка: является ли раздел ab-тестом
					if (EqualityComparer<T>.Default.Equals(parent, default(T)))
					{
						parent = UnifiedContent.GetParent(content);
					}
					abGroupsContent = GetAbTestInfofromFromInnerFormat(parent, out abTestName);
					if (abGroupsContent != null && abGroupsContent.Count > 0)
					{
						var valueAbGroups = abGroupsContent.FirstOrDefault(abgr => abgr.NodeId == UnifiedContent.GetContentId(content));
						if (valueAbGroups != null)
						{
							wSection.Properties.Add(new WebSectionProperty {Name = "ab_test", Value = abTestName});
							wSection.Properties.Add(new WebSectionProperty {Name = "ab_groups", Value = valueAbGroups.AbGroupIds});
						}
					}
				}
			}
			return new Tuple<WebSection, List<ErrorInfo>>(wSection, errors);
		}


		private WebSection ConvertFromArchetypeModelToWebSection(string propValue)
		{
			return ArchetypeConverterManager.ConvertFromArchetypeModelToT<WebSection>(propValue, ConvertFromArchetypeModelToWebSectionInfo);
		}

		private void ConvertFromArchetypeModelToWebSectionInfo(string alias, WebSection section, string propValue)
		{
			switch (alias) {
				case "catalogId": {
						section.CatalogId = JsonConvert.DeserializeObject<int>(propValue);
						break;
					}
				case "displayName": {
						section.DisplayName = propValue;
						break;
					}
				case "id": {
						section.ID = JsonConvert.DeserializeObject<int>(propValue);
						break;
					}
				case "color": {
						section.Color = propValue;
						break;
					}
				case "comment": {
						section.Comment = propValue;
						break;
					}
				case "isMenu": {
					section.IsMenu = ArchetypeConverterManager.ConvertToBool(propValue);
						break;
					}
				case "isSlashMake": {
					section.IsSlashMake = ArchetypeConverterManager.ConvertToBool(propValue);
						break;
					}
				case "properties": {
					var tmp = ArchetypeConverterManager.ConvertFromArchetypeModelToListT<WebSectionProperty>(propValue, ConvertFromArchetypeModelToWebSectionPropertyInfo);
						section.Properties = new WebSectionProperties();
						foreach (var prop in tmp) {
							if (prop!=null && !string.IsNullOrEmpty(prop.Name)) {section.Properties.Add(prop);}
						}
						break;
					}
				case "lock": {
					section.Lock = ArchetypeConverterManager.ConvertToBool(propValue);
						break;
					}
				case "sortOrder": {
						section.SortOrder = JsonConvert.DeserializeObject<int>(propValue);
						break;
					}
				case "title": {
						section.Title = propValue;
						break;
					}
			}
		}

		private void ConvertFromArchetypeModelToWebSectionPropertyInfo(string alias, WebSectionProperty sectionProperty,
			string propValue)
		{
			switch (alias) {
				case "name": {
						// проблема в том, что эти имена свойств - dropdownlist, которые затейник IContent хранит в виде Id пунктов списка,
						// а простой парень INode - тупенько строчкой с названием. Т.к. сам метод передается параметром и единообразно чуть ли 
						// не с самого верха, мне лень (да и не кошерно ради 1 параметра все это делать) все править, то буду парсить, в надежде на лучшее
						var preValueId = 0;
						if (int.TryParse(propValue, out preValueId) && preValueId > 0) {
							sectionProperty.Name = preValueId.ToString();
						} else {
							sectionProperty.Name = propValue;
						}
						break;
					}
				case "value": {
						sectionProperty.Value = propValue;
						break;
					}
			}
		}

		private List<AbGroup> GetAbTestInfofromFromInnerFormat(T content, out string abTestName)
		{
			List<AbGroup> result = null;
			abTestName = string.Empty;
			if (EqualityComparer<T>.Default.Equals(content, default(T)) || !UnifiedContent.IsPropertyExist(ExportManager.AbTestFieldAlias, content)) { return null; }

			var abTestUmbraco = ConvertFromArchetypeModelToAbTestInnerUmbraco(GetPropertyValue<string>(content, (ExportManager.AbTestFieldAlias)));
			if (abTestUmbraco == null || abTestUmbraco.AbGroups == null || abTestUmbraco.AbGroups.Count == 0) { return null; }

			abTestName = abTestUmbraco.Name;
			result = abTestUmbraco.AbGroups;
			return result;
		}
		
		/// <summary>
		/// подать ab-тесты
		/// </summary>
		/// <param name="propValue">значение поля для разбора</param>
		/// <returns>информация по ab-тестам на странице</returns>
		public AbTestInnerUmbraco ConvertFromArchetypeModelToAbTestInnerUmbraco(string propValue)
		{
			return ArchetypeConverterManager.ConvertFromArchetypeModelToT<AbTestInnerUmbraco>(propValue, ConvertFromArchetypeModelToAbTestInnerUmbracoSwitch);
		}

		/// <summary>
		/// собираю ab-тесты на коленке
		/// </summary>
		/// <param name="alias">имя поля</param>
		/// <param name="abtest">куда класть</param>
		/// <param name="propValue">значение поля</param>
		private void ConvertFromArchetypeModelToAbTestInnerUmbracoSwitch(string alias, AbTestInnerUmbraco abtest, string propValue)
		{
			switch (alias) {
				case "name": {
						abtest.Name = propValue;
						break;
					}
				case "ids": {
					abtest.AbGroups = ArchetypeConverterManager.ConvertFromArchetypeModelToListT<AbGroup>(propValue, ConvertFromArchetypeModelToAbGroup).ToList();
						break;
					}
			}
		}

		/// <summary>
		/// Преобразование к внутриумбрачному формату ab-тестов (внутренний id страницы+список ab групп) для того, чтобы использовать при сохранении страниц 
		/// и автошмякании чекбокса "это ab-тест" на ab-тестном разделе. Для передачи в PFS NodeId преобразовать к названию страницы
		/// </summary>
		/// <param name="alias">Название поля</param>
		/// <param name="abGroup">Объект для инициализации</param>
		/// <param name="propValue">Значение поля</param>
		private static void ConvertFromArchetypeModelToAbGroup(string alias, AbGroup abGroup, string propValue)
		{
			switch (alias) {
				case "context": {
						abGroup.NodeId = JsonConvert.DeserializeObject<int>(propValue);
						break;
					}
				case "abGroupIds": {
						abGroup.AbGroupIds = propValue;
						break;
					}
			}
		}

		/// <summary>
		/// Lite-версия: получение WebSection без вложенных разделов и списка ошибок, только для начитывания фасетных параметров
		/// </summary>
		/// <param name="content">страница: где ищем?</param>
		/// <returns>начитанный WebSection</returns>
		public WebSection GetSimpleWebSection(T content)
		{
			var wSectionTuple = GetWebSection(content, needChildren:false);
			return wSectionTuple.Item2 != null && wSectionTuple.Item2.Count > 0 ? null : wSectionTuple.Item1;
		}

		public IEnumerable<MenuItemInfoBase> GetMenuItems(ref ErrorInfo bigError, int level = 0)
		{
			IEnumerable<T> contents = UnifiedContent.GetChildren(UnifiedContent.GetById(ExportManager.CatalogNodeId));
			return GetMenuItems(contents, ref bigError, level);
		}

		/// <summary>
		/// список элментов меню
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="contents">страницы</param>
		/// <param name="bigError">совокупность ошибок при начитывании всех элементов меню</param>
		/// <param name="level">Уровень меню</param>
		/// <returns></returns>
		private IEnumerable<MenuItemInfoBase> GetMenuItems(IEnumerable<T> contents, ref ErrorInfo bigError, int level=0)
		{
			var menuItems = new List<MenuItemInfoBase>();
			var errors = new List<ErrorInfo>();
			foreach (var content in contents) {
				MenuItemInfoBase menuItem = null;
				WebSection webSection = null;
				ExtendedMenuInfo extMenu = null;
				if (UnifiedContent.IsPropertyExist(ExportManager.WebSectionFieldAlias, content)) {
					var webSectionTuple = GetWebSection(content);
					webSection = webSectionTuple.Item1;
					if (webSectionTuple.Item2 != null) {
						errors.AddRange(webSectionTuple.Item2);
					}
				}
				if (webSection == null || !webSection.IsMenu) continue;

				if (level == 0)
				{
					if (webSection.Properties != null && webSection.Properties.GetInt("IsMarketing") == 1)
					{
						menuItem = new MenuMarketingSectionInfo
						           {
							           PromoText = webSection.Properties.GetString("MenuMarketingText"),
							           Color = webSection.Properties.GetString("MenuFontColor"),
							           MarkerSmall = webSection.Properties.GetString("MenuMarkerSmall"),
							           MarkerBig = webSection.Properties.GetString("MenuMarkerBig"),
							           MarkerIcon = webSection.Properties.GetString("MenuMarkerIcon")
						           };
					} else if (UnifiedContent.IsPropertyExist(PageFlowManager.ExtendedMenuFieldAlias, content))
					{
						menuItem = new MenuSectionInfo();
						ErrorInfo errorExtendedMenu = null;
						extMenu = GetObject(content, PageFlowManager.ExtendedMenuFieldAlias, ref errorExtendedMenu,
							ConvertFromArchetypeModelToExtendedMenuInfo);
						if (errorExtendedMenu != null)
						{
							errors.Add(errorExtendedMenu);
						}
						if (extMenu != null)
						{
							((MenuSectionInfo) menuItem).ExtendedMenu = extMenu;
						}
						var children = UnifiedContent.GetChildren(content).ToList();
						if (children.Any())
						{
							((MenuSectionInfo)menuItem).Sections = GetMenuItems(children, ref bigError, level + 1)
								.Cast<MenuItemInfo>().ToList();
						}
					}
				}

				menuItem = menuItem ?? new MenuItemInfo();
				menuItem.CssClass = webSection.Properties != null ? webSection.Properties.GetString("menu_class") : string.Empty;
				menuItem.DisplayName = webSection.DisplayName;
				menuItem.Id = webSection.ID;

				menuItem.Name = UnifiedContent.GetName(content);

				if (menuItem is MenuSectionInfo) {
					// надо горизонтальное меню 
					((MenuSectionInfo)menuItem).HorizontalMenu = GetHorizontalMenuInfo(webSection, extMenu);
				}
				menuItems.Add(menuItem);
			}
			if (errors.Count > 0) {
				var sb = new StringBuilder();
				foreach (var err in errors) {
					sb.Append(err.ErrorCode);
				}
				if (bigError == null) {
					bigError = new ErrorInfo { ErrorCode = sb.ToString() };
				} else {
					bigError.ErrorCode += sb.ToString();
				}
			}
			return menuItems;
		}

		private static HorizontalMenuInfo GetHorizontalMenuInfo(WebSection webSection, ExtendedMenuInfo extendedMenu)
		{
			IEnumerable<HorizontalMenuItemInfo> items = null;

			if (HorizontalMenuBySecondLevel.Contains(webSection.Name)) {
				if (webSection.Childs.Any()) {
					items =
						from section in webSection.Childs
						where !section.IsMarketing
						select new HorizontalMenuItemInfo {
							Name = section.DisplayName,
							Href = String.Format("/context/{0}/", section.Name),
						};
				}
			} else if (HorizontalMenuByExtendedMenu.Contains(webSection.Name)) {
				if (extendedMenu != null) {
					items =
						from item in extendedMenu.Blocks.SelectMany(x => x.Items)
						where !string.IsNullOrEmpty(item.Href) && item.Items.Any()
						select new HorizontalMenuItemInfo {
							Name = item.Name,
							Href = item.Href,
						};
				}

			}

			if (items == null) {
				return null;
			}

			if (HorizontalMenuItemsCount > 0) {
				items = items.Take(HorizontalMenuItemsCount);
			}

			return new HorizontalMenuInfo {
				Items = items.ToList(),
			};
		}

		public CmsPageInfo GetPage(string param, ref ErrorInfo error, CmsPageSource source)
		{
			T content = UnifiedContent.GetByName(param, source);
			var pageInfoFromOzon = new CmsPageInfo();

			if (EqualityComparer<T>.Default.Equals(content, default(T))) {
				error = new ErrorInfo{ErrorDescription = "В Umbraco не найдена страница с таким контекстом"};
				return null;
			}

			pageInfoFromOzon.Name = UnifiedContent.GetName(content);
			if (UnifiedContent.IsPropertyExist(ExportManager.WebSectionFieldAlias, content)) {
				var wSection = GetSimpleWebSection(content);
				if (wSection != null) {
					pageInfoFromOzon.DisplayName = wSection.DisplayName;
					pageInfoFromOzon.Id = wSection.ID;
					pageInfoFromOzon.Properties = wSection.Properties != null ? wSection.Properties.ToDictionary(prop => prop.Name, prop => prop.Value) : null;
				}
			}

			// шаблон
			var template ="fakeTemplate";
			if (string.IsNullOrEmpty(template)) {
				error = new ErrorInfo{ErrorDescription = "Umbraco: Шаблон страницы пустой"};
				return null;
			}
			// поля из шаблона: если в шаблоне что-то не упоминается, то оно не будет отображаться.
			var areasFromTepmalate = GetFieldsFromTemplate(template).ToList();
			if (!areasFromTepmalate.Any()) {
				error = new ErrorInfo{ErrorDescription = "Umbraco: в шаблоне не указаны поля для отображения"};
				return null;
			}

			// области из бэкофиса, для настройки соответствия, прописывания id, на которые закладывается OzonShop
			var areasCompliance = ExportManager.GetAreas().ToList();
			List<CmsAreaInfo> areasBackofice = null;
			var currentAreaCompliance = (from templateArea in areasFromTepmalate
										 join area in areasCompliance on templateArea.UmbracoDestination equals area.UmbracoDestination
										 select new AreaComplience {
											 BackofficePlaceId = area.BackofficePlaceId,
											 BackofficeSource = area.BackofficeSource,
											 BackofficeSourceID = area.BackofficeSourceID,
											 IsRecursive = templateArea.IsRecursive,
											 NeedForExport = area.NeedForExport,
											 UmbracoDestination = templateArea.UmbracoDestination
										 }).ToList();


			// последней должна идти область, 
			//куда выгружать после объединения областей при экспорте
			if (currentAreaCompliance.Any()) {
				areasBackofice = currentAreaCompliance.Select(a => new CmsAreaInfo {
					Id = a.BackofficeSourceID,
					Modules = GetModulesForSite(content, a.UmbracoDestination, a.IsRecursive),
					Name = a.BackofficeSource,
					PlaceId = a.BackofficePlaceId
				}).Where(ab => ab.Modules != null && ab.Modules.Count > 0).ToList();
			}

			pageInfoFromOzon.Areas = areasBackofice;
			// подделываю последовательность родителей под стандарты бэкофиса
			// данные из БД сразу приходят в нужном порядке, а вот из кэша - в обратном. т.к. это можно выяснить только на уровне конкретной реализации, приходится постоянно тасовать
			// колоду
			var parents = UnifiedContent.GetParentsReversed(content).ToList();
			if (parents.Count > 2) {
				// 2 верхних - это catalogs и main, наружу не отдавать. Reverse, т.к. надо идти с нижних этажей 
				var pageParent = parents.Skip(2).Select(GetPFSParent).ToList();
				pageInfoFromOzon.ParentsAdequate = pageParent;
				pageParent.Reverse();
				var parentsResult = new List<CmsPageParentInfo>();
				foreach (var p in pageParent)
				{
					parentsResult.Add(p.PageParent);
					if (!p.NeedParents)
					{
						break;
					}
				}
				// снова восстанавливаю правильную последовательность
				parentsResult.Reverse();
				if (UnifiedContent.IsPropertyExist(ExportManager.NeedParentsFieldAlias, content) && GetPropertyValue<bool>(content, ExportManager.NeedParentsFieldAlias))
				{
					pageInfoFromOzon.Parents = parentsResult;
				}
			}

			// ab-тесты есть? а если найду?
			UpdateAbTestInfo(content, pageInfoFromOzon, parents.LastOrDefault());
			return pageInfoFromOzon;
		}

		/// <summary>
		/// Получает список всех полей, упомянутых в шаблоне. Если поле есть в шаблоне, значит, оно будет отображаться. 
		/// Если признака recursive нет или =false, поле нерекурсивное
		/// </summary>
		/// <param name="templateContent"></param>
		/// <returns></returns>
		public static List<AreaComplience> GetFieldsFromTemplate(string templateContent)
		{
			List<AreaComplience> fieldsFromTemplate = null;
			if (string.IsNullOrEmpty(templateContent)) return null;

			//пример строки из шаблона <umbraco:Item field="ozonPageEditor" recursive="true" runat="server" />	
			var rg = new Regex("((<umbraco:Item field=\"(?<nameField>\\S+)\"[^/<]*?recursive=\"(?<recursive>\\S+)\".*?/>)|(<umbraco:Item field=\"(?<nameField>\\S+)\".*?/>))");
			// признака recursive="true" нет, т.е. поле false

			if (rg.IsMatch(templateContent)) {
				fieldsFromTemplate = new List<AreaComplience>();
				foreach (Match match in rg.Matches(templateContent)) {
					var nameField = match.Groups["nameField"].Success ? match.Groups["nameField"].Value : string.Empty;
					if (string.IsNullOrEmpty(nameField)) continue;
					var recursive = false;
					if (match.Groups["recursive"].Success) {
						bool.TryParse(match.Groups["recursive"].Value, out recursive);
					}
					fieldsFromTemplate.Add(new AreaComplience { UmbracoDestination = nameField, IsRecursive = recursive });
				}
			}
			return fieldsFromTemplate;
		}

		public List<CmsModuleInfo> GetModulesForSite(T content, string fieldName, bool isRecursive)
		{
			if (EqualityComparer<T>.Default.Equals(content, default(T))) return null;
			if (!UnifiedContent.IsPropertyExist(fieldName, content)) return null;
			var template = UnifiedContent.GetPropertyValue<string>(content, fieldName, isRecursive);
			if (string.IsNullOrEmpty(template)) return null;
			// Тут приходит шлак из смеси представления озоновских модулей во внутреннем формате умбраки и html. Приходится сначала расчленять на детальки, выделяя
			// умбрачные закидоны
			var rgMacro = new Regex("<\\?UMBRACO_MACRO.*?/>");

			// а потом уже вырезать параметры
			// умбрака добавляет все параметры, которые зарегистрированы у макроса, если значения не заданы, забивает "". 
			var rg = new Regex("(?<paramName>\\S+)=\"(?<paramValue>.*?)\"", RegexOptions.IgnoreCase | RegexOptions.Singleline);
			if (!rgMacro.IsMatch(template)) return null;
			var mService = new UmbracoMacroService();
			var modules = new List<CmsModuleInfo>();
			foreach (Match matchMacro in rgMacro.Matches(template)) {
				if (!rg.IsMatch(matchMacro.Value)) continue;
				CmsModuleInfo module = null;
				IMacro macro = null;
				foreach (Match match in rg.Matches(matchMacro.Value)) {
					if (string.IsNullOrEmpty(match.Groups["paramName"].Value))
					{
						continue;
					}
					if (match.Groups["paramName"].Value.ToLower().Equals("macroalias"))
						{
							macro = mService.GetByAlias(match.Groups["paramValue"].Value);
							if (macro == null) break;
							module = new CmsModuleInfo
							         {
								         Path = macro.ControlType.Replace("~/", string.Empty).Replace("\r", string.Empty),
								         Properties = new Dictionary<string, string>()
							         };
						}
						else
						{
							if (module == null) break;
						// вот этот умбрако-спам вида ИмяПараметра="" не надо никуда передавать, пусть хранит сама, раз уж у нее БД резиновая
							if (string.IsNullOrEmpty(match.Groups["paramValue"].Value))
							{
								continue;
							}
							var macroProperty = macro.Properties.FirstOrDefault(mProp => mProp.Alias == match.Groups["paramName"].Value);
							var value = macroProperty != null && macroProperty.EditorAlias == "Umbraco.TrueFalse"
								? match.Groups["paramValue"].Value == "1" || match.Groups["paramValue"].Value == "true" ? "True" : "False"
								: match.Groups["paramValue"].Value;
							module.Properties.Add(match.Groups["paramName"].Value, value);
						}
					
				}
				if (module != null) { modules.Add(module); }
			}
			return modules;
		}

		/// <summary>
		/// Выделить из родительского раздела информацию для передачи в PageFlowService
		/// </summary>
		/// <param name="content">страница</param>
		/// <returns>родительский раздел в формате PFS</returns>
		private PageInfoNeedParentsModel GetPFSParent(T content)
		{
			if (EqualityComparer<T>.Default.Equals(content , default(T))) return null;
			var result = new PageInfoNeedParentsModel { PageParent = new CmsPageParentInfo {Name =UnifiedContent.GetName(content)}};

			if (UnifiedContent.IsPropertyExist(ExportManager.WebSectionFieldAlias, content)) {
				var wSection = GetSimpleWebSection(content);
				if (wSection != null) {
					result.PageParent.Id = wSection.ID;
					result.PageParent.DisplayName = wSection.DisplayName;
				}
			}

			if (UnifiedContent.IsPropertyExist(ExportManager.NeedParentsFieldAlias, content))
			{
				result.NeedParents = GetPropertyValue<bool>(content, ExportManager.NeedParentsFieldAlias);
			}

			return result;
		}

		/// <summary>
		/// Обновить возращаемую сервисом страницу, с учетом существующих ab-тестов
		/// </summary>
		/// <param name="content">страница, на которой заведена информация об ab-тестах</param>
		/// <param name="pageInfoFromOzon">возращаемая сервисом информация</param>
		/// <param name="parent">Родительская страница</param>
		private void UpdateAbTestInfo(T content, CmsPageInfo pageInfoFromOzon, T parent)
		{
			string abTestName;
			var abTestGroups = GetAbTestInfofromFromInnerFormat(content, out abTestName);
			if (abTestGroups != null && abTestGroups.Count > 0) {
				pageInfoFromOzon.Properties.Add("ab_test", abTestName);
				string abgroups = string.Empty;
				// есть возможность для махинаций: если завести ab-тест, добавить на родительскую страницу, потом удалить страницу и снова завести с таким же именем,
				// но другим id, то метод упадет. Пусть лучше уж никаких ab-тестов, чем кривые-косые
				pageInfoFromOzon.AbTests = abTestGroups.Select(abu =>
				                                               {
																   var node = UnifiedContent.GetById(abu.NodeId);
					                                               return EqualityComparer<T>.Default.Equals(node, default(T))
						                                               ? null
						                                               : new CmsPageAbTestInfo
						                                                 {
							                                                 AbGroup = abu.AbGroupIds,
																			 ContextName = UnifiedContent.GetName(node)
						                                                 };
				                                               }).Where(abTest=> abTest!=null).ToList();
				pageInfoFromOzon.Properties.Add("ab_groups", abgroups);
			} else {
				abTestGroups = GetAbTestInfofromFromInnerFormat(parent, out abTestName);
				if (abTestGroups != null && abTestGroups.Count > 0) {
					var valueAbGroups = abTestGroups.FirstOrDefault(abgr => abgr.NodeId == UnifiedContent.GetContentId(content));
					if (valueAbGroups != null) {
						pageInfoFromOzon.Properties.Add("ab_test", abTestName);
						pageInfoFromOzon.Properties.Add("ab_groups", valueAbGroups.AbGroupIds);
					}
				}
			}
		}

		public Tuple<WebSection, List<ErrorInfo>> GetParentWebSection(string context)
		{
			var content = string.IsNullOrEmpty(context) ? UnifiedContent.GetById(ExportManager.CatalogNodeId) : UnifiedContent.GetByName(context);
			if (EqualityComparer<T>.Default.Equals(content, default(T))) return new Tuple<WebSection, List<ErrorInfo>>(null, new List<ErrorInfo>{new ErrorInfo{ErrorDescription = "Не найдена страница"}});
			var contentParent = UnifiedContent.GetParent(content);
			if (EqualityComparer<T>.Default.Equals(contentParent, default(T))) return new Tuple<WebSection, List<ErrorInfo>>(null, new List<ErrorInfo> { new ErrorInfo { ErrorDescription = "Не найден родительский раздел" } });
			var level = UnifiedContent.GetLevel(contentParent);
			return level > 2 ? GetWebSection(contentParent) : new Tuple<WebSection, List<ErrorInfo>>(null, new List<ErrorInfo> { new ErrorInfo { ErrorDescription = "Не найден websection для родительского раздела" } });
		}
	}
}