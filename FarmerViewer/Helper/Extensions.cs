using Newtonsoft.Json;

namespace Helper
{
    public static class Extensions
    {
        /// <summary>
        /// Converts to type.
        /// </summary>
        /// <typeparam name="TReturn">The type of the return.</typeparam>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public static TReturn ToType<TReturn>(this object model)
        {
            if (model == null)
                return default(TReturn);

            var jsonString = JsonConvert.SerializeObject(model, new JsonSerializerSettings()
            {
                //PreserveReferencesHandling = PreserveReferencesHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            });
            return JsonConvert.DeserializeObject<TReturn>(jsonString);
        }

    }
}
