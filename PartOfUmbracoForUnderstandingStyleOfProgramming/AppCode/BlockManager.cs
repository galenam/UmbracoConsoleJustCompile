using System;
using System.Linq;
using UmbracoCMS.AppCode.ArchetypeConverter;
using UmbracoCMS.AppCode.FakeClassLibrary;

namespace UmbracoCMS.AppCode.Manager
{
	/// <summary>
	/// Класс для возврата блоков PFS
	/// </summary>
	public static class BlockManager
	{
		#region Fields
		private static Lazy<BlockArchetypeConverter<IPublishedContent>> _publishedBlockArchetypeConverter = new Lazy<BlockArchetypeConverter<IPublishedContent>>(() => new BlockArchetypeConverter<IPublishedContent>(new PublishedContent()));
		private static Lazy<BlockArchetypeConverter<IContent>> _unpublishedBlockArchetypeConverter = new Lazy<BlockArchetypeConverter<IContent>>(() => new BlockArchetypeConverter<IContent>(new UnpublishContent()));
		#endregion

		private static BlockArchetypeConverter<IPublishedContent> PublishedBlockArchetypeConverter
		{
			get { return _publishedBlockArchetypeConverter.Value; }
		}

		private static BlockArchetypeConverter<IContent> UnpublishedBlockArchetypeConverter
		{
			get { return _unpublishedBlockArchetypeConverter.Value; }
		}

		/// <summary>
		/// Начитывание блоков, зарегистрированных в умбраке.
		/// </summary>
		/// <param name="isPreview">неопубликован/опубликован</param>
		/// <returns>Блоки</returns>
		internal static BlocksResponse GetBlocks(bool isPreview)
		{
			var bResponse = new BlocksResponse();
			ErrorInfo error = null;

			var blocks = isPreview
				? UnpublishedBlockArchetypeConverter.GetBlocks(ref error) : PublishedBlockArchetypeConverter.GetBlocks(ref error);

			if (blocks== null || !blocks.Any()) {
				error = new ErrorInfo { ErrorCode = "Шаблонов не найдено" };
				LogHelper.Info(typeof(BlockManager), (string.Format("GetBlocks ispreview={2}, ErrorDescription={0}, ErrorCode={1}",
						error.ErrorDescription, error.ErrorCode, isPreview)));
			}

			if (error != null) {
				bResponse.Error = error;
				bResponse.Status = StatusEnum.Error;
			} else {
				bResponse.Status = StatusEnum.Success;
				bResponse.Blocks =blocks;
			}
			return bResponse;
		}
	}
}