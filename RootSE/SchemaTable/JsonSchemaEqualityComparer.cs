#if false
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace RootSE.SchemaTable
{
	sealed class JsonSchemaEqualityComparer
	{
		public static bool equals(JsonSchema left, JsonSchema right)
		{
			if (left == right)
				return true;
			if (left == null || right == null)
				return false;

			if (!Equals(left.Id, right.Id))
				return false;
			if (!Equals(left.Title, right.Title))
				return false;
			if (!Equals(left.Options, right.Optional))
				return false;
			if (!Equals(left.ReadOnly, right.ReadOnly))
				return false;
			if (!Equals(left.Hidden, right.Hidden))
				return false;
			if (!Equals(left.Transient, right.Transient))
				return false;
			if (!Equals(left.Description, right.Description))
				return false;
			if (!Equals(left.Type, right.Type))
				return false;
			if (!Equals(left.Pattern, right.Pattern))
				return false;
			if (!Equals(left.MinimumLength, right.MinimumLength))
				return false;
			if (!Equals(left.MaximumLength, right.MaximumLength))
				return false;
			if (!Equals(left.MaximumDecimals, right.MaximumDecimals))
				return false;
			if (!Equals(left.Minimum, right.Minimum))
				return false;
			if (!Equals(left.Maximum, right.Maximum))
				return false;
			if (!Equals(left.MinimumItems, right.MinimumItems))
				return false;
			if (!Equals(left.MaximumItems, right.MaximumItems))
				return false;
			if (!equalsItems(left.Items, right.Items))
				return false;
			if (!equalsProperties(left.Properties, right.Properties))
				return false;
			if (!equals(left.AdditionalProperties, right.AdditionalProperties))
				return false;
			if (!Equals(left.AllowAdditionalProperties, right.AllowAdditionalProperties))
				return false;
			if (!Equals(left.Requires, right.Requires))
				return false;
			if (!equalsIdentity(left.Identity, right.Identity))
				return false;
			if (!equalsEnum(left.Enum, right.Enum))
				return false;
			if (!equalsOptions(left.Options, right.Options))
				return false;
			if (!Equals(left.Disallow, right.Disallow))
				return false;
			if (!equalsToken(left.Default, right.Default))
				return false;
			if (!equals(left.Extends, right.Extends))
				return false;
			if (!Equals(left.Format, right.Format))
				return false;

			return true;
		}

		static bool equalsItems(IList<JsonSchema> left, IList<JsonSchema> right)
		{
			if (left == right)
				return true;
			if (left == null || right == null)
				return false;

			if (left.Count != right.Count)
				return false;

			for (int i = 0; i != left.Count; ++i)
			{
				if (!equals(left[i], right[i]))
					return false;
			}

			return true;
		}

		static bool equalsProperties(IDictionary<string, JsonSchema> left, IDictionary<string, JsonSchema> right)
		{
			if (left == right)
				return true;
			if (left == null || right == null)
				return false;

			if (left.Count != right.Count)
				return false;

			foreach (var l in left)
			{
				JsonSchema r;
				if (!right.TryGetValue(l.Key, out r))
					return false;

				if (!equals(l.Value, r))
					return false;
			}

			return true;
		}

		static bool equalsIdentity(IList<string> left, IList<string> right)
		{
			if (left == right)
				return true;
			if (left == null || right == null)
				return false;

			if (left.Count != right.Count)
				return false;

			for (int i = 0; i != left.Count; ++i)
			{
				if (!Equals(left[i], right[i]))
					return false;
			}

			return true;
		}

		static bool equalsEnum(IList<JToken> left, IList<JToken> right)
		{
			if (left == right)
				return true;
			if (left == null || right == null)
				return false;

			// there is an equality comparer existing for JToken, but we can eliminate order properly, 
			// so we go for the strings and hope for the best.

			var lh = new HashSet<JToken>(left, JToken.EqualityComparer);
			return lh.SetEquals(right);
		}

		static bool equalsOptions(IDictionary<JToken, string> left, IDictionary<JToken, string> right)
		{
			if (left == right)
				return true;
			if (left == null || right == null)
				return false;

			if (left.Count != right.Count)
				return false;

			foreach (var l in left)
			{
				string r;
				if (!right.TryGetValue(l.Key, out r))
					return false;

				if (!Equals(l.Value, r))
					return false;
			}

			return true;
		}

		static bool equalsToken(JToken left, JToken right)
		{
			return JToken.EqualityComparer.Equals(left, right);
		}
	}
}
#endif
