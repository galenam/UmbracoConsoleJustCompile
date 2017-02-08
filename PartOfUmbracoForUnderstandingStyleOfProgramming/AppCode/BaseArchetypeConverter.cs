using System;
using System.Collections;
using System.Collections.Generic;
using UmbracoCMS.AppCode.FakeClassLibrary;
using UmbracoCMS.AppCode.Interfaces;

namespace UmbracoCMS.AppCode
{
	/// <summary>
	/// Базовый класс для работы с компонентом Archetype
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class BaseArchetypeConverter<T>
	{
		#region Properties

		/// <summary>
		/// Обрабатываемый узел: опубликованный или неопубликованный
		/// </summary>
		protected IUnifiedContent<T> _unifiedContent;
		public IUnifiedContent<T> UnifiedContent
		{
			get { return _unifiedContent; }
		}

		private readonly Dictionary<string, string> _errorMessages = new Dictionary<string, string>
		                                                    {
			                                                    {"NoObject", "Umbraco: нет страницы с таким контекстом"},
			                                                    {"NoField", "Umbraco: не найдено поле с таким именем "}
		                                                    };
		protected virtual Dictionary<string, string> ErrorMessages
		{
			get { return _errorMessages; }

		}

		#endregion

		public BaseArchetypeConverter(IUnifiedContent<T> content)
		{
			_unifiedContent = content;
		}

		protected D GetPropertyValue<D>(T contentPage, string propertyName)
		{
			return UnifiedContent.GetPropertyValue<D>(contentPage, propertyName, false);
		}

		/// <summary>
		/// Получить значение archetype, преобразовать в возвращаемый тип
		/// </summary>
		/// <typeparam name="Q">Тип возвращаемого значения (во что преобразовывать)</typeparam>
		/// <param name="content">Страница</param>
		/// <param name="fieldName">Поле archetype</param>
		/// <param name="error">Для возврата информации об ошибке</param>
		/// <param name="ConvertFromArchetypeModelToT">Преобразование archetype к возвращаемому типу</param>
		/// <returns></returns>
		protected Q GetObject<Q>(T content, string fieldName, ref ErrorInfo error,
			Func<string, Q> ConvertFromArchetypeModelToT)
		{
			if (EqualityComparer<T>.Default.Equals(content, default(T))) {
				error = new ErrorInfo { ErrorCode = ErrorMessages["NoObject"] };
				return default(Q);
			}
			if (UnifiedContent.IsPropertyExist(fieldName, content)) {
				var propValue = GetPropertyValue<string>(content, fieldName);
				if (propValue == null) {
					return default(Q);
				}
				return ConvertFromArchetypeModelToT(propValue);
			}
			error = new ErrorInfo { ErrorCode = ErrorMessages["NoField"] + fieldName };
			return default(Q);
		}


	}
}