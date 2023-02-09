using System;
using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Registers font and color definitions in Visual Studio.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ProvideFontsAndColorsAttribute : ProvideServiceAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProvideFontsAndColorsAttribute"/> class. 
        /// The <paramref name="providerType"/> will also be provided as a service.
        /// </summary>
        /// <param name="providerType">The type of the <see cref="BaseFontAndColorProvider"/> implementation to provide.</param>
        public ProvideFontsAndColorsAttribute(Type providerType)
            : base(providerType, "Services")
        {
            ProviderType = providerType;
        }

        /// <summary>
        /// The <see cref="BaseFontAndColorProvider"/> implementation to provide.
        /// </summary>
        public Type ProviderType { get; }

        /// <inheritdoc/>
        public override void Register(RegistrationContext context)
        {
            if (context is not null)
            {
                foreach (Type categoryType in BaseFontAndColorProvider.GetCategoryTypes(ProviderType))
                {
                    using (Key key = context.CreateKey($"FontAndColors\\{categoryType.FullName}"))
                    {
                        key.SetValue("Category", categoryType.GUID.ToString("B"));
                        key.SetValue("Package", ProviderType.GUID.ToString("B"));
                    }
                }
            }

            base.Register(context);
        }

        /// <inheritdoc/>
        public override void Unregister(RegistrationContext context)
        {
            if (context is not null)
            {
                foreach (Type categoryType in BaseFontAndColorProvider.GetCategoryTypes(ProviderType))
                {
                    context.RemoveKey($"FontAndColors\\{categoryType.FullName}");
                }
            }

            base.Unregister(context);
        }
    }
}
