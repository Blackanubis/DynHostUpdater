using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DynHostUpdater.Helpers
{
    /// <summary>
    ///     For set private properties
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Serialization.DefaultContractResolver" />
    public class JsonPrivateSetterContractResolver : DefaultContractResolver
    {
        #region Methods

        /// <summary>
        ///     Creates a <see cref="JsonProperty" /> for the given <see cref="MemberInfo" />.
        /// </summary>
        /// <param name="member">The member to create a <see cref="JsonProperty" /> for.</param>
        /// <param name="memberSerialization">The member's parent <see cref="MemberSerialization" />.</param>
        /// <returns>
        ///     A created <see cref="JsonProperty" /> for the given <see cref="MemberInfo" />.
        /// </returns>
        protected override JsonProperty CreateProperty(
            MemberInfo member,
            MemberSerialization memberSerialization)
        {
            var jProperty = base.CreateProperty(member, memberSerialization);
            if (jProperty.Writable) return jProperty;

            jProperty.Writable = member.IsPropertyWithSetter();

            return jProperty;
        }

        #endregion
    }

    /// <summary>
    ///     JsonPrivateSetterCamelCasePropertyNamesContractResolver
    /// </summary>
    /// <seealso cref="CamelCasePropertyNamesContractResolver" />
    public class JsonPrivateSetterCamelCasePropertyNamesContractResolver
        : CamelCasePropertyNamesContractResolver
    {
        #region Methods

        /// <summary>
        ///     Creates a <see cref="JsonProperty" /> for the given <see cref="MemberInfo" />.
        /// </summary>
        /// <param name="member">The member to create a <see cref="JsonProperty" /> for.</param>
        /// <param name="memberSerialization">The member's parent <see cref="MemberSerialization" />.</param>
        /// <returns>
        ///     A created <see cref="JsonProperty" /> for the given <see cref="MemberInfo" />.
        /// </returns>
        protected override JsonProperty CreateProperty(
            MemberInfo member,
            MemberSerialization memberSerialization)
        {
            var jProperty = base.CreateProperty(member, memberSerialization);
            if (jProperty.Writable) return jProperty;

            jProperty.Writable = member.IsPropertyWithSetter();

            return jProperty;
        }

        #endregion
    }

    internal static class MemberInfoExtensions
    {
        #region Methods

        /// <summary>
        ///     Determines whether [is property with setter].
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>
        ///     <c>true</c> if [is property with setter] [the specified member]; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsPropertyWithSetter(this MemberInfo member)
        {
            var property = member as PropertyInfo;
            return property?.SetMethod != null;
        }

        #endregion
    }
}