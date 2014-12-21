using System.Activities.Presentation.Metadata;
using EmguWF.Activities.Designers;

namespace EmguWF.Activities
{
    public sealed class RegisterMetadata : IRegisterMetadata
    {
        public void Register()
        {
            RegisterAll();
        }

        public static void RegisterAll()
        {
            var builder = new AttributeTableBuilder();
            
            // Register each type
            ImageLoaderDesigner.RegisterMetadata(builder);
            BgrErodeDesigner.RegisterMetadata(builder);
            GrayDilateDesigner.RegisterMetadata(builder);
            GrayErodeDesigner.RegisterMetadata(builder);
            GrayBinaryThresholdDesigner.RegisterMetadata(builder);
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}
