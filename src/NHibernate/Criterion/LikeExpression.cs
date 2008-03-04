using System;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using System.Collections.Generic;
using NHibernate.Util;

namespace NHibernate.Criterion
{
	/// <summary>
	/// An <see cref="ICriterion"/> that represents an "like" constraint.
	/// </summary>
	/// <remarks>
	/// The case sensitivity depends on the database settings for string 
	/// comparisons.  Use <see cref="InsensitiveLikeExpression"/> if the
	/// string comparison should not be case sensitive.
	/// </remarks>
	[Serializable]
	public class LikeExpression : ICriterion
	{
		private readonly string propertyName;
		private readonly string value;
		private char? escapeChar;
		private readonly bool ignoreCase;

		public LikeExpression(string propertyName, string value, char? escapeChar, bool ignoreCase)
		{
			this.propertyName = propertyName;
			this.value = value;
			this.escapeChar = escapeChar;
			this.ignoreCase = ignoreCase;
		}

		public LikeExpression(string propertyName, string value)
			: this(propertyName, value, null, false)
		{
		}

		public LikeExpression(string propertyName, string value, MatchMode matchMode)
			: this(propertyName, matchMode.ToMatchString(value))
		{
		}

		public LikeExpression(string propertyName, string value, MatchMode matchMode, char? escapeChar, bool ignoreCase)
			: this(propertyName, matchMode.ToMatchString(value), escapeChar, ignoreCase)
		{
		}

		#region ICriterion Members

		public SqlString ToSqlString(ICriteria criteria, ICriteriaQuery criteriaQuery, IDictionary<string, IFilter> enabledFilters)
		{
			string[] columns = criteriaQuery.GetColumnsUsingProjection(criteria, propertyName);
			if (columns.Length != 1)
				throw new HibernateException("Like may only be used with single-column properties");

			SqlStringBuilder lhs = new SqlStringBuilder(6);

			if(ignoreCase)
			{
				Dialect.Dialect dialect = criteriaQuery.Factory.Dialect;
				lhs.Add(dialect.LowercaseFunction).Add(StringHelper.OpenParen).Add(columns[0]).Add(
					StringHelper.ClosedParen);
			}
			else 
				lhs.Add(columns[0]);
			lhs.Add(" like ").AddParameter();
			if (escapeChar.HasValue)
				lhs.Add(" escape '" + escapeChar + "'");
			return lhs.ToSqlString();
		}

		public TypedValue[] GetTypedValues(ICriteria criteria, ICriteriaQuery criteriaQuery)
		{
			return new TypedValue[] {criteriaQuery.GetTypedValue(criteria, propertyName, ignoreCase ? value.ToLower() : value)};
		}

		#endregion
	}
}