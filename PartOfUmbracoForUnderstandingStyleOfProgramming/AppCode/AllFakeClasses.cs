using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UmbracoCMS.AppCode.FakeClassLibrary
{
    public class ErrorInfo
    {
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
    }

    public enum BlockType
    {
        AdFox, Goods, Links
    }

    public struct BlockSize
    {
    }

    public class Block
    {
        public string Title { get; set; }
        public int Id { get; set; }
        public string TitleHref { get; set; }
        public string TitleIcon { get; set; }
        public string BgPic { get; set; }
        public bool IsTwoCols { get; set; }
        public BlockType Type { get; set; }
        public BlockSize Size { get; set; }

    }

    public class BlockAdFox : Block
    {
        public string ZoneId { get; set; }
        public string Selection { get; set; }
        public string Name { get; set; }
    }

    public class BlockGoods : Block
    {
        public string TitleSmall { get; set; }
        public string ClassInstanceId { get; set; }
    }

    public class BlockLinks : Block
    {
        public bool IsDarkTheme { get; set; }
        public string BgColor { get; set; }
        public IEnumerable<LinkItem> Items { get; set; }
    }

    public static class ArchetypeConverterManager
    {
        public static T ConvertFromArchetypeModelToT<T>(string propValue, Action<string, T, string> func)
        {
            return default(T);
        }

        public static IEnumerable<T> ConvertFromArchetypeModelToListT<T>(string propValue, Action<string, T, string> func)
        {
            return Enumerable.Empty<T>();
        }
        public static T ConvertFromArchetypeModelToStructT<T>(string propValue, Action<string, T, string> func)
            where T:struct 
        {
            return default(T);
        }


        public static bool ConvertToBool(string value)
        {
            return false;
        }

        public static void ParseFromArchetypeToSize(string alias, BlockSize bs, string value)
        {
        }
    }

    public interface IPublishedContent
    {
        bool HasProperty(string name);
        IEnumerable<IPublishedContent> Children();
        string Name { get; }
        IPublishedContent Parent { get; }
        int Id { get; }
        int Level { get; }
        int TemplateId { get; }
        IEnumerable<IPublishedContent> Ancestors();
        IEnumerable<IPublishedContent> Descendants();
        IPublishedProperty GetProperty(string propName, bool isRecursive);
    }

    public interface IPublishedProperty
    {
        object DataValue { get; set; }
    }

    public interface DataValue
    {
        T TryConvertTo<T>(object o);
    }

    public interface IContent
    {
        bool HasPublishedVersion();
        bool HasProperty(string name);
        IEnumerable<IContent> Children();
        string Name { get; }
        IContent Parent();
        int Id { get; }
        int Level { get; }
        int TemplateId { get; }
        IEnumerable<IContent> Ancestors();
        IEnumerable<IContent> Descendants();
        IPublishedProperty GetProperty(string propName, bool isRecursive);
        ITemplate Template{get;}
        T GetValue<T>(string propName);
    }

    public interface ITemplate
    {
        int Id { get; }
    }

    public interface IContentService
    {
        IContent GetById(int id);
    }

    public enum StatusEnum
    {
        Error,
        Success
    }

    public class BaseResponse
    {
        public ErrorInfo Error { get; set; }
        public StatusEnum Status { get; set; }

        public void InitError(string code, string error)
        {
        }
    }

    public class BlocksResponse:BaseResponse
    {
        public IEnumerable<Block> Blocks { get; set; }
    }

    public static class LogHelper
    {
        public static void Info(Type t, string s)
        {
        }
    }

    public enum CmsPageSource
    {
        Default
    }

    public class ExtendedMenuInfo
    {
        public string Hash { get; set; }

        public string ToHash()
        {
            return null;
        }

        public IEnumerable<ExtendedMenuBlockInfo> Blocks { get; set; }
        public IEnumerable<ExtendedMenuBrandInfo> Brands { get; set; }
        public int ItemsBlockId { get; set; }
    }

    public class ExtendedMenuItemInfoBase
    {
        public string Name { get; set; }
        public string Href { get; set; }
    }

    public class ExtendedMenuBlockInfo
    {
        public IEnumerable<ExtendedMenuSecondLevelItemInfo> Items { get; set; }
    }

    public class ExtendedMenuBrandInfo
    {
        public string Name { get; set; }
        public string Href { get; set; }
        public string ImageUrl { get; set; }
        public string ImageUrlGray { get; set; }
    }

    public class ExtendedMenuSecondLevelItemInfo: ExtendedMenuItemInfoBase
    {
        public bool IsNew { get; set; }
        public IEnumerable<ExtendedMenuThirdLevelItemInfo> Items { get; set; }
    }

    public class ExtendedMenuThirdLevelItemInfo: ExtendedMenuItemInfoBase
    {
        public bool InOneLine { get; set; }
    }
    public class ExtendedMenuResponse:BaseResponse
    {
        public ExtendedMenuInfo ExtendedMenu { get; set; }
    }

    public class CmsPageResponse : BaseResponse
    {
        public CmsPageInfo Page { get; set; }
        public bool IsPageExists { get; set; }
    }

    public class CmsPageInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public int Id { get; set; }
        public Dictionary<string,string> Properties { get; set; }
        public List<CmsAreaInfo> Areas { get; set; }
        public List<CmsPageParentInfo> Parents { get; set; }
        public List<PageInfoNeedParentsModel> ParentsAdequate { get; set; }
        public List<CmsPageAbTestInfo> AbTests { get; set; }
    }

    public class CmsPageParentInfo: CmsPageInfo
    {
        
    }

    public class MenuResponse : BaseResponse
    {
        public MenuInfo Menu { get; set; }
    }

    public class MenuInfo
    {
        public List<MenuMarketingSectionInfo> MarketingSections { get; set; }
        public List<MenuSectionInfo> Sections { get; set; }
    }

    public class MenuItemInfoBase
    {
        public string CssClass { get; set; }
        public string DisplayName { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class WebSection
    {
        public int ID { get; set; }
        public int SortOrder { get; set; }
        public int Level { get; set; }
        public bool HaveParent { get; set; }
        public int ParentID { get; set; }
        public int FacetId { get; set; }
        public WebSection Parent { get; set; }
        public string Name { get; set; }
        public IEnumerable<WebSection> Childs { get; set; }
        public bool Lock { get; set; }
        public WebSectionProperties Properties { get; set; }
        public Collection<WebSection> AbTests { get; set; }
        public int CatalogId { get; set; }
        public string DisplayName { get; set; }
        public string Color { get; set; }
        public string Comment { get; set; }
        public string Title { get; set; }
        public bool IsMenu { get; set; }
        public bool IsSlashMake { get; set; }
        public bool IsMarketing { get; set; }

    }

    public class MenuMarketingSectionInfo : MenuItemInfoBase
    {
        public string PromoText { get; set; }
        public string MarkerSmall { get; set; }
        public string MarkerBig { get; set; }
        public string MarkerIcon { get; set; }
        public string Color { get; set; }
    }

    public class MenuSectionInfo : MenuItemInfoBase
    {
        public ExtendedMenuInfo ExtendedMenu { get; set; }
        public List<MenuItemInfo> Sections{get; set; }
        public HorizontalMenuInfo HorizontalMenu { get; set; }
        
    }

    public class HorizontalMenuInfo
    {
        public IEnumerable<HorizontalMenuItemInfo> Items { get; set; }
    }

    public class HorizontalMenuItemInfo
    {
        public string Name { get; set; }
        public string Href { get; set; }
        
        
    }

    public class MenuItemInfo : MenuItemInfoBase
    {
    }

    public class WebSectionProperties : List<WebSectionProperty>
    {
        public int GetInt(string propName) => 0;

        public string GetString(string propName) => string.Empty;
    }

    public class WebSectionProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public static class ExportManager
    {
        public static int CatalogNodeId { get; set; }
        public static string WebSectionFieldAlias { get; set; }
        public static string NeedParentsFieldAlias { get; set; }
        public static string AbTestFieldAlias { get; set; }

        public static IEnumerable<AreaComplience> GetAreas() => Enumerable.Empty<AreaComplience>();
    }

    public class AbGroup
    {
        public int NodeId { get; set; }
        public string AbGroupIds { get; set; }
    }

    public class AbTestInnerUmbraco
    {
        public string Name { get; set; }
        public List<AbGroup> AbGroups { get; set; }
    }

    public class CmsAreaInfo
    {
        public int Id { get; set; }
        public List<CmsModuleInfo> Modules { get; set; }
        public string Name { get; set; }
        public int PlaceId { get; set; }

    }

    public class AreaComplience
    {
        public int BackofficePlaceId { get; set; }
        public string BackofficeSource { get; set; }
        public string UmbracoDestination { get; set; }
        public bool IsRecursive { get; set; }
        public int BackofficeSourceID { get; set; }
        public int NeedForExport { get; set; }
    }

    public class CmsModuleInfo
    {
        public string Path { get; set; }
        public Dictionary<string, string>Properties { get; set; }
        
    }

    public class UmbracoMacroService
    {
        public IMacro GetByAlias(string alias) => null;
    }

    public interface IMacro
    {
        string ControlType { get; }
        IEnumerable<IUmbracoProperty> Properties { get; set; }
    }

    public interface IUmbracoProperty
    {
        string Alias { get; }
        string EditorAlias { get; }

    }

    public class PageInfoNeedParentsModel
    {
        public CmsPageParentInfo PageParent { get; set; }
        public bool NeedParents { get; set; }
    }

    public class CmsPageAbTestInfo
    {
        public string AbGroup { get; set; }
        public string ContextName { get;set; }
    }

    public class ContextInfo: BaseResponse
    {
        public bool IsPageExists { get; set; }
        public WebSection WebSection { get; set; }
    }

    public class RootIdsConfigurationSection
    {
        public IEnumerable<IdUmbraco> Ids { get; set; }
    }

    public class IdUmbraco
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class UmbracoHelper
    {
        public UmbracoHelper(UmbracoContext uc)
        {
        }

        public IPublishedContent TypedContent(int id) => null;
        public IEnumerable<IPublishedContent> TypedSearch(string context) => null;
    }

    public class UmbracoContext
    {
        public static UmbracoContext Current { get; set; }
    }

    public class LinkItem
    {
        public string Href { get; set; }
        public string Title { get; set; }
    }
}
