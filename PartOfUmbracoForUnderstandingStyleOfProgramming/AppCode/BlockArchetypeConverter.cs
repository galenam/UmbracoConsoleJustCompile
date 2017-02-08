using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using UmbracoCMS.AppCode.FakeClassLibrary;
using UmbracoCMS.AppCode.Interfaces;
using UmbracoCMS.AppCode.Manager;

namespace UmbracoCMS.AppCode.ArchetypeConverter
{
	/// <summary>
	/// Класс для преобразования Archetype-формата для блоков в блоки PFS
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class BlockArchetypeConverter<T> : BaseArchetypeConverter<T>
	{
		#region Fields
		readonly int _blockNodeId = PageFlowManager.GetRootNodeIdBySource("blocks");
		private readonly string _adFoxBlockFieldAlias = ConfigurationManager.AppSettings["AdFoxBlockFieldAlias"];
		private readonly string _goodsBlockFieldAlias = ConfigurationManager.AppSettings["GoodsBlockFieldAlias"];
		private readonly string _linkBlockFieldAlias = ConfigurationManager.AppSettings["LinkBlockFieldAlias"];

		private readonly Dictionary<string, string> _errorMessages = new Dictionary<string, string>
		                                                    {
			                                                    {"NoObject", "Umbraco: нет типа блоков"},
			                                                    {"NoField", "Umbraco: не найдено поле с таким именем "},
																{"NoChildren", "Umbraco: нет блоков"}
		                                                    };
		#endregion

		#region Propertyes
		protected override Dictionary<string, string> ErrorMessages
		{
			get { return _errorMessages; }

		}
		#endregion

		public BlockArchetypeConverter(IUnifiedContent<T> content) : base(content) { }

		/// <summary>
		/// Получить все блоки, лежащие ниже root-node типа "блоки"
		/// </summary>
		/// <param name="error">Ошибка для возврата наружу</param>
		/// <returns></returns>
		public List<Block> GetBlocks(ref ErrorInfo error)
		{
			var content = UnifiedContent.GetById(_blockNodeId);
			if (EqualityComparer<T>.Default.Equals(content, default(T))) {
				error = new ErrorInfo { ErrorCode = ErrorMessages["NoObject"] };
				return null;
			}
			var childs = UnifiedContent.GetChildrenFlatten(content).ToList();
			if (childs==null || !childs.Any()) {
				error = new ErrorInfo { ErrorCode = ErrorMessages["NoChildren"] };
				return null;
			}
			var blocks = new List<Block>();
			foreach (var child in childs)
			{
				bool isExists;
				Block block = GetBlock((Action<string, BlockAdFox, string>)ConvertFromArchetypeModelToBlockAdFox, child, _adFoxBlockFieldAlias, BlockType.AdFox, out isExists);

				if (!isExists) {
					block = GetBlock((Action<string, BlockGoods, string>)ConvertFromArchetypeModelToBlockGoods, child, _goodsBlockFieldAlias, BlockType.Goods, out isExists);
					if (block != null)
					{
						((BlockGoods) block).Title = UnifiedContent.GetName(child);
					}
				}

				if (!isExists) {
					block = GetBlock((Action<string, BlockLinks, string>)ConvertFromArchetypeModelToBlockLinks, child, _linkBlockFieldAlias, BlockType.Links, out isExists);
					if (block != null) {
						((BlockLinks)block).Title = UnifiedContent.GetName(child);
					}
				}

				if (block == null) continue;
				block.Id = UnifiedContent.GetContentId(child);
				blocks.Add(block);
			}
			return blocks;
		}

		/// <summary>
		/// Получить блок конкретного типа
		/// </summary>
		/// <typeparam name="TD">Класс блока</typeparam>
		/// <param name="func">Archetype-Парсер</param>
		/// <param name="child">страница с полем-описания блока</param>
		/// <param name="propertyName">свойство страницы, где хранится описание блока</param>
		/// <param name="bType">тип блока (для инициализации)</param>
		/// <param name="isExist">существует ли блок такого типа на странице? - для дальнейшей обработки</param>
		/// <returns></returns>
		private TD GetBlock<TD>(Action<string, TD, string> func, T child, string propertyName, BlockType bType, out bool isExist)
			where TD: Block, new()
		{
			isExist = false;
			if (!UnifiedContent.IsPropertyExist(propertyName, child))
			{
				return null;
			}
			isExist = true;
			var propValue = UnifiedContent.GetPropertyValue<string>(child, propertyName, false);
			var block = (string.IsNullOrEmpty(propValue))
				? null
				: ArchetypeConverterManager.ConvertFromArchetypeModelToT(propValue, func);
			if (block != null)
			{
				block.Type = bType;
			}
			return block;
		}

		/// <summary>
		/// Заполнение полей блока типа "BlockLinks"
		/// </summary>
		/// <param name="alias">название поля archetype</param>
		/// <param name="block">блок для заполнения</param>
		/// <param name="value">значение поля archetype</param>
		private void ConvertFromArchetypeModelToBlockLinks(string alias, BlockLinks block, string value)
		{
			switch (alias) {
				case "titleHref":
					block.TitleHref = value;
					break;
				case "titleIcon":
					block.TitleIcon = value;
					break;
				case "bgPic":
					block.BgPic = value;
					break;
				case "isTwoCols":
					block.IsTwoCols = ArchetypeConverterManager.ConvertToBool(value);
					break;
				case "isDarkTheme":
					block.IsDarkTheme = ArchetypeConverterManager.ConvertToBool(value);
					break;
				case "bgColor":
					block.BgColor = value;
					break;
				case "size":
					block.Size = ArchetypeConverterManager.ConvertFromArchetypeModelToStructT<BlockSize>(value, ArchetypeConverterManager.ParseFromArchetypeToSize);
					break;
				case "items":
					block.Items = ArchetypeConverterManager.ConvertFromArchetypeModelToListT<LinkItem>(value, ParseFromArchetypeToLinkItem);
					break;
			}
		}

		/// <summary>
		/// Заполнение полей ссылки "LinkItem"
		/// </summary>
		/// <param name="alias">название поля archetype</param>
		/// <param name="link">ссылка для заполнения</param>
		/// <param name="value">значение поля archetype</param>
		private void ParseFromArchetypeToLinkItem(string alias, LinkItem link, string value)
		{
			switch (alias)
			{
				case "href":
					link.Href = value;
					break;
				case "title":
					link.Title = value;
					break;
			}
		}

		/// <summary>
		/// Заполнение полей блока типа "BlockGoods"
		/// </summary>
		/// <param name="alias">название поля archetype</param>
		/// <param name="block">блок для заполнения</param>
		/// <param name="value">значение поля archetype</param>
		private void ConvertFromArchetypeModelToBlockGoods(string alias, BlockGoods block, string value)
		{
			switch (alias) {
				case "titleSmall":
					block.TitleSmall = value;
					break;
				case "classInstanceId":
					block.ClassInstanceId = value;
					break;
				case "size":
					block.Size = ArchetypeConverterManager.ConvertFromArchetypeModelToStructT<BlockSize>(value, ArchetypeConverterManager.ParseFromArchetypeToSize);
					break;
			}
		}

		/// <summary>
		/// Заполнение полей блока типа "BlockAdFox"
		/// </summary>
		/// <param name="alias">название поля archetype</param>
		/// <param name="block">блок для заполнения</param>
		/// <param name="value">значение поля archetype</param>
		private void ConvertFromArchetypeModelToBlockAdFox(string alias, BlockAdFox block, string value)
		{
			switch (alias) {
				case "zoneId":
					block.ZoneId = value;
					break;
				case "selection":
					block.Selection = value;
					break;
				case "name":
					block.Name = value;
					break;
				case "size":
					block.Size = ArchetypeConverterManager.ConvertFromArchetypeModelToStructT<BlockSize>(value, ArchetypeConverterManager.ParseFromArchetypeToSize);
					break;
			}
		}
	}
}