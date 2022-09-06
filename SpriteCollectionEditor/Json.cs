using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TML {
	public static class Json {
		public static string SerializeStruct<T>(T source, bool formatted) where T : struct {
			return JsonConvert.SerializeObject(source, (formatted ? Formatting.Indented : Formatting.None));
		}
		public static string SerializeClass<T>(T? source, bool formatted) where T : class {
			return JsonConvert.SerializeObject(source, (formatted ? Formatting.Indented : Formatting.None));
		}

		public static T DeserializeStruct<T>(string json) where T : struct {
			return JsonConvert.DeserializeObject<T>(json);
		}
		public static bool TryDeserializeStruct<T>(string json, out T result) where T : struct {
			try {
				result = DeserializeStruct<T>(json);
				return true;
			} catch (Exception) {
				result = default;
				return false;
			}
		}
		
		public static T? DeserializeClass<T>(string json) where T : class {
			return JsonConvert.DeserializeObject<T>(json);
		}
		public static bool TryDeserializeClass<T>(string json, out T? result) where T : class {
			try {
				result = DeserializeClass<T>(json);
				return true;
			} catch (Exception) {
				result = default;
				return false;
			}
		}
	}
}
